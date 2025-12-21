using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Station;

sealed class OvergrownAltar : ModItem {
    public override void SetDefaults() {
        int width = 40; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = false;

        Item.createTile = ModContent.TileType<Tiles.Station.OvergrownAltar>();
    }
}
