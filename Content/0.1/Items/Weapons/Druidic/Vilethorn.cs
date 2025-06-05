using RoA.Common.Configs;
using RoA.Common.Druid;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Druidic;

sealed class Vilethorn : NatureItem {
    public override string Texture => ModContent.GetInstance<RoAClientConfig>().VanillaResprites ? base.Texture : $"Terraria/Images/Item_{ItemID.Vilethorn}";

    protected override void SafeSetDefaults() {
        Item.damage = 2;
        Item.useStyle = 1;
        Item.shootSpeed = 32f;
        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Druidic.Vilethorn>();
        Item.width = 26;
        Item.height = 28;
        Item.useAnimation = 32;
        Item.useTime = 32;
        Item.rare = 1;
        Item.noMelee = true;
        Item.knockBack = 1f;

        Item.value = Item.sellPrice(0, 1, 50, 0);

        NatureWeaponHandler.SetPotentialDamage(Item, 9);
        NatureWeaponHandler.SetFillingRateModifier(Item, 0.06f);
    }
}