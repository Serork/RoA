using Iced.Intel;

using RoA.Content.Tiles.Miscellaneous;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class GobletOfPain_WorldGen : ModSystem {
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        tasks.Insert(tasks.FindIndex(task => task.Name == "Micro Biomes"), new PassLegacy("Goblet of Pain", delegate (GenerationProgress progress, GameConfiguration passConfig) {
            var genRand = WorldGen.genRand;
            for (int i = 0; i < 2; i++) {
                int count = WorldGenHelper.WorldSize;
                for (int num436 = 0; num436 < count; num436++) {
                    double value2 = (double)num436 / (double)(count);
                    progress.Set(value2);
                    bool flag24 = false;
                    int num437 = 0;
                    while (!flag24) {
                        int num438 = genRand.Next((int)(Main.maxTilesX * 0.17f), (int)(Main.maxTilesX * 0.83f));
                        int num439 = genRand.Next(Main.maxTilesY - 250, Main.maxTilesY - 30);
                        Tile tile = WorldGenHelper.GetTileSafely(num438, num439);
                        if (tile.HasTile && ((tile.TileType == TileID.Tables && tile.TileFrameX >= 702 && tile.TileFrameX <= 356)
                        || (tile.TileType == TileID.WorkBenches && tile.TileFrameX >= 504 && tile.TileFrameX <= 540))) {
                            if (!WorldGenHelper.GetTileSafely(num438, num439 - 1).HasTile && (tile.TileType != TileID.Tables || WorldGenHelper.GetTileSafely(num438, num439 + 1).TileType == TileID.Tables)) {
                                bool flag4 = true;
                                if (!(num436 == 1 ? genRand.NextBool(2) : num436 != 2 || genRand.NextBool(4))) {
                                    flag4 = false;
                                }
                                if (flag4) {
                                    WorldGen.PlaceTile(num438, num439 - 1, ModContent.TileType<GobletOfPain>());
                                    Console.WriteLine($"{num438},{num439}");
                                }
                                else {
                                    flag24 = true;
                                }
                                if (WorldGenHelper.GetTileSafely(num438, num439 - 1).TileType == ModContent.TileType<GobletOfPain>()) {
                                    flag24 = true;
                                }
                                else {
                                    num437++;
                                    if (num437 >= 10000)
                                        flag24 = true;
                                }
                            }
                        }
                    }
                }
            }
        }));
    }
}
