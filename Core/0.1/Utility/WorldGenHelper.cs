using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using RoA.Common.BackwoodsSystems;
using RoA.Content.Tiles.Platforms;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.World.Generations;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Core.Utility;

static class WorldGenHelper {
    public static int TILESIZE = 16;

    sealed class WorldGenHelperVars : ModSystem {
        public static int worldSurfaceLow = 0;

        private void GetWorldSurfaceLow(GenerationProgress progress, GameConfiguration configuration) => worldSurfaceLow = (int)GenVars.worldSurfaceLow;
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) => tasks.Insert(5, new PassLegacy(string.Empty, GetWorldSurfaceLow));

        public override void OnWorldLoad() => worldSurfaceLow = 0;
        public override void OnWorldUnload() => worldSurfaceLow = 0;

        public override void SaveWorldData(TagCompound tag) => tag[RoA.ModName + "backwoods" + nameof(worldSurfaceLow)] = worldSurfaceLow;
        public override void LoadWorldData(TagCompound tag) => worldSurfaceLow = tag.GetInt(RoA.ModName + "backwoods" + nameof(worldSurfaceLow));
    }

    public static bool CustomEmptyTileCheck(int startX, int endX, int startY, int endY, params int[] ignoreIDs) {
        if (startX < 0)
            return false;

        if (endX >= Main.maxTilesX)
            return false;

        if (startY < 0)
            return false;

        if (endY >= Main.maxTilesY)
            return false;

        bool flag = false;
        if (ignoreIDs.Any(x => TileID.Sets.CommonSapling[x])) {
            flag = true;
        }

        for (int i = startX; i < endX + 1; i++) {
            for (int j = startY; j < endY + 1; j++) {
                if (!Main.tile[i, j].HasTile)
                    continue;

                if (ignoreIDs.Contains(-1)) {
                    return false;
                }

                if (ignoreIDs.Contains(11)) {
                    ushort type = Main.tile[i, j].TileType;
                    if (type == 11)
                        continue;

                    return false;
                }

                if (ignoreIDs.Contains(71)) {
                    ushort type = Main.tile[i, j].TileType;
                    if (type == 71)
                        continue;

                    return false;
                }

                if (!flag && ignoreIDs.Contains(Main.tile[i, j].TileType)) {
                    continue;
                }

                if (flag) {
                    // Extra patch context.
                    if (TileID.Sets.CommonSapling[Main.tile[i, j].TileType])
                        break;

                    /*
					switch (Main.tile[i, j].type) {
						case 3:
						case 24:
						case 32:
						case 61:
						case 62:
						case 69:
						case 71:
						case 73:
						case 74:
						case 82:
						case 83:
						case 84:
						case 110:
						case 113:
						case 184:
						case 201:
						case 233:
						case 352:
						case 485:
						case 529:
						case 530:
						case 637:
						case 655:
							continue;
					}
					*/

                    // Be sure to keep this set updated with IDs from the above.
                    if (TileID.Sets.IgnoredByGrowingSaplings[Main.tile[i, j].TileType])
                        continue;

                    return false;
                }
            }
        }

        return true;
    }

    public static bool CustomEmptyTileCheck2(int startX, int endX, int startY, int endY, params int[] ignoreIDs) {
        if (startX < 0)
            return false;

        if (endX >= Main.maxTilesX)
            return false;

        if (startY < 0)
            return false;

        if (endY >= Main.maxTilesY)
            return false;

        bool flag = false;
        for (int i = 0; i < ignoreIDs.Length; i++) {
            if (TileID.Sets.CommonSapling[i])
                flag = true;
        }

        for (int i = startX; i < endX + 1; i++) {
            for (int j = startY; j < endY + 1; j++) {
                if (!Main.tile[i, j].HasTile)
                    continue;

                if (ignoreIDs.Contains(-1)) {
                    return false;
                }

                if (ignoreIDs.Contains(11)) {
                    ushort type = Main.tile[i, j].TileType;
                    if (type == 11)
                        continue;

                    return false;
                }

                if (ignoreIDs.Contains(71)) {
                    ushort type = Main.tile[i, j].TileType;
                    if (type == 71)
                        continue;

                    return false;
                }

                if (!flag && ignoreIDs.Contains(Main.tile[i, j].TileType)) {
                    continue;
                }

                if (flag) {
                    // Extra patch context.
                    if (TileID.Sets.CommonSapling[Main.tile[i, j].TileType])
                        break;

                    /*
					switch (Main.tile[i, j].type) {
						case 3:
						case 24:
						case 32:
						case 61:
						case 62:
						case 69:
						case 71:
						case 73:
						case 74:
						case 82:
						case 83:
						case 84:
						case 110:
						case 113:
						case 184:
						case 201:
						case 233:
						case 352:
						case 485:
						case 529:
						case 530:
						case 637:
						case 655:
							continue;
					}
					*/

                    // Be sure to keep this set updated with IDs from the above.
                    if (TileID.Sets.IgnoredByGrowingSaplings[Main.tile[i, j].TileType])
                        continue;

                    return false;
                }
            }
        }

        return true;
    }

    public static void CustomWall2(int x, int y, int wallType, int minX, int maxX, Action onSpread, params ushort[] ignoreWallTypes) {
        if (!WorldGen.InWorld(x, y))
            return;

        ushort num = (ushort)wallType;
        int num2 = 0;
        int maxWallOut = WorldGen.maxWallOut2;
        List<Point> list = new List<Point>();
        List<Point> list2 = new List<Point>();
        HashSet<Point> hashSet = new HashSet<Point>();
        list2.Add(new Point(x, y));
        while (list2.Count > 0) {
            list.Clear();
            list.AddRange(list2);
            list2.Clear();
            while (list.Count > 0) {
                Point item = list[0];
                if (!WorldGen.InWorld(item.X, item.Y, 1)) {
                    list.Remove(item);
                    continue;
                }

                hashSet.Add(item);
                list.Remove(item);
                Tile tile = Main.tile[item.X, item.Y];
                if (tile.WallType == num || ignoreWallTypes.Contains(num) || WallID.Sets.CannotBeReplacedByWallSpread[tile.WallType])
                    continue;

                double fluff = 15.0;
                if (!SolidTile(item.X, item.Y)) {
                    bool flag = WallID.Sets.WallSpreadStopsAtAir[num];
                    if (flag && tile.WallType == 0) {
                        list.Remove(item);
                        continue;
                    }

                    num2++;
                    if (num2 >= maxWallOut) {
                        list.Remove(item);
                        continue;
                    }

                    double chance =
                        item.X > maxX ?
                        Math.Clamp(1.0 - (double)Math.Abs(item.X - maxX) / fluff, 0.0, 1.0) :
                        item.X < minX ?
                        Math.Clamp(1.0 - (double)Math.Abs(item.X - minX) / fluff, 0.0, 1.0) : 1.0;
                    if (!ignoreWallTypes.Contains(num) && WorldGen.genRand.NextChance(chance)) {
                        tile.WallType = num;
                        onSpread();
                    }
                    Point item2 = new Point(item.X - 1, item.Y);
                    if (!hashSet.Contains(item2))
                        list2.Add(item2);

                    item2 = new Point(item.X + 1, item.Y);
                    if (!hashSet.Contains(item2))
                        list2.Add(item2);

                    item2 = new Point(item.X, item.Y - 1);
                    if (!hashSet.Contains(item2))
                        list2.Add(item2);

                    item2 = new Point(item.X, item.Y + 1);
                    if (!hashSet.Contains(item2))
                        list2.Add(item2);

                    if (flag) {
                        item2 = new Point(item.X - 1, item.Y - 1);
                        if (!hashSet.Contains(item2))
                            list2.Add(item2);

                        item2 = new Point(item.X + 1, item.Y - 1);
                        if (!hashSet.Contains(item2))
                            list2.Add(item2);

                        item2 = new Point(item.X - 1, item.Y + 1);
                        if (!hashSet.Contains(item2))
                            list2.Add(item2);

                        item2 = new Point(item.X + 1, item.Y + 1);
                        if (!hashSet.Contains(item2))
                            list2.Add(item2);

                        item2 = new Point(item.X - 2, item.Y);
                        if (!hashSet.Contains(item2))
                            list2.Add(item2);

                        item2 = new Point(item.X + 2, item.Y);
                        if (!hashSet.Contains(item2))
                            list2.Add(item2);
                    }
                }
                else if (tile.HasTile) {
                    double chance =
                        item.X > maxX ?
                        Math.Clamp(1.0 - (double)Math.Abs(item.X - maxX) / fluff, 0.0, 1.0) :
                        item.X < minX ?
                        Math.Clamp(1.0 - (double)Math.Abs(item.X - minX) / fluff, 0.0, 1.0) : 1.0;
                    if (!ignoreWallTypes.Contains(num) && WorldGen.genRand.NextChance(chance)) {
                        tile.WallType = num;
                        onSpread();
                    }
                }
            }
        }
    }

    public static bool CustomSolidCollision(Vector2 Position, int Width, int Height, bool[] conditions = null, params ushort[] extraTypes) {
        int value = (int)(Position.X / 16f) - 1;
        int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
        int value3 = (int)(Position.Y / 16f) - 1;
        int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
        int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
        value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
        value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
        value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
        Vector2 vector = default(Vector2);
        for (int i = num; i < value2; i++) {
            for (int j = value3; j < value4; j++) {
                if (Main.tile[i, j] != null && !Main.tile[i, j].IsActuated && Main.tile[i, j].HasTile && ((Main.tileSolid[Main.tile[i, j].TileType] && !Main.tileSolidTop[Main.tile[i, j].TileType]) || (conditions != null && conditions[Main.tile[i, j].TileType]) || extraTypes.Contains(Main.tile[i, j].TileType))) {
                    vector.X = i * 16;
                    vector.Y = j * 16;
                    int num2 = 16;
                    if (Main.tile[i, j].IsHalfBlock) {
                        vector.Y += 8f;
                        num2 -= 8;
                    }

                    if (Position.X + (float)Width > vector.X && Position.X < vector.X + 16f && Position.Y + (float)Height > vector.Y && Position.Y < vector.Y + (float)num2)
                        return true;
                }
            }
        }

        return false;
    }

    public static void CustomSpreadGrass(int i, int j, int dirt = 0, int grass = 2, bool repeat = true, TileColorCache color = default(TileColorCache), int maxY = -1, bool growUnderground = false) {
        try {
            if (!WorldGen.InWorld(i, j, 10) || !Main.tile[i, j].HasTile || Main.tile[i, j].TileType != dirt)
                return;

            if (WorldGen.gen && (grass == 199 || grass == 23)) {
                int num = WorldGen.beachDistance;
                if ((!WorldGen.tenthAnniversaryWorldGen && (double)i > (double)Main.maxTilesX * 0.45 && (double)i <= (double)Main.maxTilesX * 0.55) || i < num || i >= Main.maxTilesX - num)
                    return;
            }
            else if (grass != ModContent.TileType<BackwoodsGrass>() && ((WorldGen.gen || (grass != 199 && grass != 23 && grass != 661 && grass != 662)) && (Main.tile[i, j].TileType != dirt || !Main.tile[i, j].HasTile || ((double)j >= (maxY == -1 ? Main.worldSurface : maxY) && dirt == 0)) && growUnderground)) {
                return;
            }

            int num2 = i - 1;
            int num3 = i + 2;
            int num4 = j - 1;
            int num5 = j + 2;
            if (num2 < 0)
                num2 = 0;

            if (num3 > Main.maxTilesX)
                num3 = Main.maxTilesX;

            if (num4 < 0)
                num4 = 0;

            if (num5 > Main.maxTilesY)
                num5 = Main.maxTilesY;

            bool flag = true;
            for (int k = num2; k < num3; k++) {
                for (int l = num4; l < num5; l++) {
                    if (!Main.tile[k, l].HasTile || !Main.tileSolid[Main.tile[k, l].TileType])
                        flag = false;

                    if (Main.tile[k, l].LiquidType == LiquidID.Lava && Main.tile[k, l].LiquidAmount > 0) {
                        flag = true;
                        break;
                    }
                }
            }

            if (flag || !TileID.Sets.CanBeClearedDuringGeneration[Main.tile[i, j].TileType] || ((grass == 23 || grass == 661) && Main.tile[i, j - 1].TileType == 27) || ((grass == 199 || grass == 662) && Main.tile[i, j - 1].TileType == 27) || (grass == 109 && Main.tile[i, j - 1].TileType == 27))
                return;

            Main.tile[i, j].TileType = (ushort)grass;
            Main.tile[i, j].UseBlockColors(color);
            for (int m = num2; m < num3; m++) {
                for (int n = num4; n < num5; n++) {
                    if (!Main.tile[m, n].HasTile || Main.tile[m, n].TileType != dirt)
                        continue;

                    try {
                        if (repeat && WorldGen.grassSpread < 1000) {
                            WorldGen.grassSpread++;
                            CustomSpreadGrass(m, n, dirt, grass);
                            WorldGen.grassSpread--;
                        }
                    }
                    catch {
                    }
                }
            }
        }
        catch {
        }
    }

    public static bool SolidTile(int i, int j) => SolidTile(new Point(i, j));

    public static bool SolidTile(Point tilePosition) {
        Tile tile = GetTileSafely(tilePosition.X, tilePosition.Y);
        return tile.HasTile && (Main.tileSolid[tile.TileType] /*|| Main.tileSolidTop[tile.TileType]*/ || IsPlatform(tilePosition));
    }

    public static bool SolidTile2(int i, int j) => SolidTile2(new Point(i, j));

    public static bool SolidTile2(Point tilePosition) {
        Tile tile = GetTileSafely(tilePosition.X, tilePosition.Y);
        return tile.HasTile && (Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType] || IsPlatform(tilePosition));
    }

    public static bool SolidTileNoPlatform(int i, int j) => SolidTileNoPlatform(new Point(i, j));

    public static bool SolidTileNoPlatform(Point tilePosition) {
        Tile tile = GetTileSafely(tilePosition.X, tilePosition.Y);
        return tile.HasTile && (Main.tileSolid[tile.TileType]/* || Main.tileSolidTop[tile.TileType]*/) && !IsPlatform(tilePosition);
    }

    public static bool IsPlatform(int i, int j) => IsPlatform(new Point(i, j));

    public static bool IsPlatform(Point tilePosition) {
        Tile tile = GetTileSafely(tilePosition.X, tilePosition.Y);
        return (TileID.Sets.Platforms[tile.TileType] && WorldGen.PlatformProperTopFrame(tile.TileFrameX));
    }

    public static int SafeFloatingIslandY => ModLoader.HasMod("Remnants") ? (WorldGenHelperVars.worldSurfaceLow + 66) : (WorldGenHelperVars.worldSurfaceLow - 22);

    public static int WorldSize => SmallWorld ? 1 : MediumWorld ? 2 : 3;
    public static float WorldSize2 => Main.maxTilesX / 4200f - 1f;
    public static bool SmallWorld => Main.maxTilesX >= 0 && Main.maxTilesX < 6400;
    public static bool MediumWorld => Main.maxTilesX >= 6400 && Main.maxTilesX < 8400;
    public static bool BigWorld => Main.maxTilesX >= 8400;

    public static ushort GetType(int i, int j) => GetTileSafely(i, j).TileType;

    public static bool AnyLiquid(this Tile tile) => tile.LiquidAmount > 0;

    public static bool AnyLiquid(int i, int j) => GetTileSafely(i, j).AnyLiquid();

    public static Tile GetTileSafely(int i, int j) => !WorldGen.InWorld(i, j) ? Framing.GetTileSafely(1, 1) : Framing.GetTileSafely(i, j);

    public static Tile GetTileSafely(Point position) => GetTileSafely(position.X, position.Y);
    public static Tile GetTileSafely(Point16 position) => GetTileSafely(position.X, position.Y);

    public static bool ActiveTile(int i, int j) => GetTileSafely(i, j).HasTile;

    public static bool ActiveTile(Point16 position) => ActiveTile(position.X, position.Y);
    public static bool ActiveTile(Point position) => ActiveTile(position.X, position.Y);

    public static bool ActiveTile(this Tile tile, int tileType) => tile.HasTile && tile.TileType == tileType;

    public static bool ActiveTile2(this Tile tile, int tileType) => tile.ActiveTile(tileType) && tile.Slope == 0 && !tile.IsHalfBlock;

    public static bool ActiveTile(int i, int j, int tileType) {
        Tile tile = GetTileSafely(i, j);
        return tile.ActiveTile(tileType);
    }

    public static bool ActiveTile2(int i, int j, int tileType) {
        Tile tile = GetTileSafely(i, j);
        return tile.ActiveTile2(tileType);
    }

    public static bool ActiveWall(int i, int j, int wallType) => GetTileSafely(i, j).ActiveWall(wallType);

    public static bool ActiveWall(int i, int j) => !ActiveWall(i, j, WallID.None);

    public static bool ActiveWall(this Tile tile, int wallType) => tile.WallType == wallType;

    public static bool ActiveWall(this Tile tile) => !tile.ActiveWall(WallID.None);

    public static void ReplaceTile(int i, int j, int type, bool noItem = true, bool mute = true, int style = 0, bool clearEverything = false) {
        if (GetTileSafely(i, j).HasTile) {
            if (clearEverything) {
                GetTileSafely(i, j).ClearEverything();
            }
            else {
                GetTileSafely(i, j).ClearTile();
            }
            //WorldGen.KillTile(i, j, false, false, noItem);
        }
        WorldGen.PlaceTile(i, j, type, mute, true, -1, style);
        //WorldGen.SquareTileFrame(i, j);
    }

    public static bool Place6x4Wall(int x, int y, ushort type, int style, ushort? wallType = null) {
        int num = x - 2;
        int num2 = y - 2;
        bool flag = true;
        for (int i = num; i < num + 6; i++) {
            for (int j = num2; j < num2 + 4; j++) {
                if (Main.tile[i, j].HasTile || Main.tile[i, j].WallType == WallID.None) {
                    flag = false;
                    break;
                }
            }
        }

        if (!flag)
            return false;

        int num3 = 27;
        int num4 = style / num3 * 108;
        int num5 = style % num3 * 72;
        for (int k = num; k < num + 6; k++) {
            for (int l = num2; l < num2 + 4; l++) {
                Tile tile = Main.tile[k, l];
                if (wallType.HasValue) {
                    tile.WallType = wallType.Value;
                }
            }
        }
        for (int k = num; k < num + 6; k++) {
            for (int l = num2; l < num2 + 4; l++) {
                Tile tile = Main.tile[k, l];
                tile.HasTile = true;
                tile.TileType = type;
                tile.TileFrameX = (short)(num4 + 18 * (k - num));
                tile.TileFrameY = (short)(num5 + 18 * (l - num2));
            }
        }


        return true;
    }

    public static bool PlaceXxXWall(int x, int y, int sizeX, int sizeY, ushort type, int style, ushort? wallType = null) {
        int num = x - 1;
        int num2 = y - 1;
        bool flag = true;
        for (int i = num; i < num + sizeX; i++) {
            for (int j = num2; j < num2 + sizeY; j++) {
                Tile tile = Main.tile[i, j];
                if (tile.HasTile && Main.tileSolid[tile.TileType]) {
                    flag = false;
                    break;
                }
            }
        }

        if (!flag)
            return false;

        int num4 = 0;
        int num5 = style * 72;
        for (int k = num; k < num + sizeX; k++) {
            for (int l = num2; l < num2 + sizeY; l++) {
                Tile tile = Main.tile[k, l];
                if (wallType.HasValue) {
                    tile.WallType = wallType.Value;
                }
            }
        }
        for (int k = num; k < num + sizeY; k++) {
            for (int l = num2; l < num2 + sizeY; l++) {
                Tile tile = Main.tile[k, l];
                tile.HasTile = true;
                tile.TileType = type;
                tile.TileFrameX = (short)(num4 + 18 * (k - num));
                tile.TileFrameY = (short)(num5 + 18 * (l - num2));
            }
        }

        return true;
    }

    public static bool Place4x4Wall(int x, int y, ushort type, int style, ushort? wallType = null) {
        int num = x - 1;
        int num2 = y - 1;
        bool flag = true;
        for (int i = num; i < num + 4; i++) {
            for (int j = num2; j < num2 + 4; j++) {
                if (Main.tile[i, j].HasTile || Main.tile[i, j].WallType == WallID.None) {
                    flag = false;
                    break;
                }
            }
        }

        if (!flag)
            return false;

        int num4 = 0;
        int num5 = style * 72;
        for (int k = num; k < num + 4; k++) {
            for (int l = num2; l < num2 + 4; l++) {
                Tile tile = Main.tile[k, l];
                if (wallType.HasValue) {
                    tile.WallType = wallType.Value;
                }
            }
        }
        for (int k = num; k < num + 4; k++) {
            for (int l = num2; l < num2 + 4; l++) {
                Tile tile = Main.tile[k, l];
                tile.HasTile = true;
                tile.TileType = type;
                tile.TileFrameX = (short)(num4 + 18 * (k - num));
                tile.TileFrameY = (short)(num5 + 18 * (l - num2));
            }
        }

        return true;
    }

    public static bool Place4x3Wall(int x, int y, ushort type, int style, ushort? wallType = null) {
        int num = x - 1;
        int num2 = y - 1;
        bool flag = true;
        for (int i = num; i < num + 4; i++) {
            for (int j = num2; j < num2 + 3; j++) {
                if (Main.tile[i, j].HasTile || Main.tile[i, j].WallType == WallID.None) {
                    flag = false;
                    break;
                }
            }
        }

        if (!flag)
            return false;

        int num4 = 0;
        int num5 = style * 72;
        for (int k = num; k < num + 4; k++) {
            for (int l = num2; l < num2 + 3; l++) {
                Tile tile = Main.tile[k, l];
                if (wallType.HasValue) {
                    tile.WallType = wallType.Value;
                }
            }
        }
        for (int k = num; k < num + 4; k++) {
            for (int l = num2; l < num2 + 3; l++) {
                Tile tile = Main.tile[k, l];
                tile.HasTile = true;
                tile.TileType = type;
                tile.TileFrameX = (short)(num4 + 18 * (k - num));
                tile.TileFrameY = (short)(num5 + 18 * (l - num2));
            }
        }

        return true;
    }

    public static void ReplaceTile(Point position, int type, bool noItem = true, bool mute = true, int style = 0) => ReplaceTile(position.X, position.Y, type, noItem, mute, style);

    public static void ReplaceWall(int i, int j, int type, bool mute = true) {
        if (GetTileSafely(i, j).AnyWall()) {
            WorldGen.KillWall(i, j, false);
        }

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
                if (WorldGen.InWorld(x, y) && ActiveTile(x, y, tileType)) {
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
    public static bool Place2x3(int x, int y, ushort type, int style = 0, bool countCut = true) {
        int num = style * 36;
        int num2 = 0;
        int num3 = 3;

        bool flag = true;
        Tile tile2;
        for (int i = y - num3 + 1; i < y + 1; i++) {
            tile2 = Main.tile[x, i];
            if (tile2.HasTile && ((!Main.tileCut[tile2.TileType] && !countCut) || countCut)) {
                flag = false;
            }
            tile2 = Main.tile[x + 1, i];
            if (tile2.HasTile && ((!Main.tileCut[tile2.TileType] && !countCut) || countCut)) {
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
                int count = 0;
                for (; Framing.GetTileSafely(x, j - count).ActiveTile(vineType); count++) {
                }
                if (count < 8) {
                    WorldGen.PlaceTile(x, j, vineType);
                }
                else {
                    finished = true;
                }
            }
            else {
                finished = true;
            }

            if (numVines <= 1) {
                finished = true;
            }
        }
    }

    public static void GetVineTop(int i, int j, out int x, out int y) {
        x = i;
        y = j;
        Tile tileSafely = Framing.GetTileSafely(x, y);
        if (TileID.Sets.IsVine[tileSafely.TileType]) {
            while (y > 20 && tileSafely.HasTile && TileID.Sets.IsVine[tileSafely.TileType]) {
                y--;
                tileSafely = Framing.GetTileSafely(x, y);
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
    public static void Place1x2(int x, int y, ushort type, int styleX, int styleY, Action? onPlaced = null, byte? paintID = null) {
        if (WorldGen.SolidTile2(x, y + 1) & !Framing.GetTileSafely(x, y - 1).HasTile) {
            Tile tile = Framing.GetTileSafely(x, y - 1);
            tile.HasTile = true;
            tile.TileFrameY = (short)styleY;
            tile.TileFrameX = (short)styleX;
            tile.TileType = type;
            if (paintID != null) {
                tile.TileColor = paintID.Value;
            }
            tile = Framing.GetTileSafely(x, y);
            tile.HasTile = true;
            tile.TileFrameY = (short)(styleY + 18);
            tile.TileFrameX = (short)styleX;
            tile.TileType = type;
            if (paintID != null) {
                tile.TileColor = paintID.Value;
            }
            onPlaced?.Invoke();
        }
    }

    // adapted vanilla
    public static bool Place3x2(int x, int y, ushort type, int styleX = 0, int styleY = 0, Action? onPlaced = null) {
        if (x < 5 || x > Main.maxTilesX - 5 || y < 5 || y > Main.maxTilesY - 5)
            return false;

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

        return flag2;
    }

    // adapted vanilla
    public static bool Place2x2(int x, int y, ushort type, int style = 0, Action? onPlaced = null, bool countCut = true) {
        if (x < 5 || x > Main.maxTilesX - 5 || y < 5 || y > Main.maxTilesY - 5)
            return false;

        for (int i = x - 1; i < x + 1; i++) {
            for (int j = y - 1; j < y + 1; j++) {
                Tile tileSafely = Framing.GetTileSafely(i, j);
                if (((!Main.tileCut[tileSafely.TileType] && !countCut) || countCut) && (tileSafely.HasTile || (type == 98 && tileSafely.LiquidAmount > 0)))
                    return false;
            }

            switch (type) {
                case 95:
                case 126: {
                    Tile tileSafely = Framing.GetTileSafely(i, y - 2);
                    if (!tileSafely.HasUnactuatedTile || !Main.tileSolid[tileSafely.TileType] || Main.tileSolidTop[tileSafely.TileType])
                        return false;

                    break;
                }
                default: {
                    Tile tileSafely = Framing.GetTileSafely(i, y + 1);
                    if (!tileSafely.HasUnactuatedTile || (!WorldGen.SolidTile2(tileSafely) && !Main.tileTable[tileSafely.TileType]))
                        return false;

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

        return true;
    }

    public static bool Place1x2Top(int x, int y, ushort type, int styleX = 0, int styleY = 0, Action<Point16>? onPlace = null) {
        if (WorldGen.SolidTile2(x, y - 1) && !Main.tile[x, y].HasTile && !Main.tile[x, y + 1].HasTile) {
            short num = (short)(styleY * 36);
            short num2 = (short)(styleX * 18);
            Tile tile = Main.tile[x, y];
            tile.HasTile = true;
            tile.TileFrameY = num;
            tile.TileFrameX = num2;
            tile.TileType = type;
            tile = Main.tile[x, y + 1];
            tile.HasTile = true;
            tile.TileFrameY = (short)(num + 18);
            tile.TileFrameX = num2;
            tile.TileType = type;

            onPlace?.Invoke(new Point16(x, y));

            return true;
        }

        return false;
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

    public static void Place1x2Right(int x, int y, ushort type, int width = 18, int style = 0, Action? onPlaced = null) {
        bool flag = false;
        if (WorldGen.SolidTile2(x - 1, y) && WorldGen.SolidTile2(x - 1, y + 1) && !Main.tile[x, y].HasTile && !Main.tile[x, y + 1].HasTile)
            flag = true;

        if (flag) {
            Tile tile = GetTileSafely(x, y);
            tile.HasTile = true;
            tile.TileFrameY = 0;
            tile.TileFrameX = (short)(width * style);
            tile.TileType = type;
            tile = GetTileSafely(x, y + 1);
            tile.HasTile = true;
            tile.TileFrameY = 18;
            tile.TileFrameX = (short)(width * style);
            tile.TileType = type;
            onPlaced?.Invoke();
        }
    }

    public static void Place1x2Left(int x, int y, ushort type, int width = 18, int style = 0, Action? onPlaced = null) {
        bool flag = false;
        if (WorldGen.SolidTile2(x + 1, y) && WorldGen.SolidTile2(x + 1, y + 1) && !Main.tile[x, y].HasTile && !Main.tile[x, y + 1].HasTile)
            flag = true;

        if (flag) {
            Tile tile = GetTileSafely(x, y);
            tile.HasTile = true;
            tile.TileFrameY = 0;
            tile.TileFrameX = (short)(width * style);
            tile.TileType = type;
            tile = GetTileSafely(x, y + 1);
            tile.HasTile = true;
            tile.TileFrameY = 18;
            tile.TileFrameX = (short)(width * style);
            tile.TileType = type;
            onPlaced?.Invoke();
        }
    }

    public static void Place2x1Top(int x, int y, ushort type, int style = 0, Action? onPlaced = null) {
        bool flag = false;
        if (WorldGen.SolidTile2(x, y - 1) && WorldGen.SolidTile2(x + 1, y - 1) && !Main.tile[x, y].HasTile && !Main.tile[x + 1, y].HasTile)
            flag = true;
        else if ((type == 29 || type == 103) && Main.tile[x, y - 1].HasTile && Main.tile[x + 1, y - 1].HasTile && Main.tileTable[Main.tile[x, y - 1].TileType] && Main.tileTable[Main.tile[x + 1, y - 1].TileType] && !Main.tile[x, y].HasTile && !Main.tile[x + 1, y].HasTile)
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
    public static void TileWallRunner(int i, int j, double strength, int steps, ushort tileType, ushort wallType, bool addTile = false, double speedX = 0.0, double speedY = 0.0, bool noYChange = false, bool overRide = true, int ignoreTileType = -1, bool onlyWall = false, bool applySeedSettings = false, bool shouldntHasTile = false, params ushort[] skipWalls) {
        var drunkWorldGen = WorldGen.drunkWorldGen;
        var remixWorldGen = WorldGen.remixWorldGen;
        var genRand = WorldGen.genRand;
        var getGoodWorldGen = WorldGen.getGoodWorldGen;
        if (applySeedSettings) {
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
                            if (!onlyWall) {
                                tile.TileType = tileType;
                                tile.WallType = wallType;
                                tile.HasTile = true;
                                tile.LiquidAmount = 0;
                            }
                            else {
                                if ((shouldntHasTile && (!Main.tile[k, l].HasTile || !Main.tileSolid[Main.tile[k, l].TileType])) || !shouldntHasTile) {
                                    if (!skipWalls.Contains(tile.WallType)) {
                                        tile.WallType = wallType;
                                    }
                                }
                            }
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
    public static bool GrowTreeWithBranches<T>(int i, int y, int minHeight = 20, int maxHeight = 30, int branchChance = 5, bool skipMainCheck = false) where T : TreeBranch {
        UnifiedRandom genRand = WorldGen.genRand;

        int j;
        for (j = y; TileID.Sets.TreeSapling[Main.tile[i, j].TileType]; j++) {
        }

        //if ((Main.tile[i - 1, j - 1].LiquidAmount != 0 || Main.tile[i, j - 1].LiquidAmount != 0 || Main.tile[i + 1, j - 1].LiquidAmount != 0) && !WorldGen.notTheBees)
        //    return false;

        if (skipMainCheck || (!skipMainCheck && Main.tile[i, j].HasUnactuatedTile && !Main.tile[i, j].IsHalfBlock && Main.tile[i, j].Slope == 0 && WorldGen.IsTileTypeFitForTree(Main.tile[i, j].TileType) && ((Main.remixWorld && (double)j > Main.worldSurface) || Main.tile[i, j - 1].WallType == 0 || WorldGen.DefaultTreeWallTest(Main.tile[i, j - 1].WallType)) && ((Main.tile[i - 1, j].HasTile && WorldGen.IsTileTypeFitForTree(Main.tile[i - 1, j].TileType)) || (Main.tile[i + 1, j].HasTile && WorldGen.IsTileTypeFitForTree(Main.tile[i + 1, j].TileType))))) {
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
                        BackwoodsVars.AddBackwoodsTree(i, i1);
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
                            if (WorldGen.genRand.Next(branchChance) < 3) {
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
                            BackwoodsVars.AddBackwoodsTree(i - 1, i1);
                        }
                        if (num5 == 6 || num5 == 7) {
                            tile = Main.tile[i + 1, i1];
                            tile.HasTile = true;
                            if (WorldGen.genRand.Next(branchChance) < 3) {
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
                            BackwoodsVars.AddBackwoodsTree(i + 1, i1);
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
                        BackwoodsVars.AddBackwoodsTree(i + 1, num1 - 1);
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
                        BackwoodsVars.AddBackwoodsTree(i - 1, num1 - 1);
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
        }
        return false;
    }

    // adapted vanilla
    public static void ModifiedDirtyRockRunner(int i, int j, params ushort[] validWallTypes) {
        double num = WorldGen.genRand.Next(2, 6);
        double num2 = WorldGen.genRand.Next(5, 50);
        double num3 = num2;
        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = j;
        Vector2D vector2D2 = default(Vector2D);
        vector2D2.X = (double)WorldGen.genRand.Next(-10, 11) * 0.1;
        vector2D2.Y = (double)WorldGen.genRand.Next(-10, 11) * 0.1;
        while (num > 0.0 && num3 > 0.0) {
            double num4 = num * (num3 / num2);
            num3 -= 1.0;
            int num5 = (int)(vector2D.X - num4 * 0.5);
            int num6 = (int)(vector2D.X + num4 * 0.5);
            int num7 = (int)(vector2D.Y - num4 * 0.5);
            int num8 = (int)(vector2D.Y + num4 * 0.5);
            if (num5 < 0)
                num5 = 0;

            if (num6 > Main.maxTilesX)
                num6 = Main.maxTilesX;

            if (num7 < 0)
                num7 = 0;

            if (num8 > Main.maxTilesY)
                num8 = Main.maxTilesY;

            for (int k = num5; k < num6; k++) {
                for (int l = num7; l < num8; l++) {
                    if (Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < num * 0.5 * (1.0 + (double)WorldGen.genRand.Next(-10, 11) * 0.015) && (Main.tile[k, l].WallType == 2 || (validWallTypes != null && validWallTypes.Contains(Main.tile[k, l].WallType))))
                        Main.tile[k, l].WallType = 59;
                }
            }

            vector2D += vector2D2;
            vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
            if (vector2D2.X > 1.0)
                vector2D2.X = 1.0;

            if (vector2D2.X < -1.0)
                vector2D2.X = -1.0;

            vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
            if (vector2D2.Y > 1.0)
                vector2D2.Y = 1.0;

            if (vector2D2.Y < -1.0)
                vector2D2.Y = -1.0;
        }
    }

    // adapted vanilla
    public static bool ModifiedCanHit(Vector2 Position1, int Width1, int Height1, Vector2 Position2, int Width2, int Height2, int ignoreTileType) {
        int num = (int)(Position1.X + Width1 / 2) / 16;
        int num2 = (int)(Position1.Y + Height1 / 2) / 16;
        int num3 = (int)(Position2.X + Width2 / 2) / 16;
        int num4 = (int)(Position2.Y + Height2 / 2) / 16;
        if (num <= 1)
            num = 1;

        if (num >= Main.maxTilesX)
            num = Main.maxTilesX - 1;

        if (num3 <= 1)
            num3 = 1;

        if (num3 >= Main.maxTilesX)
            num3 = Main.maxTilesX - 1;

        if (num2 <= 1)
            num2 = 1;

        if (num2 >= Main.maxTilesY)
            num2 = Main.maxTilesY - 1;

        if (num4 <= 1)
            num4 = 1;

        if (num4 >= Main.maxTilesY)
            num4 = Main.maxTilesY - 1;

        try {
            do {
                int num5 = Math.Abs(num - num3);
                int num6 = Math.Abs(num2 - num4);
                if (num == num3 && num2 == num4)
                    return true;

                if (num5 > num6) {
                    num = ((num >= num3) ? (num - 1) : (num + 1));
                    if (Main.tile[num, num2 - 1] == null)
                        return false;

                    if (Main.tile[num, num2 + 1] == null)
                        return false;

                    if (!Main.tile[num, num2 - 1].IsActuated && Main.tile[num, num2 - 1].TileType != ignoreTileType && Main.tile[num, num2 - 1].HasTile && Main.tileSolid[Main.tile[num, num2 - 1].TileType] && !Main.tileSolidTop[Main.tile[num, num2 - 1].TileType] && Main.tile[num, num2 - 1].Slope == 0 && !Main.tile[num, num2 - 1].IsHalfBlock &&
                        !Main.tile[num, num2 + 1].IsActuated && Main.tile[num, num2 + 1].TileType != ignoreTileType && Main.tile[num, num2 + 1].HasTile && Main.tileSolid[Main.tile[num, num2 + 1].TileType] && !Main.tileSolidTop[Main.tile[num, num2 + 1].TileType] && Main.tile[num, num2 + 1].Slope == 0 && !Main.tile[num, num2 + 1].IsHalfBlock)
                        return false;
                }
                else {
                    num2 = ((num2 >= num4) ? (num2 - 1) : (num2 + 1));
                    if (Main.tile[num - 1, num2] == null)
                        return false;

                    if (Main.tile[num + 1, num2] == null)
                        return false;

                    if (!Main.tile[num - 1, num2].IsActuated && Main.tile[num - 1, num2].TileType != ignoreTileType && Main.tile[num - 1, num2].HasTile && Main.tileSolid[Main.tile[num - 1, num2].TileType] && !Main.tileSolidTop[Main.tile[num - 1, num2].TileType] && Main.tile[num - 1, num2].Slope == 0 && !Main.tile[num - 1, num2].IsHalfBlock &&
                        !Main.tile[num + 1, num2].IsActuated && Main.tile[num + 1, num2].TileType != ignoreTileType && Main.tile[num + 1, num2].HasTile && Main.tileSolid[Main.tile[num + 1, num2].TileType] && !Main.tileSolidTop[Main.tile[num + 1, num2].TileType] && Main.tile[num + 1, num2].Slope == 0 && !Main.tile[num + 1, num2].IsHalfBlock)
                        return false;
                }

                if (Main.tile[num, num2] == null)
                    return false;
            } while (Main.tile[num, num2].IsActuated || !Main.tile[num, num2].HasTile || Main.tile[num, num2].TileType == ignoreTileType || !Main.tileSolid[Main.tile[num, num2].TileType] || Main.tileSolidTop[Main.tile[num, num2].TileType]);

            return false;
        }
        catch {
            return false;
        }
    }

    public static Func<float> TopSizeFactor = () => 1f;
    public static Func<float> BottomSizeFactor = () => 1f;
    public static Func<float> LeftSizeFactor = () => 1f;
    public static Func<float> RightSizeFactor = () => 1f;

    // adapted vanilla
    public static void ModifiedTileRunner(int i, int j, double strength, int steps, int type = 0, bool addTile = false, double speedX = 0.0, double speedY = 0.0, bool noYChange = false, bool overRide = true, int[] ignoreTileTypes = null, bool applySeedSettings = false, Action? onIteration = null, Predicate<Point16>? onTilePlacement = null, ushort? wallType = null) {
        if (applySeedSettings) {
            if (!GenVars.mudWall) {
                if (WorldGen.drunkWorldGen) {
                    strength *= 1.0 + (double)WorldGen.genRand.Next(-80, 81) * 0.01;
                    steps = (int)((double)steps * (1.0 + (double)WorldGen.genRand.Next(-80, 81) * 0.01));
                }
                else if (WorldGen.remixWorldGen) {
                    strength *= 1.0 + (double)WorldGen.genRand.Next(-50, 51) * 0.01;
                }
                else if (WorldGen.getGoodWorldGen && type != 57) {
                    strength *= 1.0 + (double)WorldGen.genRand.Next(-80, 81) * 0.015;
                    steps += WorldGen.genRand.Next(3);
                }
            }
        }

        double num = strength;
        double num2 = steps;
        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = j;
        Vector2D vector2D2 = default(Vector2D);
        vector2D2.X = (double)WorldGen.genRand.Next(-10, 11) * 0.1;
        vector2D2.Y = (double)WorldGen.genRand.Next(-10, 11) * 0.1;
        if (speedX != 0.0 || speedY != 0.0) {
            vector2D2.X = speedX;
            vector2D2.Y = speedY;
        }

        bool flag = type == 368;
        bool flag2 = type == 367;
        bool lava = false;
        if (WorldGen.getGoodWorldGen && WorldGen.genRand.Next(4) == 0)
            lava = true;

        while (num > 0.0 && num2 > 0.0) {
            if (WorldGen.drunkWorldGen && WorldGen.genRand.Next(30) == 0) {
                vector2D.X += (double)WorldGen.genRand.Next(-100, 101) * 0.05;
                vector2D.Y += (double)WorldGen.genRand.Next(-100, 101) * 0.05;
            }

            if (vector2D.Y < 0.0 && num2 > 0.0 && type == 59)
                num2 = 0.0;

            num = strength * (num2 / (double)steps);
            num2 -= 1.0;
            onIteration?.Invoke();
            int num3 = (int)(vector2D.X - num * 0.5 * LeftSizeFactor());
            int num4 = (int)(vector2D.X + num * 0.5 * RightSizeFactor());
            int num5 = (int)(vector2D.Y - num * 0.5 * TopSizeFactor());
            int num6 = (int)(vector2D.Y + num * 0.5 * BottomSizeFactor());
            if (num3 < 1)
                num3 = 1;

            if (num4 > Main.maxTilesX - 1)
                num4 = Main.maxTilesX - 1;

            if (num5 < 1)
                num5 = 1;

            if (num6 > Main.maxTilesY - 1)
                num6 = Main.maxTilesY - 1;

            for (int k = num3; k < num4; k++) {
                if (k < WorldGen.beachDistance + 50 || k >= Main.maxTilesX - WorldGen.beachDistance - 50)
                    lava = false;

                for (int l = num5; l < num6; l++) {
                    bool shouldIgnoreTile = ignoreTileTypes != null && ignoreTileTypes.Contains(Main.tile[k, l].TileType);
                    if ((WorldGen.drunkWorldGen && l < Main.maxTilesY - 300 && type == 57) || (Main.tile[k, l].HasTile && shouldIgnoreTile) || !(Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < strength * 0.5 * (1.0 + (double)WorldGen.genRand.Next(-10, 11) * 0.015)))
                        continue;

                    if (GenVars.mudWall && (double)l > Main.worldSurface && Main.tile[k, l - 1].WallType != 2 && l < Main.maxTilesY - 210 - WorldGen.genRand.Next(3) && Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < strength * 0.45 * (1.0 + (double)WorldGen.genRand.Next(-10, 11) * 0.01)) {
                        if (l > GenVars.lavaLine - WorldGen.genRand.Next(0, 4) - 50) {
                            if (Main.tile[k, l - 1].WallType != 64 && Main.tile[k, l + 1].WallType != 64 && Main.tile[k - 1, l].WallType != 64 && Main.tile[k + 1, l].WallType != 64)
                                WorldGen.PlaceWall(k, l, 15, mute: true);
                        }
                        else if (Main.tile[k, l - 1].WallType != 15 && Main.tile[k, l + 1].WallType != 15 && Main.tile[k - 1, l].WallType != 15 && Main.tile[k + 1, l].WallType != 15) {
                            WorldGen.PlaceWall(k, l, 64, mute: true);
                        }
                    }

                    if (type < 0) {
                        if (Main.tile[k, l].TileType == 53)
                            continue;

                        Tile tile = Main.tile[k, l];
                        if (type == -2 && Main.tile[k, l].HasTile && (l < GenVars.waterLine || l > GenVars.lavaLine)) {
                            tile.LiquidAmount = byte.MaxValue;
                            if (lava) {
                                tile.LiquidType = LiquidID.Lava;
                            }
                            if (WorldGen.remixWorldGen) {
                                if (l > GenVars.lavaLine && ((double)l < Main.rockLayer - 80.0 || l > Main.maxTilesY - 350) && !WorldGen.oceanDepths(k, l))
                                    tile.LiquidType = LiquidID.Lava;
                            }
                            else if (l > GenVars.lavaLine) {
                                tile.LiquidType = LiquidID.Lava;
                            }
                        }

                        tile.HasTile = false;
                        continue;
                    }

                    if (flag && Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < strength * 0.3 * (1.0 + (double)WorldGen.genRand.Next(-10, 11) * 0.01))
                        WorldGen.PlaceWall(k, l, 180, mute: true);

                    if (flag2 && Math.Abs((double)k - vector2D.X) + Math.Abs((double)l - vector2D.Y) < strength * 0.3 * (1.0 + (double)WorldGen.genRand.Next(-10, 11) * 0.01))
                        WorldGen.PlaceWall(k, l, 178, mute: true);

                    if (overRide || !Main.tile[k, l].HasTile) {
                        Tile tile = Main.tile[k, l];
                        bool flag3 = false;
                        flag3 = Main.tileStone[type] && tile.TileType != 1;
                        if (!TileID.Sets.CanBeClearedDuringGeneration[tile.TileType])
                            flag3 = true;

                        switch (tile.TileType) {
                            case 53:
                                if (type == 59 && GenVars.UndergroundDesertLocation.Contains(k, l))
                                    flag3 = true;
                                if (type == 40)
                                    flag3 = true;
                                if ((double)l < Main.worldSurface && type != 59)
                                    flag3 = true;
                                break;
                            case 45:
                            case 147:
                            case 189:
                            case 190:
                            case 196:
                            case 460:
                                flag3 = true;
                                break;
                            case 396:
                            case 397:
                                flag3 = !TileID.Sets.Ore[type];
                                break;
                            case 1:
                                if (type == 59 && (double)l < Main.worldSurface + (double)WorldGen.genRand.Next(-50, 50))
                                    flag3 = true;
                                break;
                            case 367:
                            case 368:
                                if (type == 59)
                                    flag3 = true;
                                break;
                        }

                        if (!flag3) {
                            if (wallType == null) {
                                if (onTilePlacement == null || onTilePlacement(new Point16(k, l))) {
                                    tile.TileType = (ushort)type;
                                }
                            }
                            else {
                                tile.WallType = wallType.Value;
                            }
                        }
                    }

                    if (addTile) {
                        if (onTilePlacement == null || onTilePlacement(new Point16(k, l))) {
                            Tile tile = Main.tile[k, l];
                            tile.HasTile = true;
                            tile.LiquidAmount = 0;
                        }
                    }

                    if (noYChange && (double)l < Main.worldSurface && type != 59)
                        Main.tile[k, l].WallType = 2;

                    if (type == 59 && l > GenVars.waterLine && Main.tile[k, l].LiquidAmount > 0) {
                        Tile tile = Main.tile[k, l];
                        tile.LiquidAmount = 0;
                    }
                }
            }

            vector2D += vector2D2;
            if ((!WorldGen.drunkWorldGen || WorldGen.genRand.Next(3) != 0) && num > 50.0) {
                vector2D += vector2D2;
                num2 -= 1.0;
                vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                if (num > 100.0) {
                    vector2D += vector2D2;
                    num2 -= 1.0;
                    vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                    vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                    if (num > 150.0) {
                        vector2D += vector2D2;
                        num2 -= 1.0;
                        vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                        vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                        if (num > 200.0) {
                            vector2D += vector2D2;
                            num2 -= 1.0;
                            vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                            vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                            if (num > 250.0) {
                                vector2D += vector2D2;
                                num2 -= 1.0;
                                vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                if (num > 300.0) {
                                    vector2D += vector2D2;
                                    num2 -= 1.0;
                                    vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                    vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                    if (num > 400.0) {
                                        vector2D += vector2D2;
                                        num2 -= 1.0;
                                        vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                        vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                        if (num > 500.0) {
                                            vector2D += vector2D2;
                                            num2 -= 1.0;
                                            vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                            vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                            if (num > 600.0) {
                                                vector2D += vector2D2;
                                                num2 -= 1.0;
                                                vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                                vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                                if (num > 700.0) {
                                                    vector2D += vector2D2;
                                                    num2 -= 1.0;
                                                    vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                                    vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                                    if (num > 800.0) {
                                                        vector2D += vector2D2;
                                                        num2 -= 1.0;
                                                        vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                                        vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                                        if (num > 900.0) {
                                                            vector2D += vector2D2;
                                                            num2 -= 1.0;
                                                            vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                                                            vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
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

            vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
            if (WorldGen.drunkWorldGen)
                vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.25;

            if (vector2D2.X > 1.0)
                vector2D2.X = 1.0;

            if (vector2D2.X < -1.0)
                vector2D2.X = -1.0;

            if (!noYChange) {
                vector2D2.Y += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
                if (vector2D2.Y > 1.0)
                    vector2D2.Y = 1.0;

                if (vector2D2.Y < -1.0)
                    vector2D2.Y = -1.0;
            }
            else if (type != 59 && num < 3.0) {
                if (vector2D2.Y > 1.0)
                    vector2D2.Y = 1.0;

                if (vector2D2.Y < -1.0)
                    vector2D2.Y = -1.0;
            }

            if (type == 59 && !noYChange) {
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

        TopSizeFactor = () => 1f;
        BottomSizeFactor = () => 1f;
        LeftSizeFactor = () => 1f;
        RightSizeFactor = () => 1f;
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
                                 !BackwoodsBiomePass.MidInvalidTileTypesToKill2.Contains(Main.tile[k, l].TileType) &&
                                 Main.tile[k, l].TileType != TileID.Sand)) {
                                if (Main.tile[k, l].AnyLiquid()) {
                                    Main.tile[k, l].LiquidAmount = 0;
                                }
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
