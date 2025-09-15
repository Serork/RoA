using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using ReLogic.Utilities;

using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Danger;
using RoA.Content.Tiles.LiquidsSpecific;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

// TODO: seeds support
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
    private Magma[,] _sourceMagmaMap;
    private Magma[,] _targetMagmaMap;
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

    private static ushort TARTILETYPE => (ushort)ModContent.TileType<SolidifiedTar>();
    private static ushort TARDRIPPINGTILETYPE => (ushort)ModContent.TileType<DrippingTar>();
    private static ushort TARWALLTYPE => (ushort)ModContent.WallType<SolidifiedTarWall_Unsafe>();

    public static bool CanPlace(Point origin, StructureMap structures) {
        if (TooCloseToImportantLocations(origin)) {
            return false;
        }

        if (origin.X > GenVars.shimmerPosition.X - WorldGen.shimmerSafetyDistance && origin.X < GenVars.shimmerPosition.X + WorldGen.shimmerSafetyDistance &&
            origin.Y > GenVars.shimmerPosition.Y - WorldGen.shimmerSafetyDistance && origin.Y < GenVars.shimmerPosition.Y + WorldGen.shimmerSafetyDistance) {
            return false;
        }
        if (origin.X > GenVars.JungleX - Main.maxTilesX / 10 && origin.X < GenVars.JungleX + Main.maxTilesX / 10) {
            return false;
        }

        if (WorldGen.BiomeTileCheck(origin.X, origin.Y))
            return false;

        if (BiomeTileCheck2(origin.X, origin.Y)) {
            return false;
        }

        return true;
    }

    public static bool BiomeTileCheck2(int x, int y) {
        int num = 100;
        for (int i = x - num; i <= x + num; i++) {
            for (int j = y - num; j <= y + num; j++) {
                if (!WorldGen.InWorld(i, j))
                    continue;

                if (Main.tile[i, j].HasTile) {
                    int type = Main.tile[i, j].TileType;
                    if (type == TileID.Crimstone || type == TileID.Ebonstone || Main.tileDungeon[type] || type == TARTILETYPE)
                        return true;
                }

                int wall = Main.tile[i, j].WallType;
                if (wall == WallID.CrimstoneUnsafe || wall == WallID.EbonstoneUnsafe || Main.wallDungeon[wall] || wall == TARWALLTYPE)
                    return true;
            }
        }

        return false;
    }

    public override bool Place(Point origin, StructureMap structures) {
        _sourceMagmaMap = new Magma[90, 70];
        _targetMagmaMap = new Magma[90, 70];
        origin.X -= _sourceMagmaMap.GetLength(0) / 2;
        origin.Y -= _sourceMagmaMap.GetLength(1) / 2;
        BuildMagmaMap(origin);
        SimulatePressure(out var effectedMapArea);
        PlaceGranite(origin, effectedMapArea);
        CleanupTiles(origin, effectedMapArea);
        PlaceStalactites(origin);
        PlaceDecorations(origin, effectedMapArea);
        if (WorldGen.gen) {
            structures.AddProtectedStructure(effectedMapArea, 8);
        }
        return true;
    }

    private void PlaceStalactites(Point tileOrigin) {
        ushort tarStalactiteTileType = (ushort)ModContent.TileType<SolidifiedTarStalactite>();
        int width = 50, height = 50;
        tileOrigin.X += 4;
        tileOrigin.Y += 4;
        for (int i = tileOrigin.X - width / 2; i < tileOrigin.X + width * 2; i++) {
            for (int j = tileOrigin.Y - height / 2; j < tileOrigin.Y + height * 2; j++) {
                if (!WorldGen.InWorld(i, j, 10)) {
                    continue;
                }

                if (i > tileOrigin.X + width / 3 && i < tileOrigin.X + width / 3 + width &&
                    j > tileOrigin.Y + height / 3 && j < tileOrigin.Y - 10 + height / 3 + height) {
                    continue;
                }

                if (WorldGen.genRand.NextChance(0.05f)) {
                    int checkWidth = 5, checkHeight = 5;
                    bool canPlace = true;
                    for (int checkX = i - checkWidth; checkX < i + checkWidth; checkX++) {
                        if (!canPlace) {
                            break;
                        }
                        for (int checkY = j - checkHeight; checkY < j + checkHeight; checkY++) {
                            if (WorldGenHelper.GetTileSafely(checkX, checkY).TileType == tarStalactiteTileType) {
                                canPlace = false;
                                break;
                            }
                        }
                    }
                    if (canPlace) {
                        if (WorldGenHelper.Place1x2Top(i, j, tarStalactiteTileType, WorldGen.genRand.Next(3), onPlace: (tilePosition) => { ModContent.GetInstance<SolidifiedTarStalactiteTE>().Place(tilePosition.X, tilePosition.Y); })) {
                            WorldGenHelper.ModifiedTileRunner(i, j, WorldGen.genRand.Next(4, 8), WorldGen.genRand.Next(1, 4), TARTILETYPE, ignoreTileTypes: [tarStalactiteTileType]);
                            for (int checkX = i - checkWidth; checkX < i + checkWidth; checkX++) {
                                for (int checkY = j - checkHeight; checkY < j + checkHeight; checkY++) {
                                    if (WorldGenHelper.GetTileSafely(checkX, checkY).TileType == TARTILETYPE && WorldGen.genRand.NextChance(0.75f)) {
                                        WorldGenHelper.Place1x2Top(checkX, checkY + 1, tarStalactiteTileType, WorldGen.genRand.Next(3), onPlace: (tilePosition) => { ModContent.GetInstance<SolidifiedTarStalactiteTE>().Place(tilePosition.X, tilePosition.Y); });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void BuildMagmaMap(Point tileOrigin) {
        for (int i = 0; i < _sourceMagmaMap.GetLength(0); i++) {
            for (int j = 0; j < _sourceMagmaMap.GetLength(1); j++) {
                int i2 = i + tileOrigin.X;
                int j2 = j + tileOrigin.Y;
                _sourceMagmaMap[i, j] = Magma.CreateEmpty(1);
                _targetMagmaMap[i, j] = _sourceMagmaMap[i, j];
            }
        }
    }

    private static bool TooCloseToImportantLocations(Point origin) {
        int x = origin.X;
        int y = origin.Y;
        int num = 150;
        for (int i = x - num; i < x + num; i += 10) {
            if (i <= 0 || i > Main.maxTilesX - 1)
                continue;

            for (int j = y - num; j < y + num; j += 10) {
                if (j > 0 && j <= Main.maxTilesY - 1) {
                    if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == 226)
                        return true;

                    if (Main.tile[i, j].WallType == 83 || Main.tile[i, j].WallType == 3 || Main.tile[i, j].WallType == 87)
                        return true;
                }
            }
        }

        return false;
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
                        _targetMagmaMap[j, k] = Magma.CreateFlow(val, Math.Max(0.0, magma.Resistance - val * 0.0185));
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

                double num5_2 = (j - magmaMapArea.Top) / (float)(magmaMapArea.Bottom - magmaMapArea.Top);
                double num3 = (magmaMapArea.Top - j) / (float)(magmaMapArea.Bottom - j);
                double num = Math.Sin((double)(num3) * 0.25) * 0.7 + 0.8;
                double num2 = 0.5 + 1 / Math.Sqrt(Math.Max(0.0, magma.Pressure - magma.Resistance));
                if (Math.Max(1.0 - Math.Max(0.0, num * num2), magma.Pressure / 10.0) > num * 0.115 + (WorldGen.SolidTile(tileOrigin.X + i, tileOrigin.Y + j) ? 0 : (1f - num5_2))) {
                    //if (TileID.Sets.Ore[tile.TileType])
                    //    tile.ResetToType(tile.TileType);
                    //else
                        tile.ResetToType(type);

                    //tile.WallType = wall;

                    if (num4 < 0.1f && _random.NextChance(0.01f)) {
                        //if (j < magmaMapArea.Bottom - 20) {
                        //    WorldGen.TileRunner(tileOrigin.X + i, tileOrigin.Y + j, _random.Next(20), _random.Next(5), -1);
                        //    WorldGen.TileRunner(tileOrigin.X + i, tileOrigin.Y + j, _random.Next(5, 8), _random.Next(6, 9), -1, addTile: false, -2.0, -0.3);
                        //    WorldGen.TileRunner(tileOrigin.X + i, tileOrigin.Y + j, _random.Next(5, 8), _random.Next(6, 9), -1, addTile: false, 2.0, -0.3);
                        //}
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
                        if (flag2 && j > magmaMapArea.Bottom - 30 - _random.Next(10)) {
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

                Tile tile = GenBase._tiles[tileOrigin.X + i, tileOrigin.Y + j];
                Tile checkTile = _tiles[tileOrigin.X + i, tileOrigin.Y + j + 1];
                if (tile.LiquidAmount > 0 && WorldGen.SolidTile(tileOrigin.X + i, tileOrigin.Y + j + 1) && checkTile.TileType != TARTILETYPE) {
                    int checkJ = tileOrigin.Y + j;
                    for (int checkY = checkJ; checkY > checkJ - 10; checkY--) {
                        checkTile = _tiles[tileOrigin.X + i, checkY];
                        checkTile.LiquidAmount = 0;
                    }
                }

                y += _random.Next(-1, 2);
                int maxRandomY = 3;
                if (y > maxRandomY) {
                    y = maxRandomY;
                }
                if (y < -maxRandomY) {
                    y = -maxRandomY;
                }
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
                    if (_tiles[num, num2].TileType == TARTILETYPE) {
                        if (!GenBase._tiles[num, num2 - 1].HasTile)
                            WorldGen.Place2x1(num, num2 - 1, (ushort)ModContent.TileType<TarRocks2>(), _random.Next(3));

                        if (!GenBase._tiles[num, num2 - 1].HasTile) {
                            ushort tarRocks1 = (ushort)ModContent.TileType<TarRocks1>();
                            WorldGen.Place1x1(num, num2 - 1, tarRocks1);
                            Tile tile = _tiles[num, num2 - 1];
                            if (tile.TileType == tarRocks1) {
                                tile.TileFrameX = (short)(18 * _random.Next(6));
                                tile.TileFrameY = 0;
                            }
                        }
                    }

                }
                if (_tiles[num, num2].LiquidType != 5) {
                    _tiles[num, num2].LiquidAmount = 0;
                }
                ushort distanceToFirstEmptyTile = TileHelper.GetDistanceToFirstEmptyTileAround(num, num2, extraCondition: (tilePosition) => {
                    int x = tilePosition.X, y = tilePosition.Y;
                    for (int i = x - 5; i <= x + 6; i++) {
                        for (int j = y - 5; j <= y + 6; j++) {
                            if (_tiles[i, j].LiquidAmount > 0)
                                return false;
                        }
                    }
                    for (int i = x - 1; i <= x + 2; i++) {
                        for (int j = y - 1; j <= y + 2; j++) {
                            if (!_tiles[i, j].HasTile && _tiles[i, j].WallType != TARWALLTYPE)
                                return false;
                        }
                    }
                    return true;
                });
                double num5_2 = (j - magmaMapArea.Top) / (float)(magmaMapArea.Bottom - magmaMapArea.Top);
                if (i > magmaMapArea.Left + 10 && i < magmaMapArea.Right - 10 &&
                    j > magmaMapArea.Top + 10 && j < magmaMapArea.Bottom - 10) {
                    if (_random.NextChance(1f - num5_2) && distanceToFirstEmptyTile != 0 && distanceToFirstEmptyTile <= 2 && !BadSpotForHoneyFall(num, num2) && !SpotActuallyNotInHive(num, num2)) {
                        CreateBlockedHoneyCube(num, num2);
                        int num5 = (num > tileOrigin.X + magmaMapArea.Width / 2).ToDirectionInt();
                        for (int checkX = -1; checkX <= 2; checkX++) {
                            Tile checkTile = _tiles[num + checkX, num2 + 1];
                            if (checkTile.HasTile) {
                                if (!_tiles[num + checkX - 1, num2 + 1].HasTile || !_tiles[num + checkX + 1, num2 + 1].HasTile) {
                                    WorldGen.PoundTile(num + checkX, num2 + 1);
                                }
                                num5 = (checkX <= 0).ToDirectionInt();
                                break;
                            }
                        }
                        CreateDentForHoneyFall(num, num2, -num5);
                    }
                }
                //if (fastRandom2.Next(8) == 0 && GenBase._tiles[num, num2].HasTile) {
                //    if (!GenBase._tiles[num, num2 + 1].HasTile)
                //        WorldGen.PlaceUncheckedStalactite(num, num2 + 1, fastRandom2.Next(2) == 0, fastRandom2.Next(3), spiders: false);

                //    if (!GenBase._tiles[num, num2 - 1].HasTile)
                //        WorldGen.PlaceUncheckedStalactite(num, num2 - 1, fastRandom2.Next(2) == 0, fastRandom2.Next(3), spiders: false);
                //}
            }
        }

        for (int i = magmaMapArea.Left; i < magmaMapArea.Right; i++) {
            for (int j = magmaMapArea.Top; j < magmaMapArea.Bottom; j++) {
                Magma magma = _sourceMagmaMap[i, j];
                int num = i + tileOrigin.X;
                int num2 = j + tileOrigin.Y;

                FastRandom fastRandom2 = fastRandom.WithModifier(num, num2);
                if (GenBase._tiles[num, num2].HasTile && !_tiles[num, num2].BottomSlope) {
                    if (!GenBase._tiles[num, num2 + 1].HasTile && GenBase._tiles[num, num2 + 1].LiquidAmount <= 0 && WorldGen.genRand.NextChance(_tiles[num, num2].TileType == TARTILETYPE ? 1f : 0.75f))
                        WorldGen.PlaceTile(num, num2 + 1, TARDRIPPINGTILETYPE);
                }
                if (_tiles[num, num2].HasTile && _tiles[num, num2].TileType == TileID.WaterDrip) {
                    _tiles[num, num2].TileType = TARDRIPPINGTILETYPE;
                }
                if (i > magmaMapArea.Left + 10 && i < magmaMapArea.Right - 10 &&
                    j > magmaMapArea.Top + 10 && j < magmaMapArea.Bottom - 10) {
                    if (_tiles[num, num2].LiquidType != 5) {
                        Tile tile = _tiles[num, num2];
                        tile.LiquidType = 5;
                    }
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

    private static bool BadSpotForHoneyFall(int x, int y) {
        if (Main.tile[x, y].TileType == TARTILETYPE && Main.tile[x, y + 1].TileType == TARTILETYPE && Main.tile[x + 1, y].TileType == TARTILETYPE)
            return !Main.tile[x + 1, y + 1].HasTile;

        return true;
    }


    private static bool SpotActuallyNotInHive(int x, int y) {
        for (int i = x - 1; i <= x + 2; i++) {
            for (int j = y - 1; j <= y + 2; j++) {
                if (i < 10 || i > Main.maxTilesX - 10)
                    return true;

                if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType != TARTILETYPE)
                    return true;
            }
        }

        return false;
    }

    private static void CreateDentForHoneyFall(int x, int y, int dir) {
        dir *= -1;
        y++;
        int num = 0;
        while ((num < 4 || WorldGen.SolidTile(x, y)) && x > 10 && x < Main.maxTilesX - 10) {
            num++;
            x += dir;
            if (WorldGen.SolidTile(x, y)) {
                WorldGen.PoundTile(x, y);
                if (!Main.tile[x, y + 1].HasTile) {
                    Tile tile = Main.tile[x, y + 1];
                    tile.HasTile = true;
                    tile.TileType = TARTILETYPE;
                }
            }
        }
    }

    private static void CreateBlockedHoneyCube(int x, int y) {
        for (int i = x - 1; i <= x + 2; i++) {
            for (int j = y - 1; j <= y + 2; j++) {
                Tile tile = Main.tile[i, j];
                if (i >= x && i <= x + 1 && j >= y && j <= y + 1) {
                    tile.HasTile = false;
                    tile.LiquidAmount = 255;
                    tile.LiquidType = 5;
                }
                else {
                    tile.HasTile = true;
                    tile.TileType = TARTILETYPE;
                }
            }
        }
    }
}
