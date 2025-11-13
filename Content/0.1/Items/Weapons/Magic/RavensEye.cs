using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Magic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Magic;

[AutoloadGlowMask(185, 185, 185)]
sealed class RavensEye : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Raven's Eye");
        // Tooltip.SetDefault("");

        Item.ResearchUnlockCount = 1;

        Item.staff[Item.type] = true;
    }

    public override void SetDefaults() {
        int width = 38; int height = 40;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 45;
        Item.autoReuse = false;

        Item.DamageType = DamageClass.Magic;
        Item.damage = 31;
        Item.knockBack = 2f;

        Item.noMelee = true;
        Item.channel = true;
        Item.mana = 15;

        Item.rare = ItemRarityID.Green;
        Item.UseSound = SoundID.Item105;

        Item.shoot = ModContent.ProjectileType<BloodyFeather>();
        Item.shootSpeed = 12f;

        Item.value = Item.sellPrice(0, 1, 50, 0);
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 newVelocity = new Vector2(velocity.X, velocity.Y).SafeNormalize(Vector2.Zero);
        position += newVelocity * 40;
        position += new Vector2(-newVelocity.Y, newVelocity.X) * (-10f * player.direction);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Vector2 funnyOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 5f;
        position += funnyOffset - new Vector2(player.direction == -1 ? 0f : 8f, -2f * player.direction).RotatedBy(funnyOffset.ToRotation());
        for (int i = 0; i < 5; i++) {
            if (Main.rand.NextBool()) {
                bool flag = Main.rand.NextBool();
                int dust = Dust.NewDust(position, 0, 0, flag ? 60 : 96, 0, 0.5f, 0, default, (!flag ? 1.5f : 2.3f) + 0.5f * Main.rand.NextFloat());
                Main.dust[dust].noGravity = true;
            }
        }
        for (int i = 0; i < 3; i++) {
            Projectile.NewProjectile(source, new Vector2(player.Center.X + Main.rand.NextFloat(-60, 60), player.Center.Y + Main.rand.NextFloat(-30, 20)), new Vector2(velocity.X, velocity.Y), type, damage, knockback, player.whoAmI);
        }
        return false;
    }
}