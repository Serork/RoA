using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class TarfallBlock : ModItem {
    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<Tiles.Solid.TarfallBlock>();
        Item.width = 16;
        Item.height = 16;
    }
}
