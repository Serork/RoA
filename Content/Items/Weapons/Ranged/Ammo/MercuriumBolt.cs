using RoA.Content.Projectiles.Friendly.Ranged.Ammo;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Weapons.Ranged.Ammo;

sealed class MercuriumBolt : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
    }

    public override void SetDefaults() {
        Item.shootSpeed = 4.3f;
        Item.shoot = ModContent.ProjectileType<MercuriumBoltProjectile>();
        Item.damage = 16;
        Item.width = 14;
        Item.height = 32;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.ammo = AmmoID.Arrow;
        Item.knockBack = 4.2f;
        Item.DamageType = DamageClass.Ranged;
        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 0, 1, 50);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Ammo;
    }
}