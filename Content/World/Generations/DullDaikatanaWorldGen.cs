using RoA.Content.Tiles.Miscellaneous;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.World.Generations;

sealed class DullDaikatanaWorldGen : ModSystem {
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Hellforge"));

        tasks.Insert(genIndex, new PassLegacy("Dull Daikatana", DullDaikatanaGenerator, 9213.443f));
    }

    private void DullDaikatanaGenerator(GenerationProgress progress, GameConfiguration configuration) {
        var genRand = WorldGen.genRand;
        int count = WorldGenHelper.WorldSize;
        for (int num436 = 0; num436 < count; num436++) {
            double value2 = (double)num436 / (double)(count);
            progress.Set(value2);
            bool flag24 = false;
            int num437 = 0;
            while (!flag24) {
                int num438 = genRand.Next(1, Main.maxTilesX);
                int num439 = genRand.Next(Main.maxTilesY - 250, Main.maxTilesY - 30);
                try {
                    if (Main.tile[num438, num439].WallType == 14) {
                        bool flag = true;
                        int check = 3;
                        for (int x = -check; x < check + 2; x++) {
                            if (!flag) {
                                break;
                            }
                            for (int y = -check; y < check + 2; y++) {
                                if (Main.tile[num438 + x, num439 + y].HasTile) {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                        int tileType = ModContent.TileType<DullDaikatana>();
                        if (flag) {
                            check = 100;
                            int checkX = 400;
                            for (int x = -checkX; x < checkX + 2; x++) {
                                if (!flag) {
                                    break;
                                }
                                for (int y = -check; y < check + 2; y++) {
                                    if (Main.tile[num438 + x, num439 + y].TileType == tileType) {
                                        flag = false;
                                        break;
                                    }
                                }
                            }
                        }
                        if (flag) {
                            WorldGen.PlaceTile(num438, num439, tileType);
                            Console.WriteLine(num438 + " " + num439);
                        }
                        if (Main.tile[num438, num439].TileType == tileType) {
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
