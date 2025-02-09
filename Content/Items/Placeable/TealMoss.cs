using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Solid.Backwoods;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable;

sealed class TealMoss : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<BackwoodsGreenMoss>());

        int width = 20; int height = 18;
        Item.Size = new Vector2(width, height);
    }
}
