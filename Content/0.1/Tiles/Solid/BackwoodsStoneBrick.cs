using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid;

sealed class BackwoodsStoneBrick : ModTile {
    public override void SetStaticDefaults() {
        TileHelper.Solid(Type, blendAll: false);
        //TileHelper.MergeWith(Type, (ushort)ModContent.TileType<BackwoodsGrass>());

        TileID.Sets.Conversion.Stone[Type] = true;

        TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        DustType = (ushort)ModContent.DustType<Dusts.Backwoods.Stone>();
        HitSound = SoundID.Tink;
        AddMapEntry(new Color(53, 55, 54));

        MineResist = 1.25f;
    }
}