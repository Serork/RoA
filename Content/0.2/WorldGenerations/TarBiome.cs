using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Biomes;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class TarBiome_AddPass : ModSystem {
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        if (!RoA.HasRoALiquidMod()) {
            return;
        }

        tasks.Insert(tasks.FindIndex(task => task.Name == "Buried Chests") - 10, new PassLegacy("Tar Biome", delegate (GenerationProgress progress, GameConfiguration passConfig) {
            int num916 = 8 * WorldGenHelper.WorldSize;
            double num917 = (double)(Main.maxTilesX - 200) / (double)num916;
            List<Point> list2 = new List<Point>(num916);
            int num918 = 0;
            int num919 = 0;
            while (num919 < num916) {
                double num920 = (double)num919 / (double)num916;
                Point point3 = WorldGen.RandomRectanglePoint((int)(num920 * (double)(Main.maxTilesX - 200)) + 100, (int)GenVars.rockLayer + 20, (int)num917, Main.maxTilesY - ((int)GenVars.rockLayer + 40) - 500);
                //if (WorldGen.remixWorldGen)
                //    point3 = WorldGen.RandomRectanglePoint((int)(num920 * (double)(Main.maxTilesX - 200)) + 100, (int)GenVars.worldSurface + 100, (int)num917, (int)GenVars.rockLayer - (int)GenVars.worldSurface - 100);

                //while ((double)point3.X > (double)Main.maxTilesX * 0.45 && (double)point3.X < (double)Main.maxTilesX * 0.55) {
                //    point3.X = WorldGen.genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);
                //}

                num918++;
                if (TarBiome.CanPlace(point3, GenVars.structures)) {
                    list2.Add(point3);
                    num919++;
                }
                else if (num918 > Main.maxTilesX * 20) {
                    num916 = num919;
                    num919++;
                    num918 = 0;
                }
            }

            TarBiome tarBiome = GenVars.configuration.CreateBiome<TarBiome>();
            for (int num921 = 0; num921 < num916; num921++) {
                tarBiome.Place(list2[num921], GenVars.structures);
            }
        }));
    }
}

sealed class TarBiome : MicroBiome {
    private struct Magma {
        public readonly double Pressure;
        public readonly double Resistance;
        public readonly bool IsActive;

        private Magma(double pressure, double resistance, bool active) {
            Pressure = pressure;
            Resistance = resistance;
            IsActive = active;
        }

        public Magma ToFlow() => new Magma(Pressure, Resistance, active: true);
        public static Magma CreateFlow(double pressure, double resistance = 0.0) => new Magma(pressure, resistance, active: true);
        public static Magma CreateEmpty(double resistance = 0.0) => new Magma(0.0, resistance, active: false);
    }

    private const int MAX_MAGMA_ITERATIONS = 300;
    private Magma[,] _sourceMagmaMap = new Magma[200, 200];
    private Magma[,] _targetMagmaMap = new Magma[200, 200];
    private static Vector2D[] _normalisedVectors = new Vector2D[9] {
        Vector2D.Normalize(new Vector2D(-1.0, -1.0)),
        Vector2D.Normalize(new Vector2D(-1.0, 0.0)),
        Vector2D.Normalize(new Vector2D(-1.0, 1.0)),
        Vector2D.Normalize(new Vector2D(0.0, -1.0)),
        new Vector2D(0.0, 0.0),
        Vector2D.Normalize(new Vector2D(0.0, 1.0)),
        Vector2D.Normalize(new Vector2D(1.0, -1.0)),
        Vector2D.Normalize(new Vector2D(1.0, 0.0)),
        Vector2D.Normalize(new Vector2D(1.0, 1.0))
    };

    private static ushort TARTILETYPE => RoA.RoALiquidMod.Find<ModTile>("SolidifiedTar").Type;
    private static ushort TARDRIPPINGTILETYPE => RoA.RoALiquidMod.Find<ModTile>("DrippingTar").Type;
    private static ushort TARWALLTYPE => (ushort)ModContent.WallType<SolidifiedTarWall>();

    public static bool CanPlace(Point origin, StructureMap structures) {
        if (origin.X > GenVars.shimmerPosition.X - WorldGen.shimmerSafetyDistance && origin.X < GenVars.shimmerPosition.X + WorldGen.shimmerSafetyDistance) {
            return false;
        }
        if (origin.X > GenVars.JungleX - Main.maxTilesX / 8 && origin.X < GenVars.JungleX + Main.maxTilesX / 8) {
            return false;
        }

        if (WorldGen.BiomeTileCheck(origin.X, origin.Y))
            return false;

        if (GenBase._tiles[origin.X, origin.Y].HasTile)
            return false;

        return true;
    }

    public override bool Place(Point origin, StructureMap structures) {
        origin.X -= _sourceMagmaMap.GetLength(0) / 2;
        origin.Y -= _sourceMagmaMap.GetLength(1) / 2;
        BuildMagmaMap(origin);
        SimulatePressure(out var effectedMapArea);
        PlaceGranite(origin, effectedMapArea);
        CleanupTiles(origin, effectedMapArea);
        PlaceDecorations(origin, effectedMapArea);
        structures.AddStructure(effectedMapArea, 8);
        return true;
    }

    private void BuildMagmaMap(Point tileOrigin) {
        _sourceMagmaMap = new Magma[90, 70];
        _targetMagmaMap = new Magma[90, 70];
        for (int i = 0; i < _sourceMagmaMap.GetLength(0); i++) {
            for (int j = 0; j < _sourceMagmaMap.GetLength(1); j++) {
                int i2 = i + tileOrigin.X;
                int j2 = j + tileOrigin.Y;
                _sourceMagmaMap[i, j] = Magma.CreateEmpty(1);
                _targetMagmaMap[i, j] = _sourceMagmaMap[i, j];
            }
        }
    }

    private void SimulatePressure(out Rectangle effectedMapArea) {
        int length = _sourceMagmaMap.GetLength(0);
        int length2 = _sourceMagmaMap.GetLength(1);
        int num = length / 2;
        int num2 = length2 / 2;
        int num3 = num;
        int num4 = num3;
        int num5 = num2;
        int num6 = num5;
        for (int i = 0; i < 300; i++) {
            for (int j = num3; j <= num4; j++) {
                for (int k = num5; k <= num6; k++) {
                    Magma magma = _sourceMagmaMap[j, k];
                    if (!magma.IsActive)
                        continue;

                    double num7 = 0.0;
                    Vector2D zero = Vector2D.Zero;
                    for (int l = -1; l <= 1; l++) {
                        for (int m = -1; m <= 1; m++) {
                            if (l == 0 && m == 0)
                                continue;

                            Vector2D vector2D = _normalisedVectors[(l + 1) * 3 + (m + 1)];
                            Magma magma2 = _sourceMagmaMap[j + l, k + m];
                            if (magma.Pressure > 0.01 && !magma2.IsActive) {
                                if (l == -1)
                                    num3 = Utils.Clamp(j + l, 1, num3);
                                else
                                    num4 = Utils.Clamp(j + l, num4, length - 2);

                                if (m == -1)
                                    num5 = Utils.Clamp(k + m, 1, num5);
                                else
                                    num6 = Utils.Clamp(k + m, num6, length2 - 2);

                                _targetMagmaMap[j + l, k + m] = magma2.ToFlow();
                            }

                            double pressure = magma2.Pressure;
                            num7 += pressure;
                            zero += pressure * vector2D;
                        }
                    }

                    num7 /= 8.0;
                    if (num7 > magma.Resistance) {
                        double num8 = zero.Length() / 8.0;
                        double val = Math.Max(num7 - num8 - magma.Pressure, 0.0) + num8 + magma.Pressure * 0.875 - magma.Resistance;
                        val = Math.Max(0.0, val);
                        _targetMagmaMap[j, k] = Magma.CreateFlow(val, Math.Max(0.0, magma.Resistance - val * 0.02));
                    }
                }
            }

            if (i < 2)
                _targetMagmaMap[num, num2] = Magma.CreateFlow(25.0);

            Utils.Swap(ref _sourceMagmaMap, ref _targetMagmaMap);
        }

        effectedMapArea = new Rectangle(num3, num5, num4 - num3 + 1, num6 - num5 + 1);
    }

    private bool ShouldUseLava(Point tileOrigin) {
        int length = _sourceMagmaMap.GetLength(0);
        int length2 = _sourceMagmaMap.GetLength(1);
        int num = length / 2;
        int num2 = length2 / 2;
        if (tileOrigin.Y + num2 <= GenVars.lavaLine - 30)
            return false;

        for (int i = -50; i < 50; i++) {
            for (int j = -50; j < 50; j++) {
                if (GenBase._tiles[tileOrigin.X + num + i, tileOrigin.Y + num2 + j].HasTile) {
                    ushort type = GenBase._tiles[tileOrigin.X + num + i, tileOrigin.Y + num2 + j].TileType;
                    if (type == 147 || (uint)(type - 161) <= 2u || type == 200)
                        return false;
                }
            }
        }

        return true;
    }

    private void PlaceGranite(Point tileOrigin, Rectangle magmaMapArea) {
        bool flag = ShouldUseLava(tileOrigin);
        ushort type = TARTILETYPE;
        int y = 0;
        for (int i = magmaMapArea.Left; i < magmaMapArea.Right; i++) {
            double num4 = (magmaMapArea.Left - i) / (float)(magmaMapArea.Right - i);
            for (int j = magmaMapArea.Top; j < magmaMapArea.Bottom; j++) {
                Magma magma = _sourceMagmaMap[i, j];
                if (!magma.IsActive)
                    continue;

                y += _random.Next(-1, 2);
                int maxRandomY = 3;
                if (y > maxRandomY) {
                    y = maxRandomY;
                }
                if (y < -maxRandomY) {
                    y = -maxRandomY;
                }
                Tile tile = GenBase._tiles[tileOrigin.X + i, tileOrigin.Y + j];
                if (j < magmaMapArea.Bottom - 50 + y || j > magmaMapArea.Bottom - 10 + y) {
                    continue;
                }

                double num3 = (magmaMapArea.Top - j) / (float)(magmaMapArea.Bottom - j);
                double num = Math.Sin((double)(num3) * 0.25) * 0.7 + 0.8;
                double num2 = 0.5 + 1 / Math.Sqrt(Math.Max(0.0, magma.Pressure - magma.Resistance));
                if (Math.Max(1.0 - Math.Max(0.0, num * num2), magma.Pressure / 10.0) > num * 0.125 + (WorldGen.SolidTile(tileOrigin.X + i, tileOrigin.Y + j) ? 0 : 0.5)) {
                    //if (TileID.Sets.Ore[tile.TileType])
                    //    tile.ResetToType(tile.TileType);
                    //else
                        tile.ResetToType(type);

                    //tile.WallType = wall;

                    if (num4 < 0.1f && _random.NextChance(0.01f)) {
                        if (j < magmaMapArea.Bottom - 20) {
                            WorldGen.TileRunner(tileOrigin.X + i, tileOrigin.Y + j, _random.Next(20), _random.Next(5), -1);
                            WorldGen.TileRunner(tileOrigin.X + i, tileOrigin.Y + j, _random.Next(5, 8), _random.Next(6, 9), -1, addTile: false, -2.0, -0.3);
                            WorldGen.TileRunner(tileOrigin.X + i, tileOrigin.Y + j, _random.Next(5, 8), _random.Next(6, 9), -1, addTile: false, 2.0, -0.3);
                        }
                    }
                }
                else if (magma.Resistance < 0.01) {
                    double num5 = (i - magmaMapArea.Left) / (float)(magmaMapArea.Right - magmaMapArea.Left);
                    bool flag2 = num5 > 0.35f + _random.NextFloat(0.05f) && num5 < 0.65f - _random.NextFloat(0.05f);
                    bool flag3 = num5 > 0.05f + _random.NextFloat(0.05f) && num5 < 0.95f - _random.NextFloat(0.05f);
                    if (tile.LiquidType != 5) {
                        tile.LiquidAmount = 0;
                    }
                    if (j > magmaMapArea.Bottom - 40 + y) {
                        if (flag2 && j > magmaMapArea.Bottom - 30 - _random.Next(-5, 5)) {
                            tile.LiquidAmount = 255;
                            tile.LiquidType = 5;
                        }
                        WorldUtils.ClearTile(tileOrigin.X + i, tileOrigin.Y + j);
                    }
                    if (flag3) {
                        tile.WallType = TARWALLTYPE;
                        //tile.WallColor = PaintID.BrownPaint;
                    }
                }

                //if (tile.LiquidAmount > 0 && flag)
                //    tile.LiquidType = LiquidID.Water;
            }
        }

        for (int i = magmaMapArea.Left; i < magmaMapArea.Right; i++) {
            double num4 = (magmaMapArea.Left - i) / (float)(magmaMapArea.Right - i);
            for (int j = magmaMapArea.Top; j < magmaMapArea.Bottom; j++) {
                Magma magma = _sourceMagmaMap[i, j];
                if (!magma.IsActive)
                    continue;

                y += _random.Next(-1, 2);
                int maxRandomY = 3;
                if (y > maxRandomY) {
                    y = maxRandomY;
                }
                if (y < -maxRandomY) {
                    y = -maxRandomY;
                }
                Tile tile = GenBase._tiles[tileOrigin.X + i, tileOrigin.Y + j];
                if (j > magmaMapArea.Bottom - 45 + y && j < magmaMapArea.Bottom - 30 + y) {
                    if (tile.HasTile) {
                        tile.ResetToType(type);
                    }
                }
            }
        }
    }

    private void CleanupTiles(Point tileOrigin, Rectangle magmaMapArea) {
        List<Point16> list = new List<Point16>();
        for (int i = magmaMapArea.Left; i < magmaMapArea.Right; i++) {
            for (int j = magmaMapArea.Top; j < magmaMapArea.Bottom; j++) {
                if (!_sourceMagmaMap[i, j].IsActive)
                    continue;

                int num = 0;
                int num2 = i + tileOrigin.X;
                int num3 = j + tileOrigin.Y;
                if (!WorldGen.SolidTile(num2, num3))
                    continue;

                for (int k = -1; k <= 1; k++) {
                    for (int l = -1; l <= 1; l++) {
                        if (WorldGen.SolidTile(num2 + k, num3 + l))
                            num++;
                    }
                }

                if (num < 3)
                    list.Add(new Point16(num2, num3));
            }
        }

        //foreach (Point16 item in list) {
        //    int x = item.X;
        //    int y = item.Y;
        //    WorldUtils.ClearTile(x, y, frameNeighbors: true);
        //    GenBase._tiles[x, y].WallType = wall;
        //}

        list.Clear();
    }

    private void PlaceDecorations(Point tileOrigin, Rectangle magmaMapArea) {
        FastRandom fastRandom = new FastRandom(Main.ActiveWorldFileData.Seed).WithModifier(65440uL);
        for (int i = magmaMapArea.Left; i < magmaMapArea.Right; i++) {
            for (int j = magmaMapArea.Top; j < magmaMapArea.Bottom; j++) {
                Magma magma = _sourceMagmaMap[i, j];
                int num = i + tileOrigin.X;
                int num2 = j + tileOrigin.Y;

                FastRandom fastRandom2 = fastRandom.WithModifier(num, num2);
                if (magma.IsActive) {
                    WorldUtils.TileFrame(num, num2);
                    WorldGen.SquareWallFrame(num, num2);

                    if (fastRandom2.Next(2) == 0)
                        Tile.SmoothSlope(num, num2);
                }
                if (GenBase._tiles[num, num2].HasTile && !_tiles[num, num2].BottomSlope) {
                    if (!GenBase._tiles[num, num2 + 1].HasTile && GenBase._tiles[num, num2 + 1].LiquidAmount <= 0)
                        WorldGen.PlaceTile(num, num2 + 1, TARDRIPPINGTILETYPE);
                }
                if (_tiles[num, num2].HasTile && _tiles[num, num2].TileType == TileID.WaterDrip) {
                    _tiles[num, num2].TileType = TARDRIPPINGTILETYPE;
                }
                if (_tiles[num, num2].LiquidType != 5) {
                    _tiles[num, num2].LiquidAmount = 0;
                }
                //if (fastRandom2.Next(8) == 0 && GenBase._tiles[num, num2].HasTile) {
                //    if (!GenBase._tiles[num, num2 + 1].HasTile)
                //        WorldGen.PlaceUncheckedStalactite(num, num2 + 1, fastRandom2.Next(2) == 0, fastRandom2.Next(3), spiders: false);

                //    if (!GenBase._tiles[num, num2 - 1].HasTile)
                //        WorldGen.PlaceUncheckedStalactite(num, num2 - 1, fastRandom2.Next(2) == 0, fastRandom2.Next(3), spiders: false);
                //}
            }
        }
    }
}
