using RoA.Common.Tiles;
using Terraria.ID;
using Terraria.ObjectData;

namespace RoA.Content.Tiles.Miscellaneous;

abstract class TulipTileBase : SimpleTileBaseToGenerateOverTime {
    public sealed override string Texture => GetType().Namespace.Replace('.', '/') + "/Tulips";

    public sealed override void SetStaticDefaults() {
        TileID.Sets.SwaysInWindBasic[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.AnchorValidTiles = AnchorValidTiles;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.Style = 0;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        base.SetStaticDefaults();
    }

    public sealed override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
        offsetY = -14;
        height = 32;
    }
}