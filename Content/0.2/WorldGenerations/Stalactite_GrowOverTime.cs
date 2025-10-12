using RoA.Content.Tiles.Danger;
using RoA.Content.Tiles.LiquidsSpecific;
using RoA.Content.Tiles.Solid.Backwoods;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.WorldGenerations;

sealed class Stalactite_GrowOverTime : GlobalTile {
    public override void RandomUpdate(int i, int j, int type) {
        if (!WorldGen.genRand.NextBool(600)) {
            return;
        }

        if (!(Main.tile[i, j].HasUnactuatedTile && Main.tile[i, j].HasTile)) {
            return;
        }
        ushort backwoodsStoneTileType = (ushort)ModContent.TileType<BackwoodsStone>();
        ushort solidifiedTarTileType = (ushort)ModContent.TileType<SolidifiedTar>();
        ushort backwoodsMossTileType = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
        j += 1;
        if (Main.hardMode) {
            if (type == TileID.Stone) {
                Stalactite_GenPass.PlaceStalactite(i, j, TileID.Stone, (ushort)ModContent.TileType<StoneStalactite>(), ModContent.GetInstance<StoneStalactiteTE>(), true);
            }
            else if (type == TileID.IceBlock) {
                Stalactite_GenPass.PlaceStalactite(i, j, TileID.IceBlock, (ushort)ModContent.TileType<IceStalactite>(), ModContent.GetInstance<IceStalactiteTE>(), true);
            }
            else if (type == backwoodsStoneTileType) {
                Stalactite_GenPass.PlaceStalactite(i, j, backwoodsStoneTileType, (ushort)ModContent.TileType<GrimrockStalactite>(), ModContent.GetInstance<GrimrockStalactiteTE>(), true);
            }
            else if (type == backwoodsMossTileType) {
                Stalactite_GenPass.PlaceStalactite(i, j, backwoodsMossTileType, (ushort)ModContent.TileType<GrimrockStalactite>(), ModContent.GetInstance<GrimrockStalactiteTE>(), true);
            }
        }
        else if (type == solidifiedTarTileType) {
            Stalactite_GenPass.PlaceStalactite(i, j, solidifiedTarTileType, (ushort)ModContent.TileType<SolidifiedTarStalactite>(), ModContent.GetInstance<SolidifiedTarStalactiteTE>(), true);
        }
    }
}
