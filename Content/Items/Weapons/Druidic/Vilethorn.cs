using RoA.Common.Druid;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic;

sealed class Vilethorn : NatureItem {
    protected override void SafeSetDefaults() {
        Item.damage = 2;
        Item.useStyle = 1;
        Item.shootSpeed = 32f;
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Druidic.Vilethorn>();
        Item.width = 26;
        Item.height = 28;
        Item.useAnimation = 28;
        Item.useTime = 28;
        Item.rare = 1;
        Item.noMelee = true;
        Item.knockBack = 1f;
        Item.value = Item.sellPrice(0, 1, 50);

        NatureWeaponHandler.SetPotentialDamage(Item, 7);
        NatureWeaponHandler.SetFillingRate(Item, 0.2f);
    }
}