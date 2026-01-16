using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Solid;

sealed class Grimstone : ModItem {
    public override void SetStaticDefaults() {
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
        //ItemID.Sets.SortingPriorityMaterials[Item.type] = ItemID.StoneBlock;

        ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ItemID.StoneBlock, 1);
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

        Item.createTile = ModContent.TileType<Tiles.Solid.Backwoods.BackwoodsStone>();
    }
}