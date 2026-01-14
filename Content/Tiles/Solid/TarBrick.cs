using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid;

sealed class TarBrick : ModTile {
    public override void SetStaticDefaults() {
        TileHelper.Solid(Type, blendAll: false);

        TileID.Sets.BlockMergesWithMergeAllBlock[Type] = true;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        DustType = (ushort)ModContent.DustType<Dusts.SolidifiedTar>();
        HitSound = SoundID.Tink;
        AddMapEntry(new Color(68, 57, 77));
    }
}