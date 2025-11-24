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
sealed class RodOfTheStream : Rod {
    protected override Color? LightingColor => new(57, 136, 232);

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Rod of the Stream");
        // Tooltip.SetDefault("Casts a water spit which splits into two when hits enemies\n'Forged with Aqua'");

        Item.ResearchUnlockCount = 1;

        //ItemID.Sets.ShimmerTransformToItem[Type] = (ushort)ModContent.ItemType<SphereOfStream>();

        //ItemSwapSystem.SwapToOnRightClick[Type] = (ushort)ModContent.ItemType<SphereOfStream>();
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 42; int height = 42;
        Item.Size = new Vector2(width, height);

        Item.useTime = Item.useAnimation = 40;
        Item.autoReuse = false;

        Item.damage = 41;
        Item.knockBack = 6f;

        Item.mana = 25;

        Item.value = Item.sellPrice(0, 3, 50, 0);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = new SoundStyle(ResourceManager.ItemSounds + "Splash") { Volume = 0.9f, Pitch = -0.2f, PitchVariance = 0.2f };

        Item.shoot = ModContent.ProjectileType<WaterStream>();
        Item.shootSpeed = 6f;
    }

    public override void ModifyShootCustom(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 newVelocity = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
        position += newVelocity * -2f;
        position += new Vector2(6f, 0f * player.direction).RotatedBy(newVelocity.ToRotation());
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        /*if (base.Shoot(player, source, position, velocity, type, damage, knockback))*/
        {
            int amount = 2;
            for (int i = 0; i < 20; i++) {
                Vector2 dustPosition = position;
                dustPosition += new Vector2(4f, 2f * player.direction).RotatedBy(velocity.ToRotation());
                Vector2 direction = velocity;
                int dust = Dust.NewDust(dustPosition - Vector2.One * 10, 20, 20, DustID.DungeonWater, direction.X * Main.rand.NextFloat(), direction.Y * Main.rand.NextFloat(), 100, default(Color), Main.rand.NextFloat(0.8f, 1.2f));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale *= Main.rand.NextFloat(1.25f, 1.5f);
                Main.dust[dust].velocity *= 0.7f;
                Main.dust[dust].velocity *= Main.rand.NextFloat(0.1f, 1f);
            }
            for (int i = -amount + 1; i < amount; i++) {
                Vector2 vector2 = Utils.RotatedBy(velocity, i / 2d);
                Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity * 40f, type, damage, knockback, player.whoAmI, 0f, -5f);
                Projectile.NewProjectileDirect(source, position + vector2, vector2, type, damage, knockback, player.whoAmI, 0f, projectile.whoAmI);
            }
        }

        return false;
    }
}