using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Recipes;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Defaults;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Nature.Hardmode;

sealed class IceRod : NatureItem, IRecipeDuplicatorItem {
    ushort[] IRecipeDuplicatorItem.SourceItemTypes => [(ushort)ItemID.IceRod];

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(32);

        Item.rare = ItemRarityID.LightRed;
        Item.damage = 28;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.shootSpeed = 12f;
        Item.shoot = ModContent.ProjectileType<IceBlock>();
        Item.UseSound = SoundID.Item28;
        Item.useAnimation = Item.useTime = 30;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.knockBack = 0f;
        Item.value = Item.buyPrice(0, 50);
        Item.knockBack = 2f;

        NatureWeaponHandler.SetPotentialDamage(Item, 50);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.05f);
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
        Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI, Player.tileTargetX, Player.tileTargetY);

        return false;
    }
}
