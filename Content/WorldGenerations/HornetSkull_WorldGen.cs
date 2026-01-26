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

sealed class HornetSkull_WorldGen : ModSystem {
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        tasks.Insert(tasks.FindIndex(task => task.Name == "Jungle Chests") + 1, new PassLegacy("Hornet Flowers", delegate (GenerationProgress progress, GameConfiguration passConfig) {
            var genRand = WorldGen.genRand;
            for (int k = 0; k < 2; k++) {
                int count = WorldGenHelper.WorldSize;
                for (int num436 = 0; num436 < count; num436++) {
                    double value2 = (double)num436 / (double)(count);
                    progress.Set(value2);
                    bool flag24 = false;
                    int num437 = 0;
                    while (!flag24) {
                        int num438 = genRand.Next((int)(Main.maxTilesX * 0.05f), (int)(Main.maxTilesX * 0.95f));
                        int num439 = genRand.Next(250, Main.maxTilesY - 30);
                        Tile tile = WorldGenHelper.GetTileSafely(num438, num439);
                        if (tile.HasTile && tile.TileType == TileID.JungleGrass) {
                            if (!WorldGenHelper.GetTileSafely(num438, num439 - 1).HasTile && WorldGenHelper.GetTileSafely(num438, num439 - 1).LiquidAmount <= 0) {
                                bool flag4 = true;
                                if (!(num436 == 1 ? genRand.NextBool(2) : num436 != 2 || genRand.NextBool(4))) {
                                    flag4 = false;
                                }
                                if (flag4) {
                                    WorldGenHelper.Place3x2(num438, num439 - 1, (ushort)ModContent.TileType<HornetFlowers>(), Main.rand.NextBool().ToInt());
                                    Console.WriteLine($"{num438}, {num439}");
                                }
                                else {
                                    flag24 = true;
                                }
                                if (WorldGenHelper.GetTileSafely(num438, num439 - 1).TileType == ModContent.TileType<HornetFlowers>()) {
                                    flag24 = true;
                                    Console.WriteLine($"{num438}, {num439}");
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
