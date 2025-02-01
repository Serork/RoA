using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class BackwoodsStone : ModTile {
	public override void SetStaticDefaults() {
        TileHelper.Solid(Type, blendAll: false);
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsGrass>());

        Main.tileStone[Type] = true;

        TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Stone>();
        HitSound = SoundID.Tink;
		AddMapEntry(new Color(53, 55, 54));

        MineResist = 1.25f;
	}
}