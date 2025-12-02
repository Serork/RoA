using RoA.Core.Defaults;

using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class NixieTube : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.NixieTube>());

        Item.SetSizeValues(20, 32);
    }
}
