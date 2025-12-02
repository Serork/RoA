using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Solid;

sealed class GrimstoneBrick : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
        //ItemID.Sets.SortingPriorityMaterials[Item.type] = ItemID.GrayBrick;
    }

    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.width = 16;
        Item.height = 16;

        Item.createTile = ModContent.TileType<Tiles.Solid.BackwoodsStoneBrick>();
    }
}