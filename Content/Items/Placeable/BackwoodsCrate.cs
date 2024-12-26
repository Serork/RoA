using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable;

sealed class BackwoodsCrate : ModItem {

    public override void SetStaticDefaults() {
        ItemID.Sets.IsFishingCrate[Type] = true;

        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 5;
    }

    public override void SetDefaults() {
        Item.width = 34;
        Item.height = 34;
        Item.rare = 2;
        Item.maxStack = Item.CommonMaxStack;
        Item.createTile = ModContent.TileType<Tiles.Decorations.BackwoodsCrate>();
        Item.useAnimation = 15;
        Item.useTime = 15;
        Item.autoReuse = true;
        Item.useStyle = 1;
        Item.consumable = true;
        Item.value = Item.sellPrice(0, 1);
    }

    public override bool CanRightClick() {
        return true;
    }

    public override void ModifyItemLoot(ItemLoot itemLoot) {

    }
}