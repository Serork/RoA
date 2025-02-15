using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class HedgehogTerrarium : ModItem {
    public override void SetStaticDefaults() {

    }

    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.width = 32;
        Item.height = 32;
        Item.createTile = ModContent.TileType<Tiles.Decorations.HedgehogTerrarium>();
    }
}