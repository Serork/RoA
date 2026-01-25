using RoA.Content.Tiles.Miscellaneous;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class DullDaikatanaWorldGen : ModSystem {
    private static bool _chairPlaced;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        _chairPlaced = false;

        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Hellforge"));

        tasks.Insert(genIndex, new PassLegacy("Dull Daikatana", DullDaikatanaGenerator, 9213.443f));
    }

    private static ushort GetUnderworldBuildingWallType() {
        ushort wallType = 14;
        if (ModLoader.TryGetMod("TheDepths", out Mod theDepths) && theDepths.TryFind<ModWall>("ArqueriteBrickWallUnsafe", out ModWall ArqueriteBrickWallUnsafe)) {
            wallType = ArqueriteBrickWallUnsafe.Type;
        }
        return wallType;
    }

    private void DullDaikatanaGenerator(GenerationProgress progress, GameConfiguration configuration) {
        var genRand = WorldGen.genRand;
        bool hasRemnants = ModLoader.HasMod("Remnants");
        int count = WorldGenHelper.WorldSize * (hasRemnants ? 2 : 1);
        ushort checkWallType = GetUnderworldBuildingWallType();
        for (int num436 = 0; num436 < count; num436++) {
            double value2 = (double)num436 / (double)(count);
            progress.Set(value2);
            bool flag24 = false;
            int num437 = 0;
            while (!flag24) {
                int num438 = genRand.Next((int)(Main.maxTilesX * 0.17f), (int)(Main.maxTilesX * 0.83f));
                int num439 = genRand.Next(Main.maxTilesY - 250, Main.maxTilesY - 30);
                try {
                    if (WorldGenHelper.GetTileSafely(num438, num439).WallType == checkWallType || WorldGenHelper.GetTileSafely(num438, num439).WallType == 14) {
                        bool flag = true;
                        int check = 3;
                        for (int x = -check; x < check + 2; x++) {
                            if (!flag) {
                                break;
                            }
                            for (int y = -check; y < check + 2; y++) {
                                if (WorldGenHelper.GetTileSafely(num438 + x, num439 + y).HasTile) {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        int tileType = ModContent.TileType<DullDaikatana>();
                        int extraX = genRand.Next(0, 4);
                        int extraY = genRand.Next(-1, 2);
                        if (flag) {
                            check = 100;
                            int checkX = 400;
                            for (int x = -checkX; x < checkX + 2; x++) {
                                if (!flag) {
                                    break;
                                }
                                for (int y = -check; y < check + 2; y++) {
                                    if (WorldGenHelper.GetTileSafely(num438 + extraX + x, num439 - extraY + y).TileType == tileType) {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (flag) {
                            bool flag4 = true;
                            if (!hasRemnants) {
                                if (!((num436 == 1 ? genRand.NextBool(2) : num436 != 2 || genRand.NextBool(4)))) {
                                    flag4 = false;
                                }
                            }
                            if (flag4) {
                                WorldGen.PlaceTile(num438 + extraX, num439 - extraY, tileType);
                            }
                            else {
                                flag24 = true;
                            }

                            if (!_chairPlaced || (hasRemnants && genRand.NextBool(2))) {
                                int y2 = num439;
                                for (; !WorldGen.SolidTile(num438, y2) && !TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(num438, y2).TileType] && y2 < Main.maxTilesY - 20; y2++) {
                                }
                                int x2 = num438 + 1;
                                int x3 = x2;
                                bool flag2 = false;
                                int attempts = 10;
                                while (attempts > 0 && (WorldGenHelper.GetTileSafely(x2, y2 - 1).HasTile || WorldGenHelper.GetTileSafely(x2, y2 - 2).HasTile)) {
                                    flag2 = true;
                                    x2++;
                                    attempts--;
                                    if (!WorldGenHelper.GetTileSafely(x2, y2).HasTile) {
                                        break;
                                    }
                                }
                                bool flag3 = false;
                                if (attempts > 0) {
                                    WorldGenHelper.Place1x2(x2, y2 - 1, TileID.Chairs, 0, 0, paintID: PaintID.WhitePaint);
                                    if (WorldGenHelper.GetTileSafely(x2, y2 - 1).TileType == TileID.Chairs) {
                                        flag3 = true;
                                        _chairPlaced = true;
                                    }
                                }
                                if (!flag3) {
                                    x2 = x3;
                                    attempts = 10;
                                    while (attempts > 0 && (WorldGenHelper.GetTileSafely(x2, y2 - 1).HasTile || WorldGenHelper.GetTileSafely(x2, y2 - 2).HasTile)) {
                                        flag2 = false;
                                        x2--;
                                        attempts--;
                                        if (!WorldGenHelper.GetTileSafely(x2, y2).HasTile) {
                                            break;
                                        }
                                    }
                                    if (attempts > 0) {
                                        WorldGenHelper.Place1x2(x2, y2 - 1, TileID.Chairs, 18, 0, paintID: PaintID.WhitePaint);
                                        _chairPlaced = true;
                                    }
                                }
                            }
                        }
                        if (WorldGenHelper.GetTileSafely(num438 + extraX, num439 - extraY).TileType == tileType) {
                            flag24 = true;
                        }
                        else {
                            num437++;
                            if (num437 >= 10000)
                                flag24 = true;
                        }
                    }
                }
                catch {
                    num437++;
                    if (num437 >= 10000)
                        flag24 = true;
                }
            }
        }
    }
}
