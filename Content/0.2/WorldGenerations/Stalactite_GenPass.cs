using Microsoft.CodeAnalysis.Text;

using RoA.Content.Tiles.Danger;
using RoA.Content.Tiles.LiquidsSpecific;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class Stalactite_GenPass : IInitializer {
    private static HashSet<(ModTileEntity, Point16)>? _TEPositions;

    void ILoadable.Load(Mod mod) {
        On_WorldGen.smCallback_End += On_WorldGen_smCallback_End;
    }

    public static void PlaceStalactite(int i, int j, ushort solidTileType, ushort stalactiteTileType, ModTileEntity teInstance, bool notHardmodeGen = false) {
        int checkWidth = 5, checkHeight = 5;
        bool canPlace = true;
        for (int checkX = i - checkWidth; checkX < i + checkWidth; checkX++) {
            if (!canPlace) {
                break;
            }
            for (int checkY = j - checkHeight; checkY < j + checkHeight; checkY++) {
                ModTile modTile = TileLoader.GetTile(WorldGenHelper.GetTileSafely(checkX, checkY).TileType);
                if (modTile is GrimrockStalactite || modTile is IceStalactite || modTile is StoneStalactite || modTile is SolidifiedTarStalactite) {
                    canPlace = false;
                    break;
                }
            }
        }
        if (!canPlace) {
            return;
        }
        ushort tileType = WorldGenHelper.GetTileSafely(i, j).TileType;
        if (Main.tileCut[tileType] ||
            tileType == (ushort)ModContent.TileType<DrippingTar>() ||
            tileType == TileID.WaterDrip ||
            tileType == TileID.LavaDrip) {
            WorldGen.KillTile(i, j);
        }
        void placeTE(Point16 tilePosition) {
            if (notHardmodeGen) {
                teInstance.Place(tilePosition.X, tilePosition.Y);
            }
            else {
                _TEPositions!.Add((teInstance, tilePosition));
            }
        }
        if (WorldGenHelper.Place1x2Top(i, j, stalactiteTileType, WorldGen.genRand.Next(3), onPlace: placeTE)) {
            if (!notHardmodeGen) {
                WorldGenHelper.ModifiedTileRunner(i, j, WorldGen.genRand.Next(4, 8), WorldGen.genRand.Next(1, 4), solidTileType, ignoreTileTypes: [stalactiteTileType]);
                for (int checkX = i - checkWidth; checkX < i + checkWidth; checkX++) {
                    for (int checkY = j - checkHeight; checkY < j + checkHeight; checkY++) {
                        if (WorldGenHelper.GetTileSafely(checkX, checkY).TileType == solidTileType && WorldGen.genRand.NextChance(0.75f)) {
                            WorldGenHelper.Place1x2Top(checkX, checkY + 1, stalactiteTileType, WorldGen.genRand.Next(3), onPlace: placeTE);
                        }
                    }
                }
            }
        }
    }

    private static void GenerateStalactites(GenerationProgress progress, GameConfiguration configuration) {
        ushort backwoodsStoneTileType = (ushort)ModContent.TileType<BackwoodsStone>();
        ushort solidifiedTarTileType = (ushort)ModContent.TileType<SolidifiedTar>();
        ushort backwoodsMossTileType = (ushort)ModContent.TileType<BackwoodsGreenMoss>();
        ushort[] validSolidTileTypes = [TileID.Stone, TileID.IceBlock, backwoodsStoneTileType, solidifiedTarTileType, backwoodsMossTileType];
        for (int i = 20; i < Main.maxTilesX - 20; i++) {
            for (int j = (int)Main.worldSurface; j < Main.maxTilesY - 300; j++) {
                if (!WorldGen.InWorld(i, j, 50)) {
                    continue;
                }
                Tile solidTile = WorldGenHelper.GetTileSafely(i, j - 1);
                if (WorldGen.SolidTile(i, j - 1) && Main.tile[i, j - 1].HasUnactuatedTile && WorldGen.genRand.NextBool(100) &&
                    validSolidTileTypes.Contains(solidTile.TileType)) {
                    if (solidTile.TileType == TileID.Stone) {
                        PlaceStalactite(i, j, TileID.Stone, (ushort)ModContent.TileType<StoneStalactite>(), ModContent.GetInstance<StoneStalactiteTE>());
                    }
                    else if (solidTile.TileType == TileID.IceBlock) {
                        PlaceStalactite(i, j, TileID.IceBlock, (ushort)ModContent.TileType<IceStalactite>(), ModContent.GetInstance<IceStalactiteTE>());
                    }
                    else if (solidTile.TileType == backwoodsStoneTileType) {
                        PlaceStalactite(i, j, backwoodsStoneTileType, (ushort)ModContent.TileType<GrimrockStalactite>(), ModContent.GetInstance<GrimrockStalactiteTE>());
                    }
                    else if (solidTile.TileType == backwoodsMossTileType) {
                        PlaceStalactite(i, j, backwoodsMossTileType, (ushort)ModContent.TileType<GrimrockStalactite>(), ModContent.GetInstance<GrimrockStalactiteTE>());
                    }
                    else if (solidTile.TileType == solidifiedTarTileType) {
                        PlaceStalactite(i, j, solidifiedTarTileType, (ushort)ModContent.TileType<SolidifiedTarStalactite>(), ModContent.GetInstance<SolidifiedTarStalactiteTE>());
                    }
                }
            }
        }

    }

    private void On_WorldGen_smCallback_End(On_WorldGen.orig_smCallback_End orig, System.Collections.Generic.List<Terraria.WorldBuilding.GenPass> hardmodeTasks) {
        _TEPositions = [];

        hardmodeTasks.Add(new PassLegacy("Stalactites", GenerateStalactites));

        orig(hardmodeTasks);

        foreach (var teInfo in _TEPositions) {
            teInfo.Item1.Place(teInfo.Item2.X, teInfo.Item2.Y);
        }
        _TEPositions.Clear();
        _TEPositions = null!;
    }
}
