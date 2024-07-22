using Microsoft.Xna.Framework;

using RoA.Common;
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

        TransformTileSystem.TransformOnKill[Type] = false;
        TransformTileSystem.ReplaceOnKillType[Type] = TileID.Dirt;

        DustType = (ushort)ModContent.DustType<Dusts.BackwoodsGrass>();
        AddMapEntry(new Color(38, 107, 57));
	}
}