using Microsoft.Xna.Framework;

using System.Linq;
using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Platforms;
using RoA.Content.World.Generations;
using ReLogic.Utilities;
using Terraria.WorldBuilding;
using System.Buffers.Text;

namespace RoA.Core.Utility;

static class WorldGenHelper {
    public static int SafeFloatingIslandY => Main.maxTilesY / 7 - 3;

    public static int WorldSize => SmallWorld ? 1 : MediumWorld ? 2 : 3;
    public static float WorldSize2 => Main.maxTilesX / 4200f - 1f;
    public static bool SmallWorld => Main.maxTilesX == 4200;
    public static bool MediumWorld => Main.maxTilesX == 6400;
    public static bool BigWorld => Main.maxTilesX == 8400;

    public static ushort GetType(int i, int j) => GetTileSafely(i, j).TileType;

    public static bool AnyLiquid(this Tile tile) => tile.LiquidAmount > 0;

    public static bool AnyLiquid(int i, int j) => GetTileSafely(i, j).AnyLiquid();

    public static Tile GetTileSafely(int i, int j) => !WorldGen.InWorld(i, j) ? Framing.GetTileSafely(1, 1) : Framing.GetTileSafely(i, j);

    public static Tile GetTileSafely(Point position) => GetTileSafely(position.X, position.Y);

    public static bool ActiveTile(int i, int j) => GetTileSafely(i, j).HasTile;

    public static bool ActiveTile(this Tile tile, int tileType) => tile.HasTile && tile.TileType == tileType;

    public static bool ActiveTile(int i, int j, int tileType) {
        Tile tile = GetTileSafely(i, j);
        return tile.ActiveTile(tileType);
    }

    public static bool ActiveWall(int i, int j, int wallType) => GetTileSafely(i, j).ActiveWall(wallType);

    public static bool ActiveWall(int i, int j) => !ActiveWall(i, j, WallID.None);

    public static bool ActiveWall(this Tile tile, int wallType) => tile.WallType == wallType;

    public static bool ActiveWall(this Tile tile) => !tile.ActiveWall(WallID.None);

    public static void ReplaceTile(int i, int j, int type, bool item = false, bool mute = true, int style = 0) {
        if (ActiveTile(i, j, type)) {
            return;
        }
        WorldGen.KillTile(i, j, false, false, item);
        WorldGen.PlaceTile(i, j, type, true, mute, -1, style);
        //WorldGen.SquareTileFrame(i, j);
    }

    public static void ReplaceTile(Point position, int type, bool item = false, bool mute = true, int style = 0) => ReplaceTile(position.X, position.Y, type, item, mute, style);

    public static void ReplaceWall(int i, int j, int type, bool mute = true) {
        if (ActiveWall(i, j, type)) {
            return;
        }
        WorldGen.KillWall(i, j, false);
        WorldGen.PlaceWall(i, j, type, mute);
        //WorldGen.SquareWallFrame(i, j);
    }

    public static void ReplaceWall(Point position, int type, bool mute = true) => ReplaceWall(position.X, position.Y, type, mute);

    public static bool NoWall(int i, int j) => !AnyWalls(i, j);
    public static bool AnyWall(this Tile tile) => tile.WallType > WallID.None;
    public static bool AnyWalls(int i, int j) {
        Tile tile = GetTileSafely(i, j);
        return tile.AnyWall();
    }

    public static int GetFirstTileY(int i, bool ignoreWalls = false) {
        int result = SafeFloatingIslandY;
        while (!GetTileSafely(i, result).HasTile) {
            result++;
            if (!ignoreWalls && GetTileSafely(i, result).WallType != WallID.None) {
                break;
            }
        }
        return result;
    }

    public static int GetFirstTileY(int i, int type) {
        int result = SafeFloatingIslandY;
        while (!GetTileSafely(i, result).ActiveTile(type)) {
            result++;
            if (GetTileSafely(i, result).WallType != WallID.None) {
                break;
            }
            if (result > Main.worldSurface) {
                return -1;
            }
        }
        return result;
    }

    public static int GetFirstTileY2(int i, bool skipWater = false, bool skipWalls = false) {
        int result = SafeFloatingIslandY;
        while (!WorldGen.SolidTile(i, result)) {
            result++;
            if (!skipWater && GetTileSafely(i, result).AnyLiquid()) {
                break;
            }
            if (!skipWalls && GetTileSafely(i, result).WallType != WallID.None) {
                break;
            }
        }
        return result;
    }

    public static Point GetSurfacePositionByTileType(int tileType, double maxSurfaceY = 20.0, int worldEdgeX = 5) {
        Point position = Point.Zero;
        bool scanning = false;
        for (int i = worldEdgeX; i < Main.maxTilesX - worldEdgeX; i++) {
            for (int j = 0; j < (int)(Main.worldSurface + maxSurfaceY); j++) {
                Tile tile = GetTileSafely(i, j);
                if (tile.ActiveTile(tileType)) {
                    position.X = i;
                    scanning = true;
                    break;
                }
            }

            if (scanning) {
                break;
            }
        }

        scanning = false;
        for (int i = Main.maxTilesX - worldEdgeX; i > worldEdgeX; i--) {
            for (int j = 0; j < (int)(Main.worldSurface + maxSurfaceY); j++) {
                Tile tile = GetTileSafely(i, j);
                if (tile.ActiveTile(tileType)) {
                    position.Y = i;
                    scanning = true;
                    break;
                }
            }

            if (scanning) {
                break;
            }
        }

        return new Point(position.X, position.Y);
    }

    public static bool TileCountNearby(int tileType, int i, int j, int tileAmountToCheck = 50) {
        int tilesCount = 0;
        int offset = tileAmountToCheck / 2;
        for (int i2 = -offset; i2 < offset; i2++) {
            for (int j2 = -offset; j2 < offset; j2++) {
                int x = i + i2, y = j + j2;
                if (ActiveTile(x, y, tileType)) {
                    tilesCount++;
                }
            }
        }
        return tilesCount > tileAmountToCheck;
    }

    public static bool IsCloud(int i, int j) {
        Tile tile = GetTileSafely(i, j);
        return tile.ActiveTile(TileID.Cloud) || tile.ActiveTile(TileID.RainCloud);
    }

    // adapted vanilla
    public static bool Place2x3(int x, int y, ushort type, int style = 0) {
        int num = style * 36;
        int num2 = 0;
        int num3 = 3;

        bool flag = true;
        Tile tile2;
        for (int i = y - num3 + 1; i < y + 1; i++) {
            tile2 = Main.tile[x, i];
            if (tile2.HasTile) {
                flag = false;
            }
            tile2 = Main.tile[x + 1, i];
            if (tile2.HasTile) {
                flag = false;
            }
        }

        if (flag && WorldGen.SolidTile2(x, y + 1) && WorldGen.SolidTile2(x + 1, y + 1)) {
            for (int j = 0; j < num3; j++) {
                tile2 = Main.tile[x, y - num3 + 1 + j];
                tile2.HasTile = true;
                tile2 = Main.tile[x, y - num3 + 1 + j];
                tile2.TileFrameY = (short)(num2 + j * 18);
                tile2 = Main.tile[x, y - num3 + 1 + j];
                tile2.TileFrameX = (short)num;
                tile2 = Main.tile[x, y - num3 + 1 + j];
                tile2.TileType = type;
                tile2 = Main.tile[x + 1, y - num3 + 1 + j];
                tile2.HasTile = true;
                tile2 = Main.tile[x + 1, y - num3 + 1 + j];
                tile2.TileFrameY = (short)(num2 + j * 18);
                tile2 = Main.tile[x + 1, y - num3 + 1 + j];
                tile2.TileFrameX = (short)(num + 18);
                tile2 = Main.tile[x + 1, y - num3 + 1 + j];
                tile2.TileType = type;
            }
            return true;
        }
        return false;
    }

    // adapted vanilla 
    public static void PlaceVines(int x, int y, int numVines, ushort vineType, bool finished = false) {
        for (int j = y; j <= y + numVines && !finished; j++) {
            Tile tileBelow = Framing.GetTileSafely(x, j + 1);

            if ((!tileBelow.HasTile || tileBelow.TileType == TileID.Cobweb) && WorldGen.InWorld(x, j)) {
                WorldGen.PlaceTile(x, j, vineType);
            }
            else {
                finished = true;
            }

            if (numVines <= 1) {
                finished = true;
            }
        }
    }

    // adapted vanilla
    public static void SlopeAreaNatural(int i, int j, int size, ushort? tileType = null) {
        for (int num583 = i - size; num583 < i + size; num583++) {
            for (int num584 = j - size; num584 < j + size; num584++) {
                bool flag = tileType != null && Main.tile[num583, num584].TileType == tileType.Value;
                if (flag) {
                    if (!Main.tile[num583, num584 - 1].HasTile) {
                        if (WorldGen.SolidTile(num583, num584) && TileID.Sets.CanBeClearedDuringGeneration[Main.tile[num583, num584].TileType]) {
                            if (!Main.tile[num583 - 1, num584].IsHalfBlock && !Main.tile[num583 + 1, num584].IsHalfBlock && Main.tile[num583 - 1, num584].Slope == 0 && Main.tile[num583 + 1, num584].Slope == 0) {
                                if (WorldGen.SolidTile(num583, num584 + 1)) {
                                    if (!WorldGen.SolidTile(num583 - 1, num584) && !Main.tile[num583 - 1, num584 + 1].IsHalfBlock && WorldGen.SolidTile(num583 - 1, num584 + 1) && WorldGen.SolidTile(num583 + 1, num584) && !Main.tile[num583 + 1, num584 - 1].HasTile) {
                                        if (WorldGen.genRand.Next(2) == 0)
                                            WorldGen.SlopeTile(num583, num584, 2);
                                        else
                                            WorldGen.PoundTile(num583, num584);
                                    }
                                    else if (!WorldGen.SolidTile(num583 + 1, num584) && !Main.tile[num583 + 1, num584 + 1].IsHalfBlock && WorldGen.SolidTile(num583 + 1, num584 + 1) && WorldGen.SolidTile(num583 - 1, num584) && !Main.tile[num583 - 1, num584 - 1].HasTile) {
                                        if (WorldGen.genRand.Next(2) == 0)
                                            WorldGen.SlopeTile(num583, num584, 1);
                                        else
                                            WorldGen.PoundTile(num583, num584);
                                    }
                                    else if (WorldGen.SolidTile(num583 + 1, num584 + 1) && WorldGen.SolidTile(num583 - 1, num584 + 1) && !Main.tile[num583 + 1, num584].HasTile && !Main.tile[num583 - 1, num584].HasTile) {
                                        WorldGen.PoundTile(num583, num584);
                                    }

                                    if (WorldGen.SolidTile(num583, num584)) {
                                        if (WorldGen.SolidTile(num583 - 1, num584) && WorldGen.SolidTile(num583 + 1, num584 + 2) && !Main.tile[num583 + 1, num584].HasTile && !Main.tile[num583 + 1, num584 + 1].HasTile && !Main.tile[num583 - 1, num584 - 1].HasTile) {
                                            WorldGen.KillTile(num583, num584);
                                        }
                                        else if (WorldGen.SolidTile(num583 + 1, num584) && WorldGen.SolidTile(num583 - 1, num584 + 2) && !Main.tile[num583 - 1, num584].HasTile && !Main.tile[num583 - 1, num584 + 1].HasTile && !Main.tile[num583 + 1, num584 - 1].HasTile) {
                                            WorldGen.KillTile(num583, num584);
                                        }
                                        else if (!Main.tile[num583 - 1, num584 + 1].HasTile && !Main.tile[num583 - 1, num584].HasTile && WorldGen.SolidTile(num583 + 1, num584) && WorldGen.SolidTile(num583, num584 + 2)) {
                                            if (WorldGen.genRand.Next(5) == 0)
                                                WorldGen.KillTile(num583, num584);
                                            else if (WorldGen.genRand.Next(5) == 0)
                                                WorldGen.PoundTile(num583, num584);
                                            else
                                                WorldGen.SlopeTile(num583, num584, 2);
                                        }
                                        else if (!Main.tile[num583 + 1, num584 + 1].HasTile && !Main.tile[num583 + 1, num584].HasTile && WorldGen.SolidTile(num583 - 1, num584) && WorldGen.SolidTile(num583, num584 + 2)) {
                                            if (WorldGen.genRand.Next(5) == 0)
                                                WorldGen.KillTile(num583, num584);
                                            else if (WorldGen.genRand.Next(5) == 0)
                                                WorldGen.PoundTile(num583, num584);
                                            else
                                                WorldGen.SlopeTile(num583, num584, 1);
                                        }
                                    }
                                }

                                if (WorldGen.SolidTile(num583, num584) && !Main.tile[num583 - 1, num584].HasTile && !Main.tile[num583 + 1, num584].HasTile)
                                    WorldGen.KillTile(num583, num584);
                            }
                        }
                        else if (!Main.tile[num583, num584].HasTile && Main.tile[num583, num584 + 1].TileType != 151 && Main.tile[num583, num584 + 1].TileType != 274) {
                            if (Main.tile[num583 + 1, num584].TileType != 190 && Main.tile[num583 + 1, num584].TileType != 48 && Main.tile[num583 + 1, num584].TileType != 232 && WorldGen.SolidTile(num583 - 1, num584 + 1) && WorldGen.SolidTile(num583 + 1, num584) && !Main.tile[num583 - 1, num584].HasTile && !Main.tile[num583 + 1, num584 - 1].HasTile) {
                                if (Main.tile[num583 + 1, num584].TileType == 495)
                                    WorldGen.PlaceTile(num583, num584, Main.tile[num583 + 1, num584].TileType);
                                else
                                    WorldGen.PlaceTile(num583, num584, Main.tile[num583, num584 + 1].TileType);

                                if (WorldGen.genRand.Next(2) == 0)
                                    WorldGen.SlopeTile(num583, num584, 2);
                                else
                                    WorldGen.PoundTile(num583, num584);
                            }

                            if (Main.tile[num583 - 1, num584].TileType != 190 && Main.tile[num583 - 1, num584].TileType != 48 && Main.tile[num583 - 1, num584].TileType != 232 && WorldGen.SolidTile(num583 + 1, num584 + 1) && WorldGen.SolidTile(num583 - 1, num584) && !Main.tile[num583 + 1, num584].HasTile && !Main.tile[num583 - 1, num584 - 1].HasTile) {
                                if (Main.tile[num583 - 1, num584].TileType == 495)
                                    WorldGen.PlaceTile(num583, num584, Main.tile[num583 - 1, num584].TileType);
                                else
                                    WorldGen.PlaceTile(num583, num584, Main.tile[num583, num584 + 1].TileType);

                                if (WorldGen.genRand.Next(2) == 0)
                                    WorldGen.SlopeTile(num583, num584, 1);
                                else
                                    WorldGen.PoundTile(num583, num584);
                            }
                        }
                    }
                    else if (!Main.tile[num583, num584 + 1].HasTile && WorldGen.genRand.Next(2) == 0 && WorldGen.SolidTile(num583, num584) && !Main.tile[num583 - 1, num584].IsHalfBlock && !Main.tile[num583 + 1, num584].IsHalfBlock && Main.tile[num583 - 1, num584].Slope == 0 && Main.tile[num583 + 1, num584].Slope == 0 && WorldGen.SolidTile(num583, num584 - 1)) {
                        if (WorldGen.SolidTile(num583 - 1, num584) && !WorldGen.SolidTile(num583 + 1, num584) && WorldGen.SolidTile(num583 - 1, num584 - 1))
                            WorldGen.SlopeTile(num583, num584, 3);
                        else if (WorldGen.SolidTile(num583 + 1, num584) && !WorldGen.SolidTile(num583 - 1, num584) && WorldGen.SolidTile(num583 + 1, num584 - 1))
                            WorldGen.SlopeTile(num583, num584, 4);
                    }

                    if (TileID.Sets.Conversion.Sand[Main.tile[num583, num584].TileType])
                        Tile.SmoothSlope(num583, num584, applyToNeighbors: false);
                }
            }
        }

        //for (int num585 = i - size; num585 < i + size; num585++) {
        //    for (int num586 = j - size; num586 < j + size; num586++) {
        //        if (WorldGen.genRand.Next(2) == 0 && !Main.tile[num585, num586 - 1].HasTile && Main.tile[num585, num586].TileType != 137 && Main.tile[num585, num586].TileType != 48 && Main.tile[num585, num586].TileType != 232 && Main.tile[num585, num586].TileType != 191 && Main.tile[num585, num586].TileType != 151 && Main.tile[num585, num586].TileType != 274 && Main.tile[num585, num586].TileType != 75 && Main.tile[num585, num586].TileType != 76 && WorldGen.SolidTile(num585, num586) && Main.tile[num585 - 1, num586].TileType != 137 && Main.tile[num585 + 1, num586].TileType != 137) {
        //            if (WorldGen.SolidTile(num585, num586 + 1) && WorldGen.SolidTile(num585 + 1, num586) && !Main.tile[num585 - 1, num586].HasTile)
        //                WorldGen.SlopeTile(num585, num586, 2);

        //            if (WorldGen.SolidTile(num585, num586 + 1) && WorldGen.SolidTile(num585 - 1, num586) && !Main.tile[num585 + 1, num586].HasTile)
        //                WorldGen.SlopeTile(num585, num586, 1);
        //        }

        //        if (Main.tile[num585, num586].Slope == SlopeType.SlopeDownLeft && !WorldGen.SolidTile(num585 - 1, num586)) {
        //            WorldGen.SlopeTile(num585, num586);
        //            WorldGen.PoundTile(num585, num586);
        //        }

        //        if (Main.tile[num585, num586].Slope == SlopeType.SlopeDownRight && !WorldGen.SolidTile(num585 + 1, num586)) {
        //            WorldGen.SlopeTile(num585, num586);
        //            WorldGen.PoundTile(num585, num586);
        //        }
        //    }
        //}
    }

    // adapted vanilla
    public static int PlaceChest(int x, int y, ushort type = 21, bool notNearOtherChests = false, int style = 0, Action<Chest>? onPlaced = null) {
        int num = -1;
        if (TileID.Sets.Boulders[Main.tile[x, y + 1].TileType] || TileID.Sets.Boulders[Main.tile[x + 1, y + 1].TileType])
            return -1;

        if (TileObject.CanPlace(x, y, type, style, 1, out var objectData)) {
            bool flag = true;
            if (notNearOtherChests && Chest.NearOtherChests(x - 1, y - 1))
                flag = false;

            if (flag) {
                TileObject.Place(objectData);
                num = Chest.CreateChest(objectData.xCoord, objectData.yCoord);

                onPlaced?.Invoke(Main.chest[num]);
            }
        }
        else {
            num = -1;
        }

        if (num != -1 && Main.netMode == 1 && type == 21)
            NetMessage.SendData(34, -1, -1, null, 0, x, y, style);

        if (num != -1 && Main.netMode == 1 && type == 467)
            NetMessage.SendData(34, -1, -1, null, 4, x, y, style);

        // Mod chest sync?
        if (num != 1 && Main.netMode == 1 && type >= TileID.Count && TileID.Sets.BasicChest[type])
            NetMessage.SendData(34, -1, -1, null, 100, x, y, style, 0, type, 0);

        return num;
    }

    // adapted vanilla
    public static void Place1x2(int x, int y, ushort type, int styleX, int styleY, Action? onPlaced = null) {
        if (WorldGen.SolidTile2(x, y + 1) & !Framing.GetTileSafely(x, y - 1).HasTile) {
            Tile tile = Framing.GetTileSafely(x, y - 1);
            tile.HasTile = true;
            tile.TileFrameY = (short)styleY;
            tile.TileFrameX = (short)styleX;
            tile.TileType = type;
            tile = Framing.GetTileSafely(x, y);
            tile.HasTile = true;
            tile.TileFrameY = (short)(styleY + 18);
            tile.TileFrameX = (short)styleX;
            tile.TileType = type;
            onPlaced?.Invoke();
        }
    }

    // adapted vanilla
    public static void Place3x2(int x, int y, ushort type, int styleX = 0, int styleY = 0, Action? onPlaced = null) {
        if (x < 5 || x > Main.maxTilesX - 5 || y < 5 || y > Main.maxTilesY - 5)
            return;

        bool flag2 = true;
        int num = y - 1;

        for (int i = x - 1; i < x + 2; i++) {
            for (int j = num; j < y + 1; j++) {
                if (Main.tile[i, j].HasTile)
                    flag2 = false;

                if (type == 215 && Main.tile[i, j].LiquidAmount > 0)
                    flag2 = false;
            }

            switch (type) {
                default:
                    if (type != 582 && type != 619) {
                        if (type == 26 && TileID.Sets.Boulders[Main.tile[i, y + 1].TileType])
                            flag2 = false;

                        if (!WorldGen.SolidTile2(i, y + 1))
                            flag2 = false;

                        break;
                    }
                    goto case 285;
                case 285:
                case 286:
                case 298:
                case 299:
                case 310:
                case 361:
                case 362:
                case 363:
                case 364:
                    if (!WorldGen.SolidTile2(i, y + 1) && (!Main.tile[i, y + 1].HasUnactuatedTile || !Main.tileSolidTop[Main.tile[i, y + 1].TileType] || Main.tile[i, y + 1].TileFrameY != 0))
                        flag2 = false;
                    break;
            }
        }

        if (TileID.Sets.BasicDresser[type]) {
            if (Chest.CreateChest(x - 1, y - 1) == -1)
                flag2 = false;
            else if (Main.netMode == 1)
                NetMessage.SendData(34, -1, -1, null, 2, x, y, styleY);
        }

        if (flag2) {
            short num2 = (short)(54 * styleY);
            short num3 = (short)(54 * styleX);
            Tile tile = GetTileSafely(x - 1, y - 1);
            tile.HasTile = true;
            tile.TileFrameY = 0;
            tile.TileFrameX = (short)(num3 + num2);
            tile.TileType = type;
            tile = GetTileSafely(x, y - 1);
            tile.HasTile = true;
            tile.TileFrameY = 0;
            tile.TileFrameX = (short)(num3 + num2 + 18);
            tile.TileType = type;
            tile = GetTileSafely(x + 1, y - 1);
            tile.HasTile = true;
            tile.TileFrameY = 0;
            tile.TileFrameX = (short)(num3 + num2 + 36);
            tile.TileType = type;
            tile = GetTileSafely(x - 1, y);
            tile.HasTile = true;
            tile.TileFrameY = 18;
            tile.TileFrameX = (short)(num3 + num2);
            tile.TileType = type;
            tile = GetTileSafely(x, y);
            tile.HasTile = true;
            tile.TileFrameY = 18;
            tile.TileFrameX = (short)(num3 + num2 + 18);
            tile.TileType = type;
            tile = GetTileSafely(x + 1, y);
            tile.HasTile = true;
            tile.TileFrameY = 18;
            tile.TileFrameX = (short)(num3 + num2 + 36);
            tile.TileType = type;

            onPlaced?.Invoke();
        }
    }

    // adapted vanilla
    public static void Place2x2(int x, int y, ushort type, int style = 0, Action? onPlaced = null) {
        if (x < 5 || x > Main.maxTilesX - 5 || y < 5 || y > Main.maxTilesY - 5)
            return;

        for (int i = x - 1; i < x + 1; i++) {
            for (int j = y - 1; j < y + 1; j++) {
                Tile tileSafely = Framing.GetTileSafely(i, j);
                if (tileSafely.HasTile || (type == 98 && tileSafely.LiquidAmount > 0))
                    return;
            }

            switch (type) {
                case 95:
                case 126: {
                    Tile tileSafely = Framing.GetTileSafely(i, y - 2);
                    if (!tileSafely.HasUnactuatedTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType])
                        return;

                    break;
                }
                default: {
                    Tile tileSafely = Framing.GetTileSafely(i, y + 1);
                    if (!tileSafely.HasUnactuatedTile || (!WorldGen.SolidTile2(tileSafely) && !Main.tileTable[tileSafely.TileType]))
                        return;

                    break;
                }
                case 132:
                    break;
            }
        }

        x--;
        y--;
        int num = 36;
        for (int k = 0; k < 2; k++) {
            for (int l = 0; l < 2; l++) {
                Tile tileSafely = Main.tile[x + k, y + l];
                tileSafely.HasTile = true;
                tileSafely.TileFrameX = (short)(k * 18);
                tileSafely.TileFrameY = (short)(style * num + l * 18);
                tileSafely.TileType = type;
            }
        }
        onPlaced?.Invoke();
    }

    // adapted vanilla
    public static void Place2x1(int x, int y, ushort type, int style = 0, Action? onPlaced = null) {
        bool flag = false;
        if (WorldGen.SolidTile2(x, y + 1) && WorldGen.SolidTile2(x + 1, y + 1) && !Main.tile[x, y].HasTile && !Main.tile[x + 1, y].HasTile)
            flag = true;
        else if ((type == 29 || type == 103) && Main.tile[x, y + 1].HasTile && Main.tile[x + 1, y + 1].HasTile && Main.tileTable[Main.tile[x, y + 1].TileType] && Main.tileTable[Main.tile[x + 1, y + 1].TileType] && !Main.tile[x, y].HasTile && !Main.tile[x + 1, y].HasTile)
            flag = true;

        if (flag) {
            Tile tile = GetTileSafely(x, y);
            tile.HasTile = true;
            tile.TileFrameY = 0;
            tile.TileFrameX = (short)(36 * style);
            tile.TileType = type;
            tile = GetTileSafely(x + 1, y);
            tile.HasTile = true;
            tile.TileFrameY = 0;
            tile.TileFrameX = (short)(36 * style + 18);
            tile.TileType = type;
            onPlaced?.Invoke();
        }
    }

    // adapted vanilla
    public static void TileWallRunner(int i, int j, double strength, int steps, ushort tileType, ushort wallType, bool addTile = false, double speedX = 0.0, double speedY = 0.0, bool noYChange = false, bool overRide = true, int ignoreTileType = -1) {
        var drunkWorldGen = WorldGen.drunkWorldGen;
        var remixWorldGen = WorldGen.remixWorldGen;
        var genRand = WorldGen.genRand;
        var getGoodWorldGen = WorldGen.getGoodWorldGen;
        if (!GenVars.mudWall) {
            if (drunkWorldGen) {
                strength *= 1.0 + (double)genRand.Next(-80, 81) * 0.01;
                steps = (int)((double)steps * (1.0 + (double)genRand.Next(-80, 81) * 0.01));
            }
            else if (remixWorldGen) {
                strength *= 1.0 + (double)genRand.Next(-50, 51) * 0.01;
            }
            else if (getGoodWorldGen && tileType != 57) {
                strength *= 1.0 + (double)genRand.Next(-80, 81) * 0.015;
                steps += genRand.Next(3);
            }
        }

        double num = strength;
        double num2 = steps;
        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = j;
        Vector2D vector2D2 = default(Vector2D);
        vector2D2.X = (double)genRand.Next(-10, 11) * 0.1;
        vector2D2.Y = (double)genRand.Next(-10, 11) * 0.1;
        if (speedX != 0.0 || speedY != 0.0) {
            vector2D2.X = speedX;
            vector2D2.Y = speedY;
        }

        bool flag = tileType == 368;
        bool flag2 = tileType == 367;
        bool lava = false;
        if (getGoodWorldGen && genRand.Next(4) == 0)
            lava = true;

        var beachDistance = WorldGen.beachDistance;
        while (num > 0.0 && num2 > 0.0) {
            if (drunkWorldGen && genRand.Next(30) == 0) {
                vector2D.X += (double)genRand.Next(-100, 101) * 0.05;
                vector2D.Y += (double)genRand.Next(-100, 101) * 0.05;
            }

            if (vector2D.Y < 0.0 && num2 > 0.0 && tileType == 59)
                num2 = 0.0;

            num = strength * (num2 / (double)steps);
            num2 -= 1.0;
            int num3 = (int)(vector2D.X - num * 0.5);
            int num4 = (int)(vector2D.X + num * 0.5);
            int num5 = (int)(vector2D.Y - num * 0.5);
            int num6 = (int)(vector2D.Y + num * 0.5);
            if (num3 < 1)
                num3 = 1;

            if (num4 > Main.maxTilesX - 1)
                num4 = Main.maxTilesX - 1;

            if (num5 < 1)
                num5 = 1;

            if (num6 > Main.maxTilesY - 1)
                num6 = Main.maxTilesY - 1;

            for (int k = num3; k < num4; k++) {
                if (k < beachDistance + 50 || k >= Main.maxTilesX - beachDistance - 50)
                    lava = false;

                for (int l = num5; l < num6; l++) {
                    if ((drunkWorldGen && l < Main.maxTilesY - 300 && tileType == 57) || (ignoreTileType >= 0 && Main.tile[k, l].HasTile && Main.tile[k, l].TileType == ignoreTileType) || !(Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < strength * 0.5 * (1.0 + (double)genRand.Next(-10, 11) * 0.015)))
                        continue;

                    if (flag && Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < strength * 0.3 * (1.0 + (double)genRand.Next(-10, 11) * 0.01))
                        WorldGen.PlaceWall(k, l, 180, mute: true);

                    if (flag2 && Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < strength * 0.3 * (1.0 + (double)genRand.Next(-10, 11) * 0.01))
                        WorldGen.PlaceWall(k, l, 178, mute: true);

                    if (overRide || !Main.tile[k, l].HasTile) {
                        Tile tile = GetTileSafely(k, l);
                        bool flag3 = false;
                        flag3 = Main.tileStone[tileType] && tile.TileType != 1;
                        if (!TileID.Sets.CanBeClearedDuringGeneration[tile.TileType])
                            flag3 = true;

                        if (addTile) {
                            tile.TileType = tileType;
                            tile.WallType = wallType;
                            tile.HasTile = true;
                            tile.LiquidAmount = 0;
                        }
                    }
                }
            }

            vector2D += vector2D2;
            if ((!drunkWorldGen || genRand.Next(3) != 0) && num > 50.0) {
                vector2D += vector2D2;
                num2 -= 1.0;
                vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                if (num > 100.0) {
                    vector2D += vector2D2;
                    num2 -= 1.0;
                    vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                    vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                    if (num > 150.0) {
                        vector2D += vector2D2;
                        num2 -= 1.0;
                        vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                        vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                        if (num > 200.0) {
                            vector2D += vector2D2;
                            num2 -= 1.0;
                            vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                            vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                            if (num > 250.0) {
                                vector2D += vector2D2;
                                num2 -= 1.0;
                                vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                                vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                                if (num > 300.0) {
                                    vector2D += vector2D2;
                                    num2 -= 1.0;
                                    vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                                    vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                                    if (num > 400.0) {
                                        vector2D += vector2D2;
                                        num2 -= 1.0;
                                        vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                                        vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                                        if (num > 500.0) {
                                            vector2D += vector2D2;
                                            num2 -= 1.0;
                                            vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                                            vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                                            if (num > 600.0) {
                                                vector2D += vector2D2;
                                                num2 -= 1.0;
                                                vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                                                vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                                                if (num > 700.0) {
                                                    vector2D += vector2D2;
                                                    num2 -= 1.0;
                                                    vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                                                    vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                                                    if (num > 800.0) {
                                                        vector2D += vector2D2;
                                                        num2 -= 1.0;
                                                        vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                                                        vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                                                        if (num > 900.0) {
                                                            vector2D += vector2D2;
                                                            num2 -= 1.0;
                                                            vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                                                            vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
            if (drunkWorldGen)
                vector2D2.X += (double)genRand.Next(-10, 11) * 0.25;

            if (vector2D2.X > 1.0)
                vector2D2.X = 1.0;

            if (vector2D2.X < -1.0)
                vector2D2.X = -1.0;

            if (!noYChange) {
                vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
                if (vector2D2.Y > 1.0)
                    vector2D2.Y = 1.0;

                if (vector2D2.Y < -1.0)
                    vector2D2.Y = -1.0;
            }
            else if (tileType != 59 && num < 3.0) {
                if (vector2D2.Y > 1.0)
                    vector2D2.Y = 1.0;

                if (vector2D2.Y < -1.0)
                    vector2D2.Y = -1.0;
            }

            if (tileType == 59 && !noYChange) {
                if (vector2D2.Y > 0.5)
                    vector2D2.Y = 0.5;

                if (vector2D2.Y < -0.5)
                    vector2D2.Y = -0.5;

                if (vector2D.Y < Main.rockLayer + 100.0)
                    vector2D2.Y = 1.0;

                if (vector2D.Y > (double)(Main.maxTilesY - 300))
                    vector2D2.Y = -1.0;
            }
        }
    }

    // adapted vanilla
    public static bool GrowTreeWithBranches<T>(int i, int y, int minHeight = 20, int maxHeight = 30) where T : TreeBranch {
        int num;
        int num1 = y;
        {
            int num2 = 2;
            int num3 = maxHeight;
            if (Terraria.WorldGen.EmptyTileCheck(i - num2, i + num2, num1 - num3, num1 - 1, 20)) {
                bool flag = false;
                bool flag1 = false;
                int num4 = Terraria.WorldGen.genRand.Next(minHeight, maxHeight);
                for (int i1 = num1 - num4; i1 < num1; i1++) {
                    Tile tile = Main.tile[i, i1];
                    tile.TileFrameNumber = (byte)Terraria.WorldGen.genRand.Next(3);
                    tile.HasTile = true;
                    tile.TileType = 5;
                    num = Terraria.WorldGen.genRand.Next(3);
                    int num5 = Terraria.WorldGen.genRand.Next(10);
                    if (i1 == num1 - 1 || i1 == num1 - num4) {
                        num5 = 0;
                    }
                    while (true) {
                        if ((num5 == 5 ? false : num5 != 7) | !flag) {
                            if ((num5 == 6 ? false : num5 != 7) | !flag1) {
                                break;
                            }
                        }
                        num5 = Terraria.WorldGen.genRand.Next(10);
                    }
                    flag = false;
                    flag1 = false;
                    if (num5 == 5 || num5 == 7) {
                        flag = true;
                    }
                    if (num5 == 6 || num5 == 7) {
                        flag1 = true;
                    }
                    if (num5 == 1) {
                        if (num == 0) {
                            Main.tile[i, i1].TileFrameX = 0;
                            Main.tile[i, i1].TileFrameY = 66;
                        }
                        if (num == 1) {
                            Main.tile[i, i1].TileFrameX = 0;
                            Main.tile[i, i1].TileFrameY = 88;
                        }
                        if (num == 2) {
                            Main.tile[i, i1].TileFrameX = 0;
                            Main.tile[i, i1].TileFrameY = 110;
                        }
                    }
                    else if (num5 == 2) {
                        if (num == 0) {
                            Main.tile[i, i1].TileFrameX = 22;
                            Main.tile[i, i1].TileFrameY = 0;
                        }
                        if (num == 1) {
                            Main.tile[i, i1].TileFrameX = 22;
                            Main.tile[i, i1].TileFrameY = 22;
                        }
                        if (num == 2) {
                            Main.tile[i, i1].TileFrameX = 22;
                            Main.tile[i, i1].TileFrameY = 44;
                        }
                    }
                    else if (num5 == 3) {
                        if (num == 0) {
                            Main.tile[i, i1].TileFrameX = 44;
                            Main.tile[i, i1].TileFrameY = 66;
                        }
                        if (num == 1) {
                            Main.tile[i, i1].TileFrameX = 44;
                            Main.tile[i, i1].TileFrameY = 88;
                        }
                        if (num == 2) {
                            Main.tile[i, i1].TileFrameX = 44;
                            Main.tile[i, i1].TileFrameY = 110;
                        }
                    }
                    else if (num5 == 4) {
                        if (num == 0) {
                            Main.tile[i, i1].TileFrameX = 22;
                            Main.tile[i, i1].TileFrameY = 66;
                        }
                        if (num == 1) {
                            Main.tile[i, i1].TileFrameX = 22;
                            Main.tile[i, i1].TileFrameY = 88;
                        }
                        if (num == 2) {
                            Main.tile[i, i1].TileFrameX = 22;
                            Main.tile[i, i1].TileFrameY = 110;
                        }
                    }
                    else if (num5 == 5) {
                        if (num == 0) {
                            Main.tile[i, i1].TileFrameX = 88;
                            Main.tile[i, i1].TileFrameY = 0;
                        }
                        if (num == 1) {
                            Main.tile[i, i1].TileFrameX = 88;
                            Main.tile[i, i1].TileFrameY = 22;
                        }
                        if (num == 2) {
                            Main.tile[i, i1].TileFrameX = 88;
                            Main.tile[i, i1].TileFrameY = 44;
                        }
                    }
                    else if (num5 == 6) {
                        if (num == 0) {
                            Main.tile[i, i1].TileFrameX = 66;
                            Main.tile[i, i1].TileFrameY = 66;
                        }
                        if (num == 1) {
                            Main.tile[i, i1].TileFrameX = 66;
                            Main.tile[i, i1].TileFrameY = 88;
                        }
                        if (num == 2) {
                            Main.tile[i, i1].TileFrameX = 66;
                            Main.tile[i, i1].TileFrameY = 110;
                        }
                    }
                    else if (num5 != 7) {
                        if (num == 0) {
                            Main.tile[i, i1].TileFrameX = 0;
                            Main.tile[i, i1].TileFrameY = 0;
                        }
                        if (num == 1) {
                            Main.tile[i, i1].TileFrameX = 0;
                            Main.tile[i, i1].TileFrameY = 22;
                        }
                        if (num == 2) {
                            Main.tile[i, i1].TileFrameX = 0;
                            Main.tile[i, i1].TileFrameY = 44;
                        }
                    }
                    else {
                        if (num == 0) {
                            Main.tile[i, i1].TileFrameX = 110;
                            Main.tile[i, i1].TileFrameY = 66;
                        }
                        if (num == 1) {
                            Main.tile[i, i1].TileFrameX = 110;
                            Main.tile[i, i1].TileFrameY = 88;
                        }
                        if (num == 2) {
                            Main.tile[i, i1].TileFrameX = 110;
                            Main.tile[i, i1].TileFrameY = 110;
                        }
                    }
                    if (num5 == 5 || num5 == 7) {
                        tile = Main.tile[i - 1, i1];
                        tile.HasTile = true;
                        if (WorldGen.genRand.Next(5) < 3) {
                            tile.TileType = (ushort)ModContent.TileType<T>();
                        }
                        else {
                            tile.TileType = 5;
                            num = Terraria.WorldGen.genRand.Next(3);
                            if (Terraria.WorldGen.genRand.Next(3) >= 2) {
                                if (num == 0) {
                                    Main.tile[i - 1, i1].TileFrameX = 66;
                                    Main.tile[i - 1, i1].TileFrameY = 0;
                                }
                                if (num == 1) {
                                    Main.tile[i - 1, i1].TileFrameX = 66;
                                    Main.tile[i - 1, i1].TileFrameY = 22;
                                }
                                if (num == 2) {
                                    Main.tile[i - 1, i1].TileFrameX = 66;
                                    Main.tile[i - 1, i1].TileFrameY = 44;
                                }
                            }
                            else {
                                if (num == 0) {
                                    Main.tile[i - 1, i1].TileFrameX = 44;
                                    Main.tile[i - 1, i1].TileFrameY = 198;
                                }
                                if (num == 1) {
                                    Main.tile[i - 1, i1].TileFrameX = 44;
                                    Main.tile[i - 1, i1].TileFrameY = 220;
                                }
                                if (num == 2) {
                                    Main.tile[i - 1, i1].TileFrameX = 44;
                                    Main.tile[i - 1, i1].TileFrameY = 242;
                                }
                            }
                        }
                    }
                    if (num5 == 6 || num5 == 7) {
                        tile = Main.tile[i + 1, i1];
                        tile.HasTile = true;
                        if (WorldGen.genRand.Next(5) < 3) {
                            tile.TileType = (ushort)ModContent.TileType<T>();
                        }
                        else {
                            tile.TileType = 5;
                            num = Terraria.WorldGen.genRand.Next(3);
                            if (Terraria.WorldGen.genRand.Next(3) >= 2) {
                                if (num == 0) {
                                    Main.tile[i + 1, i1].TileFrameX = 88;
                                    Main.tile[i + 1, i1].TileFrameY = 66;
                                }
                                if (num == 1) {
                                    Main.tile[i + 1, i1].TileFrameX = 88;
                                    Main.tile[i + 1, i1].TileFrameY = 88;
                                }
                                if (num == 2) {
                                    Main.tile[i + 1, i1].TileFrameX = 88;
                                    Main.tile[i + 1, i1].TileFrameY = 110;
                                }
                            }
                            else {
                                if (num == 0) {
                                    Main.tile[i + 1, i1].TileFrameX = 66;
                                    Main.tile[i + 1, i1].TileFrameY = 198;
                                }
                                if (num == 1) {
                                    Main.tile[i + 1, i1].TileFrameX = 66;
                                    Main.tile[i + 1, i1].TileFrameY = 220;
                                }
                                if (num == 2) {
                                    Main.tile[i + 1, i1].TileFrameX = 66;
                                    Main.tile[i + 1, i1].TileFrameY = 242;
                                }
                            }
                        }
                    }
                }
                int num6 = Terraria.WorldGen.genRand.Next(3);
                bool flag2 = false;
                bool flag3 = false;
                if (Main.tile[i - 1, num1].HasUnactuatedTile && !Main.tile[i - 1, num1].IsHalfBlock && Main.tile[i - 1, num1].Slope == 0 && (Main.tile[i - 1, num1].TileType == (ushort)ModContent.TileType<BackwoodsGrass>() || Main.tile[i - 1, num1].TileType == 23 || Main.tile[i - 1, num1].TileType == 60 || Main.tile[i - 1, num1].TileType == 109 || Main.tile[i - 1, num1].TileType == 147 || Main.tile[i - 1, num1].TileType == 199 || TileLoader.CanGrowModTree(Main.tile[i - 1, num1].TileType))) {
                    flag2 = true;
                }
                if (Main.tile[i + 1, num1].HasUnactuatedTile && !Main.tile[i + 1, num1].IsHalfBlock && Main.tile[i + 1, num1].Slope == 0 && (Main.tile[i + 1, num1].TileType == (ushort)ModContent.TileType<BackwoodsGrass>() || Main.tile[i + 1, num1].TileType == 23 || Main.tile[i + 1, num1].TileType == 60 || Main.tile[i + 1, num1].TileType == 109 || Main.tile[i + 1, num1].TileType == 147 || Main.tile[i + 1, num1].TileType == 199 || TileLoader.CanGrowModTree(Main.tile[i + 1, num1].TileType))) {
                    flag3 = true;
                }
                if (!flag2) {
                    if (num6 == 0) {
                        num6 = 2;
                    }
                    if (num6 == 1) {
                        num6 = 3;
                    }
                }
                if (!flag3) {
                    if (num6 == 0) {
                        num6 = 1;
                    }
                    if (num6 == 2) {
                        num6 = 3;
                    }
                }
                if (flag2 && !flag3) {
                    num6 = 2;
                }
                if (flag3 && !flag2) {
                    num6 = 1;
                }
                if (num6 == 0 || num6 == 1) {
                    Tile tile = Main.tile[i + 1, num1 - 1];
                    tile.HasTile = true;
                    tile.TileType = 5;
                    num = Terraria.WorldGen.genRand.Next(3);
                    if (num == 0) {
                        Main.tile[i + 1, num1 - 1].TileFrameX = 22;
                        Main.tile[i + 1, num1 - 1].TileFrameY = 132;
                    }
                    if (num == 1) {
                        Main.tile[i + 1, num1 - 1].TileFrameX = 22;
                        Main.tile[i + 1, num1 - 1].TileFrameY = 154;
                    }
                    if (num == 2) {
                        Main.tile[i + 1, num1 - 1].TileFrameX = 22;
                        Main.tile[i + 1, num1 - 1].TileFrameY = 176;
                    }
                }
                if (num6 == 0 || num6 == 2) {
                    Tile tile = Main.tile[i - 1, num1 - 1];
                    tile.HasTile = true;
                    tile.TileType = 5;
                    num = Terraria.WorldGen.genRand.Next(3);
                    if (num == 0) {
                        Main.tile[i - 1, num1 - 1].TileFrameX = 44;
                        Main.tile[i - 1, num1 - 1].TileFrameY = 132;
                    }
                    if (num == 1) {
                        Main.tile[i - 1, num1 - 1].TileFrameX = 44;
                        Main.tile[i - 1, num1 - 1].TileFrameY = 154;
                    }
                    if (num == 2) {
                        Main.tile[i - 1, num1 - 1].TileFrameX = 44;
                        Main.tile[i - 1, num1 - 1].TileFrameY = 176;
                    }
                }
                num = Terraria.WorldGen.genRand.Next(3);
                if (num6 == 0) {
                    if (num == 0) {
                        Main.tile[i, num1 - 1].TileFrameX = 88;
                        Main.tile[i, num1 - 1].TileFrameY = 132;
                    }
                    if (num == 1) {
                        Main.tile[i, num1 - 1].TileFrameX = 88;
                        Main.tile[i, num1 - 1].TileFrameY = 154;
                    }
                    if (num == 2) {
                        Main.tile[i, num1 - 1].TileFrameX = 88;
                        Main.tile[i, num1 - 1].TileFrameY = 176;
                    }
                }
                else if (num6 == 1) {
                    if (num == 0) {
                        Main.tile[i, num1 - 1].TileFrameX = 0;
                        Main.tile[i, num1 - 1].TileFrameY = 132;
                    }
                    if (num == 1) {
                        Main.tile[i, num1 - 1].TileFrameX = 0;
                        Main.tile[i, num1 - 1].TileFrameY = 154;
                    }
                    if (num == 2) {
                        Main.tile[i, num1 - 1].TileFrameX = 0;
                        Main.tile[i, num1 - 1].TileFrameY = 176;
                    }
                }
                else if (num6 == 2) {
                    if (num == 0) {
                        Main.tile[i, num1 - 1].TileFrameX = 66;
                        Main.tile[i, num1 - 1].TileFrameY = 132;
                    }
                    if (num == 1) {
                        Main.tile[i, num1 - 1].TileFrameX = 66;
                        Main.tile[i, num1 - 1].TileFrameY = 154;
                    }
                    if (num == 2) {
                        Main.tile[i, num1 - 1].TileFrameX = 66;
                        Main.tile[i, num1 - 1].TileFrameY = 176;
                    }
                }
                if (Terraria.WorldGen.genRand.Next(8) == 0) {
                    num = Terraria.WorldGen.genRand.Next(3);
                    if (num == 0) {
                        Main.tile[i, num1 - num4].TileFrameX = 0;
                        Main.tile[i, num1 - num4].TileFrameY = 198;
                    }
                    if (num == 1) {
                        Main.tile[i, num1 - num4].TileFrameX = 0;
                        Main.tile[i, num1 - num4].TileFrameY = 220;
                    }
                    if (num == 2) {
                        Main.tile[i, num1 - num4].TileFrameX = 0;
                        Main.tile[i, num1 - num4].TileFrameY = 242;
                    }
                }
                else {
                    num = Terraria.WorldGen.genRand.Next(3);
                    if (num == 0) {
                        Main.tile[i, num1 - num4].TileFrameX = 22;
                        Main.tile[i, num1 - num4].TileFrameY = 198;
                    }
                    if (num == 1) {
                        Main.tile[i, num1 - num4].TileFrameX = 22;
                        Main.tile[i, num1 - num4].TileFrameY = 220;
                    }
                    if (num == 2) {
                        Main.tile[i, num1 - num4].TileFrameX = 22;
                        Main.tile[i, num1 - num4].TileFrameY = 242;
                    }
                }
                Terraria.WorldGen.RangeFrame(i - 2, num1 - num4 - 1, i + 2, num1 + 1);
                if (Main.netMode == NetmodeID.Server) {
                    NetMessage.SendTileSquare(-1, i, (int)(num1 - num4 * 0.5), num4 + 1, TileChangeType.None);
                }
                return true;
            }
        }
        return false;
    }

    // adapted vanilla
    public static void ModifiedTileRunnerForBackwoods(int i, int j, double strength, int steps, int tileType,
                                                      int wallType, bool addTile = false,
                                                      float speedX = 0f, float speedY = 0f, bool noYChange = false, bool placeWalls = false, bool replaceWalls = true, bool noTiles = false) {
        double num = strength;
        float num2 = (float)steps;
        Vector2 pos;
        pos.X = (float)i;
        pos.Y = (float)j;
        Vector2 randVect;
        randVect.X = (float)WorldGen.genRand.Next(-10, 11) * 0.1f;
        randVect.Y = (float)WorldGen.genRand.Next(-10, 11) * 0.1f;
        if (speedX != 0f || speedY != 0f) {
            randVect.X = speedX;
            randVect.Y = speedY;
        }

        while (num > 0.0 && num2 > 0f) {
            num = strength * (double)(num2 / (float)steps);
            num2 -= 1f;
            int num3 = (int)((double)pos.X - num * 0.5);
            int num4 = (int)((double)pos.X + num * 0.5);
            int num5 = (int)((double)pos.Y - num * 0.5);
            int num6 = (int)((double)pos.Y + num * 0.5);
            if (num3 < 1) {
                num3 = 1;
            }
            if (num5 < 1) {
                num5 = 1;
            }
            if (num6 > Main.maxTilesY - 1) {
                num6 = Main.maxTilesY - 1;
            }
            for (int k = num3; k < num4; k++) {
                for (int l = num5; l < num6; l++) {
                    if ((double)(Math.Abs((float)k - pos.X) + Math.Abs((float)l - pos.Y)) < strength * 0.5 * (1.0 + (double)WorldGen.genRand.Next(-10, 11) * 0.015)) {
                        float divide = 7.5f;
                        int heightLimit = (int)Main.worldSurface - (Main.maxTilesY / (int)divide);

                        if (l > heightLimit && !IsCloud(k, l)) {
                            int[] Kill = { 19, 24, 27, 61, 71, 73, 74, 80, 81, 82, 83, 84, 110, 113, 129,
                                162, 165, 184, 185, 186, 187, 201, 227, 233, 236, 254, 324, 444, 461, 3, 21,
                                63, 64, 65, 66, 67, 68, 192, 10, 11, 12, 14, 15, 16, 17, 18, 19, 26, 28, 31,
                                32, 33, 34, 42, 79, 86, 87, 88, 89, 90, 91, 92, 93, 100, 101, 104, 105, 374 };

                            if ((!BackwoodsBiomePass.SkipBiomeInvalidWallTypeToKill.Contains(Main.tile[k, l].WallType) &&
                                 !BackwoodsBiomePass.SandInvalidTileTypesToKill.Contains(Main.tile[k, l].TileType) &&
                                 !BackwoodsBiomePass.SandInvalidWallTypesToKill.Contains(Main.tile[k, l].WallType) &&
                                 !BackwoodsBiomePass.MidInvalidTileTypesToKill.Contains(Main.tile[k, l].TileType) &&
                                 Main.tile[k, l].TileType != TileID.Sand) && !AnyLiquid(k, l)) {
                                if (!noTiles) {
                                    if (Kill.Contains(Main.tile[k, l].TileType)) {
                                        WorldGen.KillTile(k, l);
                                    }

                                    if (Main.tile[k, l].TileType != tileType && !Kill.Contains(Main.tile[k, l].TileType)) {
                                        ReplaceTile(k, l, (ushort)tileType);
                                    }

                                    if (Main.tile[k, l].TileType != tileType && addTile) {
                                        ReplaceTile(k, l, (ushort)tileType);
                                    }
                                }

                                if (Main.tile[k, l].WallType > 0 && replaceWalls) {
                                    Main.tile[k, l].WallType = (ushort)wallType;
                                }

                                if (Main.tile[k, l].HasTile && Main.tile[k - 1, l].HasTile && Main.tile[k + 1, l].HasTile && placeWalls) {
                                    Main.tile[k, l + 1].WallType = (ushort)wallType;
                                }
                            }
                        }
                    }
                }
            }

            pos += randVect;
            if (num > 50.0) {
                pos += randVect;
                num2 -= 1f;
                randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                if (num > 100.0) {
                    pos += randVect;
                    num2 -= 1f;
                    randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                    randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                    if (num > 150.0) {
                        pos += randVect;
                        num2 -= 1f;
                        randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                        randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                        if (num > 200.0) {
                            pos += randVect;
                            num2 -= 1f;
                            randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                            randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                            if (num > 250.0) {
                                pos += randVect;
                                num2 -= 1f;
                                randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                if (num > 300.0) {
                                    pos += randVect;
                                    num2 -= 1f;
                                    randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                    randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                    if (num > 400.0) {
                                        pos += randVect;
                                        num2 -= 1f;
                                        randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                        randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                        if (num > 500.0) {
                                            pos += randVect;
                                            num2 -= 1f;
                                            randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                            randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                            if (num > 600.0) {
                                                pos += randVect;
                                                num2 -= 1f;
                                                randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                                randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                                if (num > 700.0) {
                                                    pos += randVect;
                                                    num2 -= 1f;
                                                    randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                                    randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                                    if (num > 800.0) {
                                                        pos += randVect;
                                                        num2 -= 1f;
                                                        randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                                        randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                                        if (num > 900.0) {
                                                            pos += randVect;
                                                            num2 -= 1f;
                                                            randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                                            randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            randVect.X += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
            if (randVect.X > 1f) {
                randVect.X = 1f;
            }
            if (randVect.X < -1f) {
                randVect.X = -1f;
            }
            if (!noYChange) {
                randVect.Y += (float)WorldGen.genRand.Next(-10, 11) * 0.05f;
                if (randVect.Y > 1f) {
                    randVect.Y = 1f;
                }
                if (randVect.Y < -1f) {
                    randVect.Y = -1f;
                }
            }
        }
    }
}
