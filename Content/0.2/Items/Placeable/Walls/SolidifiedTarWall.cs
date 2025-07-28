using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Walls;

sealed class SolidifiedTarWall : ModItem {
    public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 7;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.createWall = ModContent.WallType<Tiles.Walls.SolidifiedTarWall>();
        Item.width = 24;
        Item.height = 24;
    }
}