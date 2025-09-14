using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Items.Special;
using RoA.Content.Projectiles.Friendly.Magic;
using RoA.Core;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask(shouldApplyItemAlpha: true)]
sealed class RodOfTheDragonfire : Rod {
    protected override Color? LightingColor => new(255, 154, 116);

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;

        ItemID.Sets.ShimmerTransformToItem[Type] = (ushort)ModContent.ItemType<SphereOfPyre>();

        //ItemSwapSystem.SwapToOnRightClick[Type] = (ushort)ModContent.ItemType<SphereOfPyre>();
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 42; int height = 42;
        Item.Size = new Vector2(width, height);

        Item.useTime = Item.useAnimation = 26;
        Item.autoReuse = true;

        Item.damage = 26;
        Item.knockBack = 2f;

        Item.crit = 4;

        Item.mana = 14;

        Item.value = Item.sellPrice(0, 3, 50, 0);
        Item.rare = ItemRarityID.Orange;

        Item.UseSound = new SoundStyle(ResourceManager.ItemSounds + "ScreechCast") { Volume = 0.75f, PitchVariance = 0.1f };

        Item.shoot = ModContent.ProjectileType<Hellbat>();
        Item.shootSpeed = 5f;
    }

    public override void ModifyShootCustom(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 newVelocity = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
        position += newVelocity * -2f;
        position += new Vector2(player.direction == -1 ? 4f : -4f, -6f * player.direction).RotatedBy(newVelocity.ToRotation());
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        int amount = 2;
        Vector2 dustPosition = position;
        for (int i = -amount + 1; i < amount; i++) {
            Vector2 vector2 = Utils.RotatedBy(velocity, (double)(i / 10f)) * Main.rand.NextFloat(0.75f, 1.35f);
            if (Main.rand.NextBool()) {
                Dust dust = Dust.NewDustDirect(dustPosition, 0, 0, 6, velocity.X, velocity.Y, 100, default, Main.rand.NextFloat(0.5f, 2f));
                dust.velocity.X *= Main.rand.NextFloat(0f, 3f);
                dust.velocity.Y *= Main.rand.NextFloat(0f, 3f);
                dust.fadeIn = Main.rand.NextFloat(1f, 2f);
                dust.noGravity = true;
            }
        }
        if (base.Shoot(player, source, position, velocity, type, damage, knockback)) {
            for (int i = -amount + 1; i < amount; i++) {
                Vector2 vector2 = Utils.RotatedBy(velocity, (double)(i / 10f)) * Main.rand.NextFloat(0.75f, 1.35f);
                Vector2 spawnPosition = position + vector2.SafeNormalize(Vector2.Zero) * -4f + vector2 + vector2 + new Vector2(0f, 12f * player.direction).RotatedBy(velocity.ToRotation());
                Projectile.NewProjectileDirect(source, spawnPosition, vector2, type, damage, knockback, player.whoAmI);
            }
        }

        return false;
    }
}