using RoA.Content.Tiles.Solid.Backwoods;

using System;

using Terraria.ModLoader;

namespace RoA.Common.Tiles;

sealed class TileCount : ModSystem {
    public int BackwoodsTiles { get; private set; }

    public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
        BackwoodsTiles = tileCounts[ModContent.TileType<BackwoodsGrass>()] +
                         tileCounts[ModContent.TileType<BackwoodsStone>()] +
                         tileCounts[ModContent.TileType<BackwoodsGreenMoss>()];
    }
}