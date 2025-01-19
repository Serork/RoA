using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Magic;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask]
sealed class RodOfTheStream : Rod {
    protected override Color? LightingColor => new(57, 136, 232);

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Rod of the Stream");
        // Tooltip.SetDefault("Casts a water spit which splits into two when hits enemies\n'Forged with Aqua'");

        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        base.SetDefaults();

        int width = 52; int height = 48;
        Item.Size = new Vector2(width, height);

        Item.useTime = Item.useAnimation = 24;
        Item.autoReuse = false;

        Item.damage = 28;
        Item.knockBack = 6f;

        Item.mana = 11;

        Item.value = Item.buyPrice(gold: 1, silver: 10);
        Item.rare = ItemRarityID.Orange;
        Item.UseSound = SoundID.Item87;

        Item.shoot = ModContent.ProjectileType<WaterStream>();
        Item.shootSpeed = 6f;
    }

    public override void ModifyShootCustom(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 newVelocity = Utils.SafeNormalize(new Vector2(velocity.X, velocity.Y), Vector2.Zero);
        position -= newVelocity * 4.25f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (base.Shoot(player, source, position, velocity, type, damage, knockback)) {
            int amount = 2;
            for (int i = 0; i < 20; i++) {
                Vector2 dustPosition = player.Center + velocity.SafeNormalize(Vector2.Zero) * 52.5f;
                 dustPosition += new Vector2(0f + (player.direction == -1 ? 6f : 0f), 2f * player.direction).RotatedBy(velocity.ToRotation());
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