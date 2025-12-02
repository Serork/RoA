using RoA.Content.Tiles.LiquidsSpecific;
using RoA.Content.Tiles.Solid.Backwoods;

using System;

using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class TileCount : ModSystem {
    public ushort BackwoodsTiles { get; private set; }
    public ushort TarpitTiles { get; private set; }

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
        BackwoodsTiles = (ushort)(tileCounts[ModContent.TileType<BackwoodsGrass>()] +
                                  tileCounts[ModContent.TileType<BackwoodsStone>()] +
                                  tileCounts[ModContent.TileType<BackwoodsGreenMoss>()]);

        TarpitTiles = (ushort)(tileCounts[ModContent.TileType<SolidifiedTar>()] + 
                               0);
    }
}