using Microsoft.Xna.Framework;
using RoA.Common.Tiles;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Solid.Backwoods;

sealed class BackwoodsGreenMoss : ModTile {
    public override void SetStaticDefaults() {
        ushort stoneType = (ushort)ModContent.TileType<BackwoodsStone>();
        TileHelper.Solid(Type, false, false);
        TileHelper.MergeWith(Type, stoneType);

        TileID.Sets.Grass[Type] = true;
        TileID.Sets.NeedsGrassFraming[Type] = true;
        TileID.Sets.NeedsGrassFramingDirt[Type] = stoneType;
        TileID.Sets.GeneralPlacementTiles[Type] = false;

        TransformTileSystem.OnKillNormal[Type] = false;
        TransformTileSystem.ReplaceToOnKill[Type] = stoneType;

        DustType = DustID.GreenMoss;
        HitSound = SoundID.Dig;
        AddMapEntry(new Color(49, 134, 114));
    }
}