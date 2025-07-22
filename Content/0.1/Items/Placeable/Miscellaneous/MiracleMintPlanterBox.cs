using RoA.Content.Tiles.Crafting;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class MiracleMintPlanterBox : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<PlanterBoxes>();
        Item.placeStyle = 0;
        Item.width = 28;
        Item.height = 24;
        Item.value = Item.sellPrice(0, 0, 0, 20);
    }
}