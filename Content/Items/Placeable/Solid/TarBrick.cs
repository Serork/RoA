using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Solid;

sealed class TarBrick : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile((ushort)ModContent.TileType<Tiles.Solid.TarBrick>(), 0);
    }
}
