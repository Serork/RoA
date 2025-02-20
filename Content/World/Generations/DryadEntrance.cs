using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using RoA.Content.Tiles.Miscellaneous;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Walls;
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
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Content.World.Generations;

sealed class DryadEntrance : ModSystem {
    private static int _dryadEntranceX, _dryadEntranceY;

    private static ushort PlaceholderTileType => (ushort)ModContent.TileType<LivingElderwood>();
    private static ushort PlaceholderWallType => (ushort)ModContent.WallType<ElderwoodWall3>();

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Mount Caves"));
        tasks.RemoveAt(genIndex);

        tasks.Insert(genIndex, new PassLegacy("Extra Mount Caves", ExtraMountCavesGenerator, 200f));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Mountain Caves"));
        tasks.RemoveAt(genIndex);

        tasks.Insert(genIndex, new PassLegacy("Dryad Entrance", DryadEntranceGenerator, 200f));

        tasks.Add(new PassLegacy("Dryad Entrance Clean Up", DryadEntranceCleanUp));
    }

    private void DryadEntranceCleanUp(GenerationProgress progress, GameConfiguration configuration) {
        int distance = 100;
        for (int x2 = _dryadEntranceX - distance / 2; x2 < _dryadEntranceX + distance / 2; x2++) {
            for (int y2 = _dryadEntranceY - distance / 2; y2 < _dryadEntranceY + distance / 2; y2++) {
                if (Main.tile[x2, y2].TileType == PlaceholderTileType) {
                    Main.tile[x2, y2].TileType = TileID.LivingWood;
                }
                if (Main.tile[x2, y2].WallType == PlaceholderWallType) {
                    Main.tile[x2, y2].WallType = WallID.LivingWoodUnsafe;
                } 
            }
        }
    }

    private void ExtraMountCavesGenerator(GenerationProgress progress, GameConfiguration configuration) {
        GenVars.numMCaves = 0;
        progress.Message = Lang.gen[2].Value;

        bool flag = true;
        while (flag) {
            int num1052 = 0;
            bool flag59 = false;
            bool flag60 = false;
            int fluff = 150 * WorldGenHelper.WorldSize;
            int num1053 = WorldGen.genRand.Next((int)((double)Main.maxTilesX / 2 - fluff), (int)((double)Main.maxTilesX / 2 + fluff));
            while (!flag60) {
                flag60 = true;
                while ((num1053 > Main.maxTilesX / 2 - 90 && num1053 < Main.maxTilesX / 2 + 90) ||
                    Math.Abs(GenVars.UndergroundDesertLocation.Center.X - num1053) < GenVars.UndergroundDesertLocation.Width * 0.75f) {
                    num1053 = WorldGen.genRand.Next((int)((double)Main.maxTilesX / 2 - 150), (int)((double)Main.maxTilesX / 2 + 150));
                }

                for (int num1054 = 0; num1054 < GenVars.numMCaves; num1054++) {
                    if (Math.Abs(num1053 - GenVars.mCaveX[num1054]) < 100) {
                        num1052++;
                        flag60 = false;
                        break;
                    }
                }

                if (num1052 >= Main.maxTilesX / 5) {
                    flag59 = true;
                    break;
                }
            }

            if (!flag59) {
                for (int num1055 = 0; (double)num1055 < Main.worldSurface; num1055++) {
                    if (Main.tile[num1053, num1055].HasTile) {
                        for (int num1056 = num1053 - 50; num1056 < num1053 + 50; num1056++) {
                            for (int num1057 = num1055 - 25; num1057 < num1055 + 25; num1057++) {
                                if (Main.tile[num1056, num1057].HasTile && (Main.tile[num1056, num1057].TileType == 53 || Main.tile[num1056, num1057].TileType == 151 || Main.tile[num1056, num1057].TileType == 274))
                                    flag59 = true;
                            }
                        }

                        if (!flag59) {
                            Mountinater2(num1053, num1055);
                            GenVars.mCaveX[GenVars.numMCaves] = num1053;
                            GenVars.mCaveY[GenVars.numMCaves] = num1055;
                            _dryadEntranceX = num1053;
                            _dryadEntranceY = num1055;
                            GenVars.numMCaves++;
                            flag = false;
                            break;
                        }
                    }
                }
            }
        }

        int num1050 = (int)((double)Main.maxTilesX * 0.001);
        if (WorldGen.remixWorldGen)
            num1050 = (int)((double)num1050 * 1.5);

        for (int num1051 = 0; num1051 < num1050; num1051++) {
            progress.Set((double)num1051 / (double)num1050);
            int num1052 = 0;
            bool flag59 = false;
            bool flag60 = false;
            int num1053 = WorldGen.genRand.Next((int)((double)Main.maxTilesX * 0.25), (int)((double)Main.maxTilesX * 0.75));
            while (!flag60) {
                flag60 = true;
                if (!WorldGen.remixWorldGen) {
                    while (num1053 > Main.maxTilesX / 2 - 90 && num1053 < Main.maxTilesX / 2 + 90) {
                        num1053 = WorldGen.genRand.Next((int)((double)Main.maxTilesX * 0.25), (int)((double)Main.maxTilesX * 0.75));
                    }
                }

                for (int num1054 = 0; num1054 < GenVars.numMCaves; num1054++) {
                    if (Math.Abs(num1053 - GenVars.mCaveX[num1054]) < 100) {
                        num1052++;
                        flag60 = false;
                        break;
                    }
                }

                if (num1052 >= Main.maxTilesX / 5) {
                    flag59 = true;
                    break;
                }
            }

            if (!flag59) {
                for (int num1055 = 0; (double)num1055 < Main.worldSurface; num1055++) {
                    if (Main.tile[num1053, num1055].HasTile) {
                        for (int num1056 = num1053 - 50; num1056 < num1053 + 50; num1056++) {
                            for (int num1057 = num1055 - 25; num1057 < num1055 + 25; num1057++) {
                                if (Main.tile[num1056, num1057].HasTile && (Main.tile[num1056, num1057].TileType == 53 || Main.tile[num1056, num1057].TileType == 151 || Main.tile[num1056, num1057].TileType == 274))
                                    flag59 = true;
                            }
                        }

                        if (!flag59) {
                            WorldGen.Mountinater(num1053, num1055);
                            GenVars.mCaveX[GenVars.numMCaves] = num1053;
                            GenVars.mCaveY[GenVars.numMCaves] = num1055;
                            GenVars.numMCaves++;
                            break;
                        }
                    }
                }
            }
        }
    }

    private static void Mountinater2(int i, int j) {
        double num = WorldGen.genRand.Next(100, 120);
        double num2 = num;
        double num3 = 55;

        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = (double)j + num3 / 2.0;
        Vector2D vector2D2 = default(Vector2D);
        vector2D2.X = (double)WorldGen.genRand.Next(-10, 11) * 0.1;
        vector2D2.Y = (double)WorldGen.genRand.Next(-20, -10) * 0.1;
        while (num > 0.0 && num3 > 0.0) {
            num -= (double)WorldGen.genRand.Next(4);
            num3 -= 1.0;
            int num4 = (int)(vector2D.X - num * 0.5);
            int num5 = (int)(vector2D.X + num * 0.5);
            int num6 = (int)(vector2D.Y - num * 0.5);
            int num7 = (int)(vector2D.Y + num * 0.5);
            if (num4 < 0)
                num4 = 0;

            if (num5 > Main.maxTilesX)
                num5 = Main.maxTilesX;

            if (num6 < 0)
                num6 = 0;

            if (num7 > Main.maxTilesY)
                num7 = Main.maxTilesY;

            num2 = num * (double)WorldGen.genRand.Next(80, 120) * 0.01;
            for (int k = num4; k < num5; k++) {
                for (int l = num6; l < num7; l++) {
                    double num8 = Math.Abs((double)k - vector2D.X);
                    double num9 = Math.Abs((double)l - vector2D.Y);
                    if (Math.Sqrt(num8 * num8 + num9 * num9) < num2 * 0.4 && !Main.tile[k, l].HasTile) {
                        Tile tile = Main.tile[k, l];
                        tile.HasTile = true;
                        Main.tile[k, l].TileType = 0;
                    }
                }
            }

            vector2D += vector2D2;
            vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
            vector2D2.Y += (double)WorldGen.genRand.Next(-10, 6) * 0.05;
            if (vector2D2.X > 0.5)
                vector2D2.X = 0.5;

            if (vector2D2.X < -0.5)
                vector2D2.X = -0.5;

            if (vector2D2.Y > -0.5)
                vector2D2.Y = -0.5;

            if (vector2D2.Y < -1.5)
                vector2D2.Y = -1.5;
        }
    }

    private static void Mountinater3(int i, int j, int denom = 4, int[] ignoreWalls = null) {
        double num = WorldGen.genRand.Next(100, 120) / denom;
        double num2 = num;
        double num3 = 55 / denom;

        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = (double)j + num3 / 2.0;
        Vector2D vector2D2 = default(Vector2D);
        vector2D2.X = (double)WorldGen.genRand.Next(-10, 11) * 0.1;
        vector2D2.Y = (double)WorldGen.genRand.Next(-20, -10) * 0.1;
        while (num > 0.0 && num3 > 0.0) {
            num -= (double)WorldGen.genRand.Next(4);
            num3 -= 1.0;
            int num4 = (int)(vector2D.X - num * 0.5);
            int num5 = (int)(vector2D.X + num * 0.5);
            int num6 = (int)(vector2D.Y - num * 0.5);
            int num7 = (int)(vector2D.Y + num * 0.5);
            if (num4 < 0)
                num4 = 0;

            if (num5 > Main.maxTilesX)
                num5 = Main.maxTilesX;

            if (num6 < 0)
                num6 = 0;

            if (num7 > Main.maxTilesY)
                num7 = Main.maxTilesY;

            num2 = num * (double)WorldGen.genRand.Next(80, 120) * 0.01;
            for (int k = num4; k < num5; k++) {
                for (int l = num6; l < num7; l++) {
                    double num8 = Math.Abs((double)k - vector2D.X);
                    double num9 = Math.Abs((double)l - vector2D.Y);
                    if (Math.Sqrt(num8 * num8 + num9 * num9) < num2 * 0.4 && !Main.tile[k, l].HasTile &&
                        (ignoreWalls == null || !ignoreWalls.Contains(Main.tile[k, l].WallType))) {
                        Tile tile = Main.tile[k, l];
                        tile.HasTile = true;
                        Main.tile[k, l].TileType = 0;
                    }
                }
            }

            vector2D += vector2D2;
            vector2D2.X += (double)WorldGen.genRand.Next(-10, 11) * 0.05;
            vector2D2.Y += (double)WorldGen.genRand.Next(-10, 6) * 0.05;
            if (vector2D2.X > 0.5)
                vector2D2.X = 0.5;

            if (vector2D2.X < -0.5)
                vector2D2.X = -0.5;

            if (vector2D2.Y > -0.5)
                vector2D2.Y = -0.5;

            if (vector2D2.Y < -1.5)
                vector2D2.Y = -1.5;
        }
    }

    private void DryadEntranceGenerator(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = Lang.gen[21].Value;
        for (int num749 = 0; num749 < GenVars.numMCaves; num749++) {
            int i3 = GenVars.mCaveX[num749];
            int j5 = GenVars.mCaveY[num749];
            WorldGen.CaveOpenater(i3, j5);
            WorldGen.Cavinator(i3, j5, WorldGen.genRand.Next(40, 50));
        }

        BuildDryadEntrance(_dryadEntranceX, _dryadEntranceY);
    }

    private void Samples(int i, int j, out int result, out Vector2D result2) {
        double num = 10;
        double num2 = num;
        UnifiedRandom genRand = WorldGen.genRand;
        Vector2D vector2D2 = default(Vector2D);
        int size = 30;
        result = 0;
        result2 = default(Vector2D);
        int directions = 3;
        for (int k2 = 0; k2 < 3; k2++) {
            for (int x = -directions; x < directions + 1; x++) {
                Vector2D vector2D = default(Vector2D);
                vector2D.X = i;
                vector2D.Y = j;
                vector2D2.X = x * 0.01;
                vector2D2.Y = 20 * -0.05;
                Vector2D vector2D22 = default(Vector2D);
                int tileCount = 0;
                int num4 = size;
                while (num4 > 0) {
                    num4--;
                    int num5 = (int)(vector2D.X - num * 0.5);
                    int num6 = (int)(vector2D.X + num * 0.5);
                    int num7 = (int)(vector2D.Y - num * 0.5);
                    int num8 = (int)(vector2D.Y + num * 0.5);
                    if (num5 < 0)
                        num5 = 0;

                    if (num6 > Main.maxTilesX)
                        num6 = Main.maxTilesX;

                    if (num7 < 0)
                        num7 = 0;

                    if (num8 > Main.maxTilesY)
                        num8 = Main.maxTilesY;

                    num2 = num * 100 * 0.01;
                    for (int k = num5; k < num6; k++) {
                        for (int l = num7; l < num8; l++) {
                            double num9 = Math.Abs((double)k - vector2D.X);
                            double num10 = Math.Abs((double)l - vector2D.Y);
                            if (Math.Sqrt(num9 * num9 + num10 * num10) < num2 * 0.4 &&
                                Main.tile[k, l].HasTile) {
                                tileCount++;
                            }
                        }
                    }

                    vector2D += vector2D2;
                    vector2D2.X += x * 0.01;
                    vector2D2.Y += (double)-3 * 0.05;
                    if (vector2D2.X > 2.0)
                        vector2D2.X = 2.0;

                    if (vector2D2.X < -2.0)
                        vector2D2.X = -2.0;

                    if (vector2D2.Y > 0.0)
                        vector2D2.Y = 0.0;

                    if (vector2D2.Y < -2.0)
                        vector2D2.Y = -2.0;
                }

                if (tileCount > result) {
                    result = tileCount;
                    result2 = vector2D2;
                }
            }
        }
    }

    private void BuildDryadEntrance(int i, int j) {
        UnifiedRandom genRand = WorldGen.genRand;
        double num = 10;
        double num2 = num;
        int num3 = 1;
        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = j;
        int size = 20;
        int num4 = size;
        Vector2D vector2D2 = default(Vector2D);
        Samples(i, j, out int result, out Vector2D result2);
        int x = (int)vector2D.X, y = (int)vector2D.Y;
        y -= 3;
        float sizeValue = 0.5f;
        Vector2D vector2D3 = default(Vector2D);
        vector2D3.X = i;
        vector2D3.Y = j;
        ushort tileType = PlaceholderTileType;
        ushort wallType = PlaceholderWallType;
        List<Point> killTiles2 = [];
        while (num4 > 0) {
            num4--;
            int num5 = (int)(vector2D.X - num * 0.5);
            int num6 = (int)(vector2D.X + num * 0.5);
            int num7 = (int)(vector2D.Y - num * 0.5);
            int num8 = (int)(vector2D.Y + num * 0.5);
            if (num5 < 0)
                num5 = 0;

            if (num6 > Main.maxTilesX)
                num6 = Main.maxTilesX;

            if (num7 < 0)
                num7 = 0;

            if (num8 > Main.maxTilesY)
                num8 = Main.maxTilesY;

            num2 = num * (double)genRand.Next(100, 110) * 0.01;
            float value = 1f - MathHelper.Clamp(num4 / (float)size * 0.5f, 0f, 0.9f);
            if (sizeValue < 1.25f && genRand.NextChance(value * 1.25f)) {
                sizeValue += genRand.NextFloat(0.05f, 0.1f) * 0.75f;
            }
            num2 *= sizeValue;
            bool flag = num4 < size / 3;
            bool flag2 = num4 < size - 4;
            bool flag3 = num4 > size - 8;
            if (flag) {
                Mountinater3((int)vector2D.X, (int)vector2D.Y);
            }
            for (int k = num5; k < num6; k++) {
                for (int l = num7; l < num8; l++) {
                    double num9 = Math.Abs((double)k - vector2D.X);
                    double num10 = Math.Abs((double)l - vector2D.Y);
                    if (Math.Sqrt(num9 * num9 + num10 * num10) < num2 * 0.4) {
                        Tile tile = Main.tile[k, l];
                        tile.TileType = tileType;
                        if (flag2) {
                            if (Vector2.Distance(new Vector2(i, j), new Vector2(k, l)) > 5) {
                                tile.WallType = wallType;
                            }
                        }
                        if (flag3) {
                            killTiles2.Add(new Point(k, l));
                        }
                    }
                }
            }

            vector2D += vector2D2;
            vector2D2 = Vector2D.Lerp(vector2D2, result2, num4 / (float)size * 0.2f);
            vector2D2.X = Vector2D.Lerp(vector2D2, result2, num4 / (float)size * 0.2f).X;
            vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
            vector2D2.Y += (double)-3 * 0.05;
            if (vector2D2.X > 2.0)
                vector2D2.X = 2.0;

            if (vector2D2.X < -2.0)
                vector2D2.X = -2.0;

            if (vector2D2.Y > 0.0)
                vector2D2.Y = 0.0;

            if (vector2D2.Y < -2.0)
                vector2D2.Y = -2.0;
        }
        Mountinater3((int)vector2D.X, (int)vector2D.Y, 4);
        ushort placeholderTileType = tileType, placeholderWallType = wallType;
        size = 20;
        WorldGenHelper.TileWallRunner((int)vector2D.X, (int)vector2D.Y, size / 2, size, placeholderTileType, placeholderWallType, overRide: true);
        List<Point> killTiles = [];
        int baseX = x, baseY = y;
        int distance = 100;
        for (int x2 = baseX - distance / 2; x2 < baseX + distance / 2; x2++) {
            for (int y2 = baseY - distance / 2; y2 < baseY + distance / 2; y2++) {
                if (!Main.tile[x2 - 1, y2].ActiveTile2(placeholderTileType) || !Main.tile[x2 + 1, y2].ActiveTile2(placeholderTileType) || !Main.tile[x2, y2 - 1].ActiveTile2(placeholderTileType) || !Main.tile[x2, y2 + 1].ActiveTile2(placeholderTileType) ||
                    !Main.tile[x2 - 1, y2 - 1].ActiveTile2(placeholderTileType) || !Main.tile[x2 + 1, y2 - 1].ActiveTile2(placeholderTileType) || !Main.tile[x2 + 1, y2 + 1].ActiveTile2(placeholderTileType) || !Main.tile[x2 - 1, y2 + 1].ActiveTile2(placeholderTileType)) {
                    Tile tile = WorldGenHelper.GetTileSafely(x2, y2);
                    if (tile.ActiveWall(placeholderWallType)) {
                        if (y2 < Main.worldSurface) {

                        }
                        else {
                            WorldGen.KillWall(x2, y2);
                        }
                    }
                }

                bool flag = true;
                for (int i2 = -2; i2 <= 2; i2++) {
                    for (int j2 = -2; j2 <= 2; j2++) {
                        if (Math.Abs(i2) != 1 && Math.Abs(j2) != 1) {
                            if (!Main.tile[x2 + i2, y2 + j2].ActiveTile(placeholderTileType)) {
                                flag = false;
                            }
                        }
                    }
                }
                if (!(!Main.tile[x2 - 1, y2].ActiveTile(placeholderTileType) || !Main.tile[x2 + 1, y2].ActiveTile(placeholderTileType) || !Main.tile[x2, y2 - 1].ActiveTile(placeholderTileType) || !Main.tile[x2, y2 + 1].ActiveTile(placeholderTileType) ||
                      !Main.tile[x2 - 1, y2 - 1].ActiveTile(placeholderTileType) || !Main.tile[x2 + 1, y2 - 1].ActiveTile(placeholderTileType) || !Main.tile[x2 + 1, y2 + 1].ActiveTile(placeholderTileType) || !Main.tile[x2 - 1, y2 + 1].ActiveTile(placeholderTileType)) &&
                      flag) {
                    Tile tile = WorldGenHelper.GetTileSafely(x2, y2);
                    Point pointPosition = new(x2, y2);
                    if (tile.ActiveWall(placeholderWallType) && !killTiles.Contains(pointPosition)) {
                        killTiles.Add(pointPosition);
                    }
                }
            }
        }
        foreach (Point killPos in killTiles) {
            WorldGen.KillTile(killPos.X, killPos.Y);
        }
        foreach (Point killPos in killTiles2) {
            WorldGen.KillTile(killPos.X, killPos.Y);
        }
        int entranceX = 0, entranceY = 0;
        bool flag4 = false;
        int extraX = 0;
        while (!flag4) {
            for (int k = killTiles.Count - 1; k > 0; k--) {
                Point killPos = killTiles[k];
                i = killPos.X;
                j = killPos.Y;
                if (Vector2.Distance(new Vector2(i, j), new Vector2((int)vector2D3.X, (int)vector2D3.Y)) > size * 1.55f) {
                    if (!Main.tile[i, j].HasTile && !Main.tile[i + 1, j].HasTile) {
                        bool flag6 = Main.tile[i - 1, j].TileType == tileType;
                        bool flag7 = Main.tile[i + 2, j].TileType == tileType;
                        bool flag5 = flag6 || flag7;
                        if (flag5) {
                            bool flag = true;
                            for (int i2 = 0; i2 < 2; i2++) {
                                for (int j2 = 0; j2 < 3; j2++) {
                                    if (Main.tile[i + i2, j - j2].HasTile) {
                                        flag = false;
                                    }
                                }
                            }
                            if (flag) {
                                extraX = flag6 ? genRand.Next(4) : -(8 + genRand.Next(4));
                                entranceX = i + 4 + extraX;
                                entranceY = j - 1;
                                flag4 = true;
                            }
                        }
                    }
                    if (flag4) {
                        break;
                    }
                }
            }
        }
        Point origin = new(entranceX, entranceY);
        Mountinater3((int)(entranceX + extraX / 3f), entranceY - 2, 4, [wallType, WallID.DirtUnsafe, WallID.GrassUnsafe, WallID.FlowerUnsafe, WallID.Cave6Unsafe]);
        ShapeData data = new();
        int num_ = 5;
        int num2_ = 3;
        WorldUtils.Gen(origin, new Shapes.Slime(num_), Actions.Chain(new Modifiers.Blotches(num2_, num2_, num2_, 1, 1.0).Output(data), new Actions.SetTile(tileType, setSelfFrames: true), new Modifiers.OnlyWalls(default(ushort)), new Actions.PlaceWall(wallType)));
        WorldUtils.Gen(origin, new ModShapes.All(data), Actions.Chain(new ClearTile(tileType, wallType), new Actions.SetFrames(frameNeighbors: true), new Modifiers.OnlyWalls(default(ushort)), new Actions.PlaceWall(wallType)));
        distance = 40;
        num4 = (int)(origin.X - distance * 0.5);
        int num5_ = (int)(origin.X + distance * 0.5);
        int num6_ = (int)(origin.Y - distance * 0.5);
        int num7_ = (int)(origin.Y + distance * 0.5);
        for (int x2 = num4; x2 < num5_; x2++) {
            for (int y2 = num6_; y2 < num7_; y2++) {
                if (Main.tile[x2, y2].WallType != wallType) {
                    double num9 = Math.Abs((double)x2 - origin.X);
                    double num10 = Math.Abs((double)y2 - origin.Y);
                    if (Math.Sqrt(num9 * num9 + num10 * num10) < num2 * 0.8) {
                        WorldGenHelper.ReplaceWall (x2, y2, wallType);
                        WorldGenHelper.ReplaceTile(x2, y2, tileType);
                    }
                }
            }
        }
        flag4 = false;
        while (!flag4) {
            Point16 point = data.GetData().ToArray()[genRand.Next(data.GetData().Count)];
            ushort treeDryad = (ushort)ModContent.TileType<TreeDryad>();
            i = origin.X - point.X;
            j = origin.Y - point.Y;
            if (!Main.tile[i - 1, j].HasTile && !Main.tile[i - 2, j].HasTile &&
                !Main.tile[i + 1, j].HasTile && !Main.tile[i + 2, j].HasTile) {
                int altered = genRand.NextBool() ? 2 : 0;
                WorldGen.Place2xX(i, j, treeDryad, altered);
                if (Main.tile[i, j].TileType == treeDryad) {
                    flag4 = true;
                }
            }
        }
    }

    private class ClearTile : GenAction {
        private bool _frameNeighbors;
        private ushort _tileType;
        private ushort _wallType;

        public ClearTile(ushort tileType, ushort wallType, bool frameNeighbors = false) {
            _tileType = tileType;
            _wallType = wallType;
            _frameNeighbors = frameNeighbors;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args) {
            if (Main.tile[x, y].WallType == _wallType) {
                WorldUtils.ClearTile(x, y, _frameNeighbors);
            }
            else {
                WorldGenHelper.ReplaceTile(x, y, _tileType);
            }
            return UnitApply(origin, x, y, args);
        }
    }
}
