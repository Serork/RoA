using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Prefixes;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.PreHardmode;

sealed class CrimsonRod : NatureItem {
    public override void SetStaticDefaults() {
        PrefixLegacy.ItemSets.MagicAndSummon[Type] = true;
    }

    protected override void SafeSetDefaults() {
        Item.damage = 2;
        Item.useStyle = 1;
        Item.shootSpeed = 12f;
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Nature.BloodCloudMoving>();
        Item.width = 26;
        Item.height = 28;
        Item.UseSound = SoundID.Item8;
        Item.useAnimation = 24;
        Item.useTime = 24;
        Item.rare = 1;
        Item.noMelee = true;
        Item.knockBack = 0f;

        Item.value = Item.sellPrice(0, 1, 50, 0);

        NatureWeaponHandler.SetPotentialDamage(Item, 7);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.05f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        if (player.whoAmI == Main.myPlayer) {
            Vector2 mousePosition = player.GetViableMousePosition();
            Projectile.NewProjectile(source, position.X, position.Y, velocity.X, velocity.Y, type, damage, knockback, player.whoAmI, mousePosition.X, mousePosition.Y);
        }

        return false;
    }
}