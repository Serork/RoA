using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Station;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class AltarPlacer : ModItem {
    public override void SetDefaults() {
        int width = 10; int height = width;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = false;

        Item.createTile = ModContent.TileType<OvergrownAltar>();
    }
}