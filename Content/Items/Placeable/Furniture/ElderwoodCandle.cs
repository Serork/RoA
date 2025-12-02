using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodCandle : ModItem {
    public override void SetDefaults() {
        int width = 12; int height = 20;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
        Item.value = Item.sellPrice(0, 0, 0, 60);
        Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodCandle>();
    }
}
