using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class LivingElderwoodlLeaves : ModTile {
	public override void SetStaticDefaults() {
        TileHelper.Solid(Type, false, false);
        TileHelper.MergeWith(Type, (ushort)ModContent.TileType<LivingElderwood>());
        TileHelper.MergeWith(Type, TileID.Dirt);

        TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        HitSound = SoundID.Grass;
        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Grass>();
		AddMapEntry(new Color(0, 128, 0));
	}
}