using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class BackwoodsWaterFountain : ModItem {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Million Dollar Painting");
        //Tooltip.SetDefault("'VFD'");
        CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
    }

    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<Tiles.Decorations.BackwoodsWaterFountain>();
        Item.placeStyle = 0;
        Item.width = 22;
        Item.height = 34;
        Item.value = Item.buyPrice(0, 4);
        Item.rare = 1;
    }
}