using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Walls;

sealed class ElderwoodFence : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 400;
    }

    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 7;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.width = 28;
        Item.height = 30;
        Item.createWall = ModContent.WallType<Tiles.Walls.ElderwoodFence>();
    }
}
