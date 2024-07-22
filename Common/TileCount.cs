using Terraria.ModLoader;

using System;
using RoA.Content.Tiles.Solid.Backwoods;

namespace RoA.Common;

[Autoload(Side = ModSide.Client)]
sealed class TileCount : ModSystem {
	public int BackwoodsTiles { get; private set; }

	public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts) {
        BackwoodsTiles = tileCounts[ModContent.TileType<BackwoodsGrass>()] +
						 tileCounts[ModContent.TileType<BackwoodsStone>()] + 
					     tileCounts[ModContent.TileType<BackwoodsGreenMoss>()] +
                         tileCounts[ModContent.TileType<LivingElderwood>()] +
                         tileCounts[ModContent.TileType<LivingElderwoodlLeaves>()];
	}
}