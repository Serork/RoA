using RoA.Core.Defaults;

using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class MenhirDecoration : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(20, 38);

        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.MenhirDecoration>());
    }
}
