using Microsoft.Xna.Framework;

using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodBathtub : ModItem {
    public override void SetDefaults() {
        int width = 30; int height = 22;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
        Item.value = 300;
        Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodBathtub>();
    }
}
