using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable;

sealed class JawTrap : ModItem {
    public override void SetDefaults() {
        int width = 30; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
        //Item.value = 300;
        Item.createTile = ModContent.TileType<Tiles.Miscellaneous.JawTrap>();

        Item.value = Item.sellPrice(0, 0, 20, 0);
    }
}
