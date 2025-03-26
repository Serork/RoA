using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using RoA.Common.Items;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Tiles.Decorations;
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
    private static Point _bigRubblePosition = Point.Zero;
    private static bool _loomPlacedInWorld;

    public override void Load() {
        On_WorldGen.GrowLivingTreePassageRoom += On_WorldGen_GrowLivingTreePassageRoom;
    }

    private void On_WorldGen_GrowLivingTreePassageRoom(On_WorldGen.orig_GrowLivingTreePassageRoom orig, int minl, int minr, int Y) {
        var genRand = WorldGen.genRand;
        int num = genRand.Next(2);
        if (num == 0)
            num = -1;

        int num2 = Y - 2;
        int num3 = (minl + minr) / 2;
        if (num < 0)
            num3--;

        if (num > 0)
            num3++;

        int num4 = genRand.Next(15, 30);
        int num5 = num3 + num4;
        if (num < 0) {
            num5 = num3;
            num3 -= num4;
        }

        for (int i = num3; i < num5; i++) {
            for (int j = Y - 20; j < Y + 10; j++) {
                if (Main.tile[i, j].WallType == 0 && !Main.tile[i, j].HasTile && (double)j < Main.worldSurface)
                    return;
            }
        }

        GenVars.dMinX = num3;
        GenVars.dMaxX = num5;
        if (num < 0)
            GenVars.dMinX -= 40;
        else
            GenVars.dMaxX += 40;

        for (int k = num3; k <= num5; k++) {
            for (int l = num2 - 2; l <= Y + 2; l++) {
                if (Main.tile[k - 1, l].TileType == 40)
                    Main.tile[k - 1, l].TileType = 0;

                if (Main.tile[k + 1, l].TileType == 40)
                    Main.tile[k + 1, l].TileType = 0;

                if (Main.tile[k, l - 1].TileType == 40)
                    Main.tile[k, l - 1].TileType = 0;

                if (Main.tile[k, l + 1].TileType == 40)
                    Main.tile[k, l + 1].TileType = 0;

                if (Main.tile[k, l].WallType != 244 && Main.tile[k, l].TileType != 19) {
                    Tile tile = Main.tile[k, l];
                    tile.HasTile = true;
                    tile.TileType = 191;
                    tile.IsHalfBlock = false;
                }

                if (l >= num2 && l <= Y) {
                    Tile tile = Main.tile[k, l];
                    tile.LiquidAmount = 0;
                    tile.WallType = 244;
                    tile.HasTile = false;
                }
            }
        }

        int i2 = (minl + minr) / 2 + 3 * num;
        WorldGen.PlaceTile(i2, Y, 10, mute: true, forced: false, -1, 7);
        int num6 = genRand.Next(5, 9);
        int num7 = genRand.Next(4, 6);
        if (num < 0) {
            num5 = num3 + num6;
            num3 -= num6;
        }
        else {
            num3 = num5 - num6;
            num5 += num6;
        }

        num2 = Y - num7;
        for (int m = num3 - 2; m <= num5 + 2; m++) {
            for (int n = num2 - 2; n <= Y + 2; n++) {
                if (Main.tile[m - 1, n].TileType == 40)
                    Main.tile[m - 1, n].TileType = 40;

                if (Main.tile[m + 1, n].TileType == 40)
                    Main.tile[m + 1, n].TileType = 40;

                if (Main.tile[m, n - 1].TileType == 40)
                    Main.tile[m, n - 1].TileType = 40;

                if (Main.tile[m, n + 1].TileType == 40)
                    Main.tile[m, n + 1].TileType = 40;

                if (Main.tile[m, n].WallType != 244 && Main.tile[m, n].TileType != 19) {
                    Tile tile = Main.tile[m, n];
                    tile.HasTile = true;
                    tile.TileType = 191;
                    tile.IsHalfBlock = false;
                }

                if (n >= num2 && n <= Y && m >= num3 && m <= num5) {
                    Tile tile = Main.tile[m, n];
                    tile.LiquidAmount = 0;
                    tile.WallType = 244;
                    tile.HasTile = false;
                }
            }
        }

        i2 = num3 - 2;
        if (num < 0)
            i2 = num5 + 2;

        WorldGen.PlaceTile(i2, Y, 10, mute: true, forced: false, -1, 7);
        int num8 = num5;
        if (num < 0)
            num8 = num3;

        int num9 = 2;
        if (genRand.Next(num9) == 0) {
            num9 += 2;
            WorldGen.PlaceTile(num8, Y, 15, mute: true, forced: false, -1, 5);
            if (num < 0) {
                Main.tile[num8, Y - 1].TileFrameX += 18;
                Main.tile[num8, Y].TileFrameX += 18;
            }
        }

        num8 = num5 - 2;
        if (num < 0)
            num8 = num3 + 2;

        WorldGen.PlaceTile(num8, Y, 304, mute: true);
        if (Main.tile[num8, Y].TileType == 304) {
            _loomPlacedInWorld = true;
        }
        num8 = num5 - 4;
        if (num < 0)
            num8 = num3 + 4;

        if (genRand.Next(num9) == 0) {
            WorldGen.PlaceTile(num8, Y, 15, mute: true, forced: false, -1, 5);
            if (num > 0) {
                Main.tile[num8, Y - 1].TileFrameX += 18;
                Main.tile[num8, Y].TileFrameX += 18;
            }
        }

        num8 = num5 - 7;
        if (num < 0)
            num8 = num3 + 8;

        int contain = 832;
        bool summonStaffAdded = false;
        if (genRand.Next(3) == 0) {
            contain = 4281;
            summonStaffAdded = true;
        }
        if (!summonStaffAdded && genRand.Next(5) == 0) {
            contain = ModContent.ItemType<GiantTreeSapling>();
        }
        if (!summonStaffAdded && !ExtraVanillaChestItems._giantTreeSaplingAdded) {
            ExtraVanillaChestItems._giantTreeSaplingAdded = true;
            contain = ModContent.ItemType<GiantTreeSapling>();
        }

        if (WorldGen.remixWorldGen) {
            int num10 = genRand.Next(1, 3);
            for (int num11 = 0; num11 < num10; num11++) {
                bool flag = false;
                while (!flag) {
                    int num12 = genRand.Next(Main.maxTilesX / 8, Main.maxTilesX - Main.maxTilesX / 8);
                    int num13 = genRand.Next((int)Main.rockLayer, Main.maxTilesY - 350);
                    if (!WorldGen.IsTileNearby(num12, num13, 53, 20) && !WorldGen.IsTileNearby(num12, num13, 147, 20) && !WorldGen.IsTileNearby(num12, num13, 59, 20))
                        flag = WorldGen.AddBuriedChest(num12, num13, contain, notNearOtherChests: false, 12, trySlope: false, 0);
                }
            }

            if (WorldGen.crimson)
                WorldGen.AddBuriedChest(num8, Y, 0, notNearOtherChests: false, 14, trySlope: false, 0);
            else
                WorldGen.AddBuriedChest(num8, Y, 0, notNearOtherChests: false, 7, trySlope: false, 0);
        }
        else {
            WorldGen.AddBuriedChest(num8, Y, contain, notNearOtherChests: false, 12, trySlope: false, 0);
        }
    }

    private static ushort PlaceholderTileType => (ushort)ModContent.TileType<LivingElderwood>();
    private static ushort PlaceholderWallType => (ushort)ModContent.WallType<ElderwoodWall3>();

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        _bigRubblePosition = Point.Zero;
        _loomPlacedInWorld = false;

        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Mount Caves"));
        tasks.RemoveAt(genIndex);

        tasks.Insert(genIndex, new PassLegacy("Mount Caves, Dryad Entrance Mount Cave", ExtraMountCavesGenerator, 49.9993f));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Mountain Caves"));
        tasks.RemoveAt(genIndex);

        tasks.Insert(genIndex, new PassLegacy("Mountain Caves, Dryad Entrance", DryadEntranceGenerator, 14.2958f));

        tasks.Add(new PassLegacy("Dryad Entrance", DryadEntranceCleanUp));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Living Trees"));
        tasks.Add(new PassLegacy("Dryad Entrance Loom Placement", DryadEntranceLoomPlacement));
    }

    private void DryadEntranceLoomPlacement(GenerationProgress progress, GameConfiguration configuration) {
        if (_loomPlacedInWorld) {
            return;
        }

        for (int x = -1; x < 2; x++) {
            for (int y = -2; y < 0; y++) {
                WorldGen.KillTile(_bigRubblePosition.X + x, _bigRubblePosition.Y + y);
            }
        }
        WorldGen.PlaceTile(_bigRubblePosition.X, _bigRubblePosition.Y, 304, mute: true);
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
                //if (Main.tile[x2, y2].WallType == ModContent.WallType<LivingBackwoodsLeavesWall2>()) {
                //    Main.tile[x2, y2].WallType = WallID.LivingWoodUnsafe;
                //}
            }
        }

        if (WorldGen.tenthAnniversaryWorldGen) {
            byte livingTreePaintColor = 12, livingTreeWallPaintColor = 12;
            ushort treeDryad = (ushort)ModContent.TileType<TreeDryad>();
            for (int i = _dryadEntranceX - distance / 2; i < _dryadEntranceX + distance / 2; i++) {
                for (int j = _dryadEntranceY - distance / 2; j < _dryadEntranceY + distance / 2; j++) {
                    Tile tile = Main.tile[i, j];
                    if (tile.HasTile) {
                        if (tile.TileType == treeDryad) {
                            tile.TileColor = livingTreePaintColor;
                        }
                        else if (tile.WallType == 244 || tile.WallType == WallID.LivingLeaf) {
                            tile.TileColor = livingTreePaintColor;
                        }
                        else if (tile.TileType == 192 || tile.TileType == 191) {
                            tile.TileColor = livingTreePaintColor;
                        }
                        else if (tile.TileType == 52 || tile.TileType == 382) {
                            int x = i;
                            int y = j;
                            WorldGenHelper.GetVineTop(i, j, out x, out y);
                            if (Main.tile[x, y].TileType == 192)
                                tile.TileColor = livingTreePaintColor;
                        }
                        else if (tile.TileType == 187) {
                            Tile tile2 = tile;
                            int num = 0;
                            while (tile2.TileType == 187) {
                                num++;
                                tile2 = Main.tile[i, j + (int)num];
                            }

                            if (tile2.TileType == 192)
                                tile.TileColor = livingTreePaintColor;
                        }
                    }

                    if (tile.WallType == 244 || tile.WallType == WallID.LivingLeaf)
                        tile.WallColor = livingTreePaintColor;
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
            int fluff = 120/* * WorldGenHelper.WorldSize*/;
            int num1053 = WorldGen.genRand.Next((int)((double)Main.maxTilesX / 2 - fluff), (int)((double)Main.maxTilesX / 2 + fluff));
            while (!flag60) {
                flag60 = true;
                while ((num1053 > Main.maxTilesX / 2 - 90 && num1053 < Main.maxTilesX / 2 + 90) ||
                    Math.Abs(GenVars.UndergroundDesertLocation.Center.X - num1053) < GenVars.UndergroundDesertLocation.Width ||
                    (num1053 < GenVars.jungleMaxX + 50 && num1053 > GenVars.jungleMinX - 50)) {
                    num1053 = WorldGen.genRand.Next((int)((double)Main.maxTilesX / 2 - fluff), (int)((double)Main.maxTilesX / 2 + fluff));
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
            for (int x = -directions; x < directions + 2; x++) {
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
        float sizeValue = 0.2f;
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
                sizeValue += genRand.NextFloat(0.05f, 0.085f) * 0.75f;
                if (value > 0.5f && sizeValue < 0.5f) {
                    sizeValue += genRand.NextFloat(0.05f, 0.085f) * 0.25f;
                }
            }
            num2 *= sizeValue;
            bool flag = num4 < size / 3;
            bool flag2 = num4 < size - 4;
            bool flag3 = num4 > size - 8;
            if (flag) {
                Mountinater3((int)vector2D.X, (int)vector2D.Y, 4, [wallType, WallID.DirtUnsafe, WallID.GrassUnsafe, WallID.FlowerUnsafe, WallID.Cave6Unsafe]);
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
        Mountinater3((int)vector2D.X, (int)vector2D.Y, 4, [wallType, WallID.DirtUnsafe, WallID.GrassUnsafe, WallID.FlowerUnsafe, WallID.Cave6Unsafe]);
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
                if (Vector2.Distance(new Vector2(i, j), new Vector2((int)vector2D3.X, (int)vector2D3.Y)) > size * 1.6f) {
                    if (!Main.tile[i, j].HasTile && !Main.tile[i + 1, j].HasTile) {
                        bool flag6 = Main.tile[i - 1, j].TileType == tileType;
                        bool flag7 = Main.tile[i + 2, j].TileType == tileType;
                        bool flag5 = flag6 || flag7;
                        if (flag5) {
                            bool flag = true;
                            if (flag) {
                                extraX = result2.X < 0f ? genRand.Next(4) : -(8 + genRand.Next(4));
                                extraX -= 2;
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
                int y3 = y2 - 8;
                if (Main.tile[x2, y3].WallType != wallType) {
                    double num9 = Math.Abs((double)x2 - origin.X);
                    double num10 = Math.Abs((double)y3 - origin.Y);
                    if (Math.Sqrt(num9 * num9 + num10 * num10) < num2 * 1.2f && !Main.tile[x2, y3].HasTile) {
                        WorldGenHelper.ReplaceWall(x2, y3, WallID.DirtUnsafe);
                        WorldGenHelper.ReplaceTile(x2, y3, TileID.Dirt);
                    }
                }
            }
        }
        for (int x2 = num4; x2 < num5_; x2++) {
            for (int y2 = num6_; y2 < num7_; y2++) {
                int y3 = y2 - 16;
                if (Main.tile[x2, y3].WallType != wallType) {
                    double num9 = Math.Abs((double)x2 - origin.X);
                    double num10 = Math.Abs((double)y3 - origin.Y);
                    if (Math.Sqrt(num9 * num9 + num10 * num10) < num2 * 0.8) {
                        WorldGenHelper.ReplaceWall(x2, y3, wallType);
                        WorldGenHelper.ReplaceTile(x2, y3, tileType);
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
                bool flag5 = true;
                //for (int k = -1; k < 3; k++) {
                //    if (!Main.tile[i + k, j + 1].HasTile) {
                //        flag5 = false;
                //    }
                //}
                if (flag5) {
                    int altered = genRand.NextBool() ? 2 : 0;
                    WorldGen.Place2xX(i, j, treeDryad, altered);
                    if (Main.tile[i, j].TileType == treeDryad) {
                        flag4 = true;
                    }
                }
            }
        }
        ushort tileType2 = 192;
        ushort wallType2 = WallID.LivingLeaf;
        num_ = 6;
        num2_ = 4;
        for (int k = 0; k < 2; k++) {
            WorldUtils.Gen(origin, new Shapes.Slime(num_), Actions.Chain(
                new Modifiers.Blotches(num2_, num2_, num2_, 1, 1.0).Output(data),
                new Modifiers.OnlyTiles(tileType),
                new LeafModifier(tileType),
                new Actions.SetTile(tileType2, setSelfFrames: true), new LeafPlaceWall(wallType2)));
        }
        for (int x2 = num4; x2 < num5_; x2++) {
            for (int y2 = num6_; y2 < num7_; y2++) {
                if (y2 > origin.Y + distance * 0.25) {
                    continue;
                }
                int y3 = y2 - 16;
                double num9 = Math.Abs((double)x2 - origin.X);
                double num10 = Math.Abs((double)y3 - origin.Y);
                if (Math.Sqrt(num9 * num9 + num10 * num10) < num2 * 0.75) {
                    if (Main.tile[x2, y3].HasTile && Main.tile[x2, y3].TileType == tileType && Main.tile[x2, y3 - 1].HasTile && Main.tile[x2, y3 - 1].TileType == tileType2) {
                        WorldGenHelper.ReplaceTile(x2, y3, tileType2);
                    }
                }
            }
        }
        for (int x2 = num4; x2 < num5_; x2++) {
            for (int y2 = num6_; y2 < num7_; y2++) {
                int y3 = y2;
                if (WorldGenHelper.ActiveTile(x2, y3, tileType) &&
                    (Main.tile[x2, y3 - 1].TileType != tileType || !Main.tile[x2, y3 - 1].HasTile) &&
                    (Main.tile[x2, y3 + 1].TileType != tileType || !Main.tile[x2, y3 + 1].HasTile) &&
                    (Main.tile[x2 - 1, y3].TileType != tileType || !Main.tile[x2 - 1, y3].HasTile) &&
                    (Main.tile[x2 + 1, y3].TileType != tileType || !Main.tile[x2 + 1, y3].HasTile)) {
                    WorldGenHelper.ReplaceTile(x2, y3, tileType2);
                }
            }
        }
        foreach (Point killPos in killTiles) {
            i = killPos.X;
            j = killPos.Y;
            if (Vector2.Distance(new Vector2(i, j), new Vector2((int)vector2D3.X, (int)vector2D3.Y)) > size * 1.25f) {
                if (genRand.NextChance(0.75)) {
                    for (int i2 = -1; i2 < 2; i2++) {
                        for (int j2 = -1; j2 < 2; j2++) {
                            if (Math.Abs(i2) != Math.Abs(j2)) {
                                if (Main.tile[i2 + i, j2 + j].TileType == tileType) {
                                    Main.tile[i2 + i, j2 + j].TileType = tileType2;
                                }
                            }
                        }
                    }
                }
            }
        }
        for (int k = 0; k < 2; k++) {
            for (int x2 = num4; x2 < num5_; x2++) {
                for (int y2 = num6_; y2 < num7_; y2++) {
                    if (Main.tile[x2, y2].HasTile && Main.tile[x2, y2].TileType == tileType2 && WorldGen.GrowMoreVines(x2, y2) &&
                        !Main.tile[x2, y2 + 1].HasTile && genRand.NextBool(4)) {
                        bool flag5 = true;
                        ushort type7 = TileID.Vines;
                        for (int num35 = y2; num35 > y2 - 10; num35--) {
                            if (Main.tile[x2, num35].BottomSlope) {
                                flag5 = false;
                                break;
                            }

                            if (Main.tile[x2, num35].HasTile && Main.tile[x2, num35].TileType == tileType2 && !Main.tile[x2, num35].BottomSlope) {
                                flag5 = true;
                                break;
                            }
                        }
                        if (flag5) {
                            int height = genRand.NextBool() ? genRand.Next(2, 6) : genRand.NextBool() ? genRand.Next(3, 6) : genRand.Next(1, 6);
                            for (int num35 = y2; num35 < y2 + height; num35++) {
                                int num36 = num35 + 1;
                                Tile tile = Main.tile[x2, num36];
                                if (!tile.HasTile) {
                                    tile.TileType = type7;
                                    tile.HasTile = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        bool bigPlaced = false;
        for (int x2 = num4; x2 < num5_; x2++) {
            if (bigPlaced) {
                break;
            }
            for (int y2 = num6_; y2 < num7_; y2++) {
                if (bigPlaced) {
                    break;
                }
                int y3 = y2;
                if (WorldGen.SolidTile2(x2, y3) && !Main.tile[x2, y3 - 1].HasTile && (Main.tile[x2, y3].TileType == tileType || Main.tile[x2, y3].TileType == tileType2)) {
                    WorldGen.PlaceTile(x2, y3 - 1, 187, mute: true, forced: false, -1, genRand.Next(50, 53) - 3);
                    if (Main.tile[x2, y3 - 1].TileType == 187) {
                        bigPlaced = true;
                        if (_bigRubblePosition == Point.Zero) {
                            _bigRubblePosition = new Point(x2, y3 - 1);
                        }
                    }
                }
            }
        }
        bool mediumPlaced1 = false;
        int placedIndex = 2;
        for (int x2 = num4; x2 < num5_; x2++) {
            for (int y2 = num6_; y2 < num7_; y2++) {
                int y3 = y2;
                if (WorldGen.SolidTile2(x2, y3) && (Main.tile[x2, y3].TileType == tileType || Main.tile[x2, y3].TileType == tileType2) && WorldGen.SolidTile2(x2 + 1, y3) && genRand.NextBool(3) && (Main.tile[x2 + 1, y3].TileType == tileType || Main.tile[x2 + 1, y3].TileType == tileType2) && !Main.tile[x2, y3 - 1].HasTile && !Main.tile[x2 + 1, y3 - 1].HasTile) {
                    if (placedIndex % 2 == 0 && (!mediumPlaced1 || (genRand.NextBool(2) && mediumPlaced1))) {
                        ushort type = (ushort)ModContent.TileType<TreeDryadDecoration2>();
                        Tile tile = Main.tile[x2, y3 - 1];
                        short frameX = (short)((0 + 2 * genRand.Next(0, 2)) * 18);
                        tile.HasTile = true;
                        tile.TileFrameY = 0;
                        tile.TileFrameX = frameX;
                        tile.TileType = type;
                        tile = Main.tile[x2 + 1, y3 - 1];
                        tile.HasTile = true;
                        tile.TileFrameY = 0;
                        tile.TileFrameX = (short)(frameX + 18);
                        tile.TileType = type;
                        mediumPlaced1 = true;
                        placedIndex++;
                    }
                    else {
                        ushort type = 185;
                        Tile tile = Main.tile[x2, y3 - 1];
                        short frameX = (short)((12 + 2 * genRand.Next(0, 3)) * 18);
                        tile.HasTile = true;
                        tile.TileFrameY = 36;
                        tile.TileFrameX = frameX;
                        tile.TileType = type;
                        tile = Main.tile[x2 + 1, y3 - 1];
                        tile.HasTile = true;
                        tile.TileFrameY = 36;
                        tile.TileFrameX = (short)(frameX + 18);
                        tile.TileType = type;
                        placedIndex++;
                    }
                }
            }
        }
        bool smallPlaced1 = false;
        placedIndex = 2;
        for (int x2 = num4; x2 < num5_; x2++) {
            for (int y2 = num6_; y2 < num7_; y2++) {
                int y3 = y2;
                if (WorldGen.SolidTile2(x2, y3) && (Main.tile[x2, y3].TileType == tileType || Main.tile[x2, y3].TileType == tileType2) && genRand.NextBool(4) && !Main.tile[x2, y3 - 1].HasTile) {
                    if (placedIndex % 2 == 0 && (!smallPlaced1 || (genRand.NextBool(2) && smallPlaced1))) {
                        WorldGen.PlaceSmallPile(x2, y3 - 1, genRand.Next(2), 0, (ushort)ModContent.TileType<TreeDryadDecoration1>());
                        smallPlaced1 = true;
                        placedIndex++;
                    }
                    else {
                        WorldGen.PlaceSmallPile(x2, y3 - 1, 72, 0);
                        placedIndex++;
                    }
                }
            }
        }
        num4 = (int)(origin.X - distance * 1);
        num5_ = (int)(origin.X + distance * 1);
        num6_ = (int)(origin.Y - distance * 1);
        num7_ = (int)(origin.Y + distance * 1);
        for (int x2 = num4; x2 < num5_; x2++) {
            for (int y2 = num6_; y2 < num7_; y2++) {
                int y3 = y2 - 16;
                double num9 = Math.Abs((double)x2 - origin.X);
                double num10 = Math.Abs((double)y3 - origin.Y);
                if (Math.Sqrt(num9 * num9 + num10 * num10) < num2) {
                    if (!Main.tile[x2, y3].HasTile && Main.tile[x2, y3].WallType == wallType && genRand.NextChance(0.65)) {
                        bool flag5 = false;
                        for (int i2 = -1; i2 < 2; i2++) {
                            for (int j2 = -1; j2 < 2; j2++) {
                                if (Math.Abs(i2) != Math.Abs(j2)) {
                                    if (Main.tile[i2 + x2, j2 + y2].TileType == tileType || Main.tile[i2 + x2, j2 + y2].TileType == tileType2) {
                                        flag5 = true;
                                    }
                                }
                            }
                        }
                        if (!flag5) {
                            int num877 = 1;
                            if (genRand.Next(2) == 0)
                                num877 = -1;
                            WorldGenHelper.TileWallRunner(x2 - num877, y3, genRand.Next(1, 4), genRand.Next(1, 3), TileID.Cobweb, wallType, addTile: true, num877, -1.0, noYChange: false, overRide: false);
                        }
                    }
                }
            }
        }
    }

    public class LeafPlaceWall : GenAction {
        private ushort _type;
        private bool _neighbors;

        public LeafPlaceWall(ushort type, bool neighbors = true) {
            _type = type;
            _neighbors = neighbors;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args) {
            if (y < origin.Y && _random.NextChance(0.8f)) {
                float strengh = _random.NextFloat(4f, 6f);
                WorldGenHelper.TileWallRunner(x, y, strengh, (int)(strengh / 3), 0, _type, onlyWall: true, overRide: true, addTile: true);
                if (_random.NextBool(3)) {
                    Vector2 direction = new Vector2(0f, 1f).RotatedByRandom(MathHelper.PiOver4);
                    for (int i = 1; i < 4; i++) {
                        strengh = 3f;
                        Vector2 velocity = direction * i * 2f;
                        WorldGenHelper.TileWallRunner(x + (int)velocity.X, y - 2 + (int)velocity.Y, strengh, (int)(strengh / 3), 0, _type, onlyWall: true, overRide: true, addTile: true);
                    }
                }
            }

            return UnitApply(origin, x, y, args);
        }
    }


    public class LeafModifier : GenAction {
        private ushort[] _types;

        public LeafModifier(params ushort[] types) {
            _types = types;
        }

        public override bool Apply(Point origin, int x, int y, params object[] args) {
            for (int i = 0; i < _types.Length; i++) {
                if (GenBase._tiles[x, y - 1].TileType != _types[i] || !GenBase._tiles[x, y - 1].HasTile) {
                    return Fail();
                }
                if (GenBase._tiles[x, y - 1].TileType == _types[i] && _random.NextChance(0.3)) {
                    return Fail();
                }
            }

            return UnitApply(origin, x, y, args);
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
