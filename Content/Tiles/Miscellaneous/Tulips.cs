using Terraria.ID;
using Terraria.Localization;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

using Microsoft.Xna.Framework;

using RoA.Core.Utility;

namespace RoA.Content.Tiles.Miscellaneous;

sealed class SweepTulip : TulipTileBase {
    protected override int[] AnchorValidTiles => [TileID.JungleGrass];

    protected override Color MapColor => new(216, 78, 142);
}

sealed class ExoticTulip : TulipTileBase {
    protected override int[] AnchorValidTiles => [TileID.Sand];

    protected override Color MapColor => new(255, 165, 0);
}

sealed class WeepingTulip : TulipTileBase {
    protected override int[] AnchorValidTiles => [TileID.PinkDungeonBrick, TileID.GreenDungeonBrick, TileID.BlueDungeonBrick];

    protected override Color MapColor => new(0, 0, 255);
}

abstract class TulipTileBase : ModTile {
    protected virtual int[] AnchorValidTiles { get; }

    protected abstract Color MapColor { get; }

    public sealed override string Texture => GetType().Namespace.Replace('.', '/') + "/Tulips";

    public sealed override void SetStaticDefaults() {
        Main.tileFrameImportant[Type] = true;
        Main.tileCut[Type] = true;

        foreach (int mergeType in AnchorValidTiles) {
            TileHelper.MergeWith(Type, (ushort)mergeType);
        }

        TileObjectData.newTile.AnchorValidTiles = AnchorValidTiles;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        HitSound = SoundID.Grass;
        DustType = 2;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(MapColor, name);
    }
}