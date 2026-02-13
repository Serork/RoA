using Microsoft.Xna.Framework;

using ModLiquidLib.ModLoader;

using ReLogic.Utilities;

using RoA.Common.BackwoodsSystems;
using RoA.Content.Liquids;
using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

// TODO: seeds support
sealed class TarBiome_GenPass : ModSystem {
    private void WallVariety(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = Lang.gen[79].Value;
        double num568 = (double)(Main.maxTilesX * Main.maxTilesY) / 5040000.0;
        int num569 = (int)(300.0 * num568);
        int num570 = num569;
        ShapeData shapeData = new ShapeData();
        while (num569 > 0) {
            progress.Set(1.0 - (double)num569 / (double)num570);
            Point point2 = WorldGen.RandomWorldPoint((int)GenVars.worldSurface, 2, 190, 2);
            while (Vector2D.Distance(new Vector2D(point2.X, point2.Y), GenVars.shimmerPosition) < (double)WorldGen.shimmerSafetyDistance) {
                point2 = WorldGen.RandomWorldPoint((int)GenVars.worldSurface, 2, 190, 2);
            }
            bool flag = true;
            int num = 5;
            while (flag) {
                bool flag2 = false;
                int x = point2.X;
                int y = point2.Y;
                for (int i = x - num; i <= x + num; i++) {
                    if (flag2) {
                        break;
                    }
                    for (int j = y - num; j <= y + num; j++) {
                        if (!WorldGen.InWorld(i, j))
                            continue;

                        if (Main.tile[i, j].WallType == TarBiome.TARWALLTYPE) {
                            point2 = WorldGen.RandomWorldPoint((int)GenVars.worldSurface, 2, 190, 2);
                            flag2 = true;
                            break;
                        }
                    }
                }
                if (!flag2) {
                    flag = false;
                }
            }

            Tile tile6 = Main.tile[point2.X, point2.Y];
            Tile tile7 = Main.tile[point2.X, point2.Y - 1];
            ushort num571 = 0;
            if (tile6.TileType == 60)
                num571 = (ushort)(204 + WorldGen.genRand.Next(4));
            else if (tile6.TileType == 1 && tile7.WallType == 0)
                num571 = (WorldGen.remixWorldGen ? (((double)point2.Y > GenVars.rockLayer) ? ((ushort)(196 + WorldGen.genRand.Next(4))) : ((point2.Y <= GenVars.lavaLine || WorldGen.genRand.Next(2) != 0) ? ((ushort)(212 + WorldGen.genRand.Next(4))) : ((ushort)(208 + WorldGen.genRand.Next(4))))) : (((double)point2.Y < GenVars.rockLayer) ? ((ushort)(196 + WorldGen.genRand.Next(4))) : ((point2.Y >= GenVars.lavaLine) ? ((ushort)(208 + WorldGen.genRand.Next(4))) : ((ushort)(212 + WorldGen.genRand.Next(4))))));

            if (tile6.HasTile && num571 != 0 && !tile7.HasTile) {
                bool foundInvalidTile = false;
                bool flag34 = ((tile6.TileType != 60) ? WorldUtils.Gen(new Point(point2.X, point2.Y - 1), new ShapeFloodFill(1000), Actions.Chain(new Modifiers.IsNotSolid(), new Actions.Blank().Output(shapeData), new Actions.ContinueWrapper(Actions.Chain(new Modifiers.IsTouching(true, 60, 147, 161, 396, 397, 70, 191), new Modifiers.IsTouching(true, 147, 161, 396, 397, 70, 191), new Actions.Custom(delegate {
                    foundInvalidTile = true;
                    return true;
                }))))) : WorldUtils.Gen(new Point(point2.X, point2.Y - 1), new ShapeFloodFill(1000), Actions.Chain(new Modifiers.IsNotSolid(), new Actions.Blank().Output(shapeData), new Actions.ContinueWrapper(Actions.Chain(new Modifiers.IsTouching(true, 147, 161, 396, 397, 70, 191), new Actions.Custom(delegate {
                    foundInvalidTile = true;
                    return true;
                }))))));

                if (shapeData.Count > 50 && flag34 && !foundInvalidTile) {
                    WorldUtils.Gen(new Point(point2.X, point2.Y), new ModShapes.OuterOutline(shapeData, useDiagonals: true, useInterior: true), Actions.Chain(new Modifiers.SkipWalls(87), new Actions.PlaceWall(num571)));
                    num569--;
                }

                shapeData.Clear();
            }
        }
    }
    
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Wall Variety"));
        tasks.RemoveAt(genIndex);
        string pass = "Wall Variety";
        tasks.Insert(genIndex, new PassLegacy(pass, WallVariety, 5988.0283f));

        tasks.Insert(tasks.FindIndex(task => task.Name == "Settle Liquids") + 1, new PassLegacy("Tar", delegate (GenerationProgress progress, GameConfiguration passConfig) {
            int num916 = 5 * WorldGenHelper.WorldSize;
            int beachWidth = GenVars.beachBordersWidth / 2;
            double num917 = (double)(Main.maxTilesX - beachWidth) / (double)num916;
            List<Point> list2 = new List<Point>(num916);
            int num918 = 0;
            int num919 = 0;
            while (num919 < num916) {
                double num920 = (double)num919 / (double)num916;
                progress.Set(num920);
                progress.Message = Language.GetOrRegister("Mods.RoA.WorldGen.Tar").Value;
                Point point3 = WorldGen.RandomRectanglePoint((int)(num920 * (double)(Main.maxTilesX - beachWidth)) + beachWidth, (int)GenVars.worldSurface + 200, (int)num917, (int)GenVars.lavaLine - 300 - (int)GenVars.worldSurface);
                //if (remixWorldGen)
                //    point3 = RandomRectanglePoint((int)(num920 * (double)(Main.maxTilesX - 200)) + 100, (int)GenVars.worldSurface + 100, (int)num917, (int)GenVars.rockLayer - (int)GenVars.worldSurface - 100);

                //Point point3 = new Point(WorldGen.genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance), WorldGen.genRand.Next((int)GenVars.worldSurface + 100, Main.maxTilesY - 500));
                //while ((double)point3.X < (double)Main.maxTilesX * 0.05 && (double)point3.X < (double)Main.maxTilesX * 0.95) {
                //    point3.X = WorldGen.genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);
                //}
                point3.X -= 100;

                num918++;
                if (TarBiome.CanPlace(point3, GenVars.structures)) {
                    list2.Add(point3);
                    num919++;
                }
                else if (num918 > 10000) {
                    //num916 = num919;
                    num919++;
                    num918 = 0;
                }
            }

            TarBiome tarBiome = GenVars.configuration.CreateBiome<TarBiome>();
            for (int num921 = 0; num921 < list2.Count; num921++) {
                tarBiome.Place(list2[num921], GenVars.structures);
            }
        }));
        tasks.Insert(tasks.FindIndex(task => task.Name == "Settle Liquids Again") + 2, new PassLegacy("Tar Sources", delegate (GenerationProgress progress, GameConfiguration passConfig) {
            var tarLiquid = LiquidLoader.LiquidType<Tar>();
            for (int j = 5; j < Main.maxTilesX - 5; j++) {
                for (int k = 5; k < Main.maxTilesY - 5; k++) {
                    Tile tile = Main.tile[j, k];
                    //if (tile.LiquidAmount > 0 && tile.LiquidType == tarLiquid) {
                    //    for (int k2 = -10 + WorldGen.genRand.Next(-2, 2); k2 < WorldGen.genRand.Next(-2, 3); k2++) {
                    //        int x = 3 + WorldGen.genRand.Next(-2, 3);
                    //        for (int j2 = -x; j2 <= x; j2++) {
                    //            Tile tile2 = Main.tile[j + j2, k + k2];
                    //            if (tile2.WallType != TarBiome.TARWALLTYPE) {
                    //                tile2.WallType = TarBiome.TARWALLTYPE;
                    //            }
                    //            if (tile2.LiquidAmount > 0 && tile2.LiquidType != tarLiquid) {
                    //                tile2.LiquidAmount = 0;
                    //            }
                    //        }
                    //    }
                    //}
                    if (tile.HasTile) {
                        if (tile.TileType == TarBiome.TARTILETYPE) {
                            if (Main.tile[j, k - 1].LiquidType == tarLiquid && Main.tile[j, k - 1].LiquidAmount == 255 && tile.Slope != Terraria.ID.SlopeType.Solid) {
                                tile.Slope = Terraria.ID.SlopeType.Solid;
                            }
                            if (!Main.tile[j, k - 3].HasTile && Main.tile[j, k - 3].LiquidType == tarLiquid && Main.tile[j, k - 3].LiquidAmount == 255 &&
                                !Main.tile[j, k - 2].HasTile && Main.tile[j, k - 2].LiquidType == tarLiquid && Main.tile[j, k - 2].LiquidAmount == 255 &&
                                Main.tile[j, k - 1].LiquidType == tarLiquid && Main.tile[j, k - 1].LiquidAmount == 255) {
                                ushort tarSource = (ushort)ModContent.TileType<SwellingTar>();
                                int check = 4;
                                bool flag = false;
                                for (int k2 = -check; k2 < check; k2++) {
                                    if (flag) {
                                        break;
                                    }
                                    for (int j2 = -check; j2 <= check; j2++) {
                                        Tile tile2 = Main.tile[j + j2, k - 1 + k2];
                                        if (tile2.TileType == tarSource) {
                                            flag = true;
                                            break;
                                        }
                                    }
                                }
                                if (!flag) {
                                    WorldGenHelper.Place2x2(j, k - 1, tarSource, WorldGen.genRand.NextBool().ToInt(), 0);
                                    if (Main.tile[j - 2, k - 2].TileType != tarSource) {
                                        ModContent.GetInstance<SwellingTarTE>().Place(j - 1, k - 2);
                                    }
                                    if (Main.tile[j, k - 1].TileType == tarSource) {
                                        for (int k2 = -10; k2 < 0; k2++) {
                                            for (int j2 = 0; j2 < 2; j2++) {
                                                Tile tile2 = Main.tile[j - 1 + j2, k + k2 - 1];
                                                if (tile2.LiquidAmount <= 0 && !tile2.HasTile) {
                                                    break;
                                                }
                                                if (tile2.TileType == tarSource || Main.tile[j - 1 + j2, k + k2 - 2].TileType == tarSource) {
                                                    continue;
                                                }
                                                tile2.HasTile = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (int j = 5; j < Main.maxTilesX - 5; j++) {
                for (int k = 5; k < Main.maxTilesY - 5; k++) {
                    Tile tile = Main.tile[j, k];
                    if (tile.HasTile) {
                        if (tile.TileType == TarBiome.TARTILETYPE) {
                            if (!Main.tile[j, k - 1].HasTile)
                                WorldGen.Place2x1(j, k - 1, (ushort)ModContent.TileType<TarRocks2>(), WorldGen.genRand.Next(3));

                            if (!Main.tile[j, k - 1].HasTile) {
                                ushort tarRocks1 = (ushort)ModContent.TileType<TarRocks1>();
                                WorldGen.Place1x1(j, k - 1, tarRocks1);
                                Tile tile2 = Main.tile[j, k - 1];
                                if (tile2.TileType == tarRocks1) {
                                    tile2.TileFrameX = (short)(18 * WorldGen.genRand.Next(6));
                                    tile2.TileFrameY = 0;
                                }
                            }
                        }
                    }
                }
            }
            for (int j = 5; j < Main.maxTilesX - 5; j++) {
                for (int k = 5; k < Main.maxTilesY - 5; k++) {
                    Tile tile = Main.tile[j, k];
                    if (tile.HasTile) {
                        if (tile.TileType == TarBiome.TARTILETYPE && !tile.IsHalfBlock) {
                            bool flag = false;
                            int check = 1;
                            for (int k2 = -check; k2 < check; k2++) {
                                if (flag) {
                                    break;
                                }
                                for (int j2 = -check; j2 <= check; j2++) {
                                    Tile tile2 = Main.tile[j + j2, k + k2];
                                    if (tile2.LiquidType == tarLiquid) {
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                            if (flag) {
                                FastRandom fastRandom2 = new FastRandom(Main.ActiveWorldFileData.Seed).WithModifier(65440uL).WithModifier(j, k);
                                WorldUtils.TileFrame(j, k);
                                WorldGen.SquareWallFrame(j, k);

                                if (fastRandom2.Next(2) == 0)
                                    Tile.SmoothSlope(j, k);
                            }
                        }
                    }
                }
            }
        }));
    }
}