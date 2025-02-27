using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Content.Items.Materials;
using RoA.Content.Projectiles.Friendly.Druidic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic;

sealed class SapStream : NatureItem {
    protected override void SafeSetDefaults() {
        int width = 38; int height = width;
        Item.Size = new Vector2(width, height);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTime = Item.useAnimation = 20;
        Item.autoReuse = true;

        Item.noMelee = true;
        Item.knockBack = 2f;

        Item.damage = 2;
        NatureWeaponHandler.SetPotentialDamage(Item, 12);
        NatureWeaponHandler.SetFillingRate(Item, 0.5f);

        Item.value = Item.sellPrice(silver: 15);
        Item.rare = ItemRarityID.Blue;
        Item.UseSound = SoundID.Item20;

        Item.shootSpeed = 1f;
        Item.shoot = ModContent.ProjectileType<GalipotStream>();

        Item.staff[Item.type] = true;
    }

    public sealed override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
        Vector2 velocity2 = new Vector2(velocity.X, velocity.Y).SafeNormalize(Vector2.Zero);
        position += velocity2 * 40f;
        position += new Vector2(-velocity2.Y, velocity2.X) * (5f * player.direction);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (!Collision.CanHit(player.Center, 0, 0, position + Vector2.Normalize(velocity) * Item.width / 2f, 0, 0)) {
            return false;
        }

        if (base.Shoot(player, source, position, velocity, type, damage, knockback)) {
            Vector2 vector2 = (velocity + Utils.RotatedByRandom(velocity, 0.25f) * Main.rand.NextFloat(0.8f, 1.4f) * Main.rand.NextFloat(0.75f, 1.35f)) * Main.rand.NextFloat(1.25f, 2f);
            Projectile.NewProjectileDirect(source, position + vector2, vector2, type, damage, knockback, player.whoAmI);
        }

        return false;
    }
}