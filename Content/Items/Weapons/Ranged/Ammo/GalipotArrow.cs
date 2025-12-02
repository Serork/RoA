using RoA.Content.Projectiles.Friendly.Ranged.Ammo;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged.Ammo;

sealed class GalipotArrow : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
    }

    public override void SetDefaults() {
        Item.shootSpeed = 4.3f;
        Item.shoot = ModContent.ProjectileType<GalipotArrowProjectile>();
        Item.damage = 7;
        Item.width = 14;
        Item.height = 34;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.ammo = AmmoID.Arrow;
        Item.knockBack = 2.5f;
        Item.DamageType = DamageClass.Ranged;
        Item.rare = ItemRarityID.White;

        Item.value = Item.sellPrice(0, 0, 0, 8);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Ammo;
    }
}