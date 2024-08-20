using Microsoft.Xna.Framework;
using RoA.Common.Tiles;
using RoA.Core.Utility;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class BackwoodsGrass : ModTile {
	public override void SetStaticDefaults() {
        TileHelper.Solid(Type);

        TileID.Sets.Grass[Type] = true;
		TileID.Sets.CanBeDugByShovel[Type] = true;
		TileID.Sets.NeedsGrassFraming[Type] = true;
		TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
		TileID.Sets.NeedsGrassFramingDirt[Type] = ModContent.TileType<BackwoodsDirt>();
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TransformTileSystem.OnKillNormal[Type] = false;
        TransformTileSystem.ReplaceToOnKill[Type] = TileID.Dirt;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
        AddMapEntry(new Color(38, 107, 57));
	}
}