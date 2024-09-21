using Microsoft.Xna.Framework;

using RoA.Common.GlowMasks;
using RoA.Content.Projectiles.Friendly.Magic;

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

        int width = 42; int height = width;
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
        position -= newVelocity * 2f;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (base.Shoot(player, source, position, velocity, type, damage, knockback)) {
            int amount = 2;
            for (int i = -amount + 1; i < amount; i++) {
                Vector2 vector2 = Utils.RotatedBy(velocity, i / 2d);
                Projectile projectile = Projectile.NewProjectileDirect(source, position, velocity * 40f, type, damage, knockback, player.whoAmI, 0f, -5f);
                Projectile.NewProjectileDirect(source, position + vector2, vector2, type, damage, knockback, player.whoAmI, 0f, projectile.whoAmI);
            }
        }

        return false;
    }
}