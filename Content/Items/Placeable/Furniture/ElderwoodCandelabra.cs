using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodCandelabra : ModItem {
    public override void SetDefaults() {
        int width = 22; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
        Item.value = 1500;
        Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodCandelabra>();
    }
}
