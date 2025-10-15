using RoA.Common.BackwoodsSystems;
using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.World.Generations;

sealed class BackwoodsWorldGen : ModSystem {
    private const float LAYERWEIGHT = 5000f;

    internal static bool _extraModSupport;

    public static BackwoodsBiomePass? BackwoodsWorldGenPass { get; private set; }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        bool hasSpirit = ModLoader.HasMod("SpiritMod");
        bool hasSpiritReforged = ModLoader.HasMod("SpiritReforged");
        bool hasRemnants = ModLoader.HasMod("Remnants");

        if (hasRemnants) {
            _extraModSupport = true;
        }

        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Corruption"));
        if (genIndex == -1) {
            _extraModSupport = true;
        }
        else {
            //genIndex += 6;
        }
        if (_extraModSupport) {
            genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Pyramids"));
            genIndex -= 1;
        }
        if (hasSpiritReforged) {
            genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Pyramids"));
            genIndex += 2;
        }
        tasks.Insert(genIndex, BackwoodsWorldGenPass = new("Backwoods", LAYERWEIGHT));

        if (hasRemnants) {
            genIndex += 22;
            tasks.Insert(genIndex, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsLootRooms, 1500f));
            genIndex++;
            tasks.Insert(genIndex, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsCleanup, 600f));

            genIndex++;
            tasks.Insert(genIndex, new PassLegacy(string.Empty, BackwoodsWorldGenPass.ReplaceAllSnowBlockForSpiritModSupport, 10f));

            tasks.Insert(tasks.Count - 4, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsOtherPlacements, 3000f));
            tasks.Insert(tasks.Count - 3, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsTilesReplacement));
            tasks.Insert(tasks.Count - 2, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsOnLast0));
            tasks.Insert(tasks.Count - 1, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsOnLast));

            tasks.Add(new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsOnLast1));

            return;
        }

        genIndex += 3;
        tasks.Insert(genIndex, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsLootRooms, 1500f));

        //if (!hasSpirit) {
        //    genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Smooth World"));
        //    genIndex -= 2;
        //}
        //else {
        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Jungle Chests"));
        genIndex += 2;
        //}
        tasks.Insert(genIndex, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsCleanup, 600f));

        //if (hasSpirit) {
        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Jungle Chests"));
        genIndex += 2;

        tasks.Insert(genIndex, new PassLegacy(string.Empty, BackwoodsWorldGenPass.ReplaceAllSnowBlockForSpiritModSupport, 10f));
        //}

        //genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Spreading Grass"));
        //tasks.RemoveAt(genIndex);

        //tasks.Insert(genIndex, new PassLegacy("Spreading Grass", SpreadingGrass, 80.3414f));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
        genIndex -= 3;
        tasks.Insert(genIndex, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsOtherPlacements, 3000f));

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Micro Biomes"));
        genIndex += 1;
        tasks.Insert(genIndex, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsTilesReplacement));

        tasks.Insert(tasks.Count - 4, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsOnLast0));
        tasks.Insert(tasks.Count - 2, new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsOnLast));

        tasks.Add(new PassLegacy(string.Empty, BackwoodsWorldGenPass.BackwoodsOnLast1));
    }

    public override void PostWorldGen() {
        _extraModSupport = false;
    }

    public override void Load() {
        On_WorldGen.Convert_int_int_int_int += On_WorldGen_Convert_int_int_int_int;
        On_WorldGen.PaintTheLivingTrees += On_WorldGen_PaintTheLivingTrees;
        On_WorldGen.NotTheBees += On_WorldGen_NotTheBees;
    }

    private void On_WorldGen_NotTheBees(On_WorldGen.orig_NotTheBees orig) {
        int num = Main.maxTilesX / 7;
        if (!WorldGen.notTheBees)
            return;

        var genRand = WorldGen.genRand;
        for (int i = 0; i < Main.maxTilesX; i++) {
            for (int j = 0; j < Main.maxTilesY - 180; j++) {
                if (i > BackwoodsVars.BackwoodsStartX && i < BackwoodsVars.BackwoodsEndX) {
                    continue;
                }
                if (j > WorldGenHelper.SafeFloatingIslandY && j < BackwoodsVars.BackwoodsEndY) {
                    continue;
                }

                if (WorldGen.remixWorldGen && (i < num + genRand.Next(3) || i >= Main.maxTilesX - num - genRand.Next(3) || ((double)j > (Main.worldSurface * 2.0 + Main.rockLayer) / 3.0 + (double)genRand.Next(3) && j < Main.maxTilesY - 350 - genRand.Next(3))))
                    continue;

                if (Main.tile[i, j].TileType == 52)
                    Main.tile[i, j].TileType = 62;

                if ((WorldGen.SolidOrSlopedTile(i, j) || TileID.Sets.CrackedBricks[Main.tile[i, j].TileType]) && !TileID.Sets.Ore[Main.tile[i, j].TileType] && Main.tile[i, j].TileType != 123 && Main.tile[i, j].TileType != 40) {
                    if (Main.tile[i, j].TileType == 191 || Main.tile[i, j].TileType == 383) {
                        if (!WorldGen.remixWorldGen)
                            Main.tile[i, j].TileType = 383;
                    }
                    else if (Main.tile[i, j].TileType == 192 || Main.tile[i, j].TileType == 384) {
                        if (!WorldGen.remixWorldGen)
                            Main.tile[i, j].TileType = 384;
                    }
                    else if (Main.tile[i, j].TileType != 151 && Main.tile[i, j].TileType != 662 && Main.tile[i, j].TileType != 661 && Main.tile[i, j].TileType != 189 && Main.tile[i, j].TileType != 196 && Main.tile[i, j].TileType != 120 && Main.tile[i, j].TileType != 158 && Main.tile[i, j].TileType != 175 && Main.tile[i, j].TileType != 45 && Main.tile[i, j].TileType != 119) {
                        if (Main.tile[i, j].TileType >= 63 && Main.tile[i, j].TileType <= 68) {
                            Main.tile[i, j].TileType = 230;
                        }
                        else if (Main.tile[i, j].TileType != 57 && Main.tile[i, j].TileType != 76 && Main.tile[i, j].TileType != 75 && Main.tile[i, j].TileType != 229 && Main.tile[i, j].TileType != 230 && Main.tile[i, j].TileType != 407 && Main.tile[i, j].TileType != 404) {
                            if (Main.tile[i, j].TileType == 224) {
                                Main.tile[i, j].TileType = 229;
                            }
                            else if (Main.tile[i, j].TileType == 53) {
                                if (i < WorldGen.beachDistance + genRand.Next(3) || i > Main.maxTilesX - WorldGen.beachDistance - genRand.Next(3))
                                    Main.tile[i, j].TileType = 229;
                            }
                            else if ((i <= WorldGen.beachDistance - genRand.Next(3) || i >= Main.maxTilesX - WorldGen.beachDistance + genRand.Next(3) || (Main.tile[i, j].TileType != 397 && Main.tile[i, j].TileType != 396)) && Main.tile[i, j].TileType != 10 && Main.tile[i, j].TileType != 203 && Main.tile[i, j].TileType != 25 && Main.tile[i, j].TileType != 137 && Main.tile[i, j].TileType != 138 && Main.tile[i, j].TileType != 141) {
                                if (Main.tileDungeon[Main.tile[i, j].TileType] || TileID.Sets.CrackedBricks[Main.tile[i, j].TileType]) {
                                    Tile tile = Main.tile[i, j];
                                    tile.TileColor = 14;
                                }
                                else if (Main.tile[i, j].TileType == 226) {
                                    Tile tile = Main.tile[i, j];
                                    tile.TileColor = 15;
                                }
                                else if (Main.tile[i, j].TileType != 202 && Main.tile[i, j].TileType != 70 && Main.tile[i, j].TileType != 48 && Main.tile[i, j].TileType != 232) {
                                    if (TileID.Sets.Conversion.Grass[Main.tile[i, j].TileType] || Main.tile[i, j].TileType == 60 || Main.tile[i, j].TileType == 70) {
                                        if (j > GenVars.lavaLine + genRand.Next(-2, 3) + 2)
                                            Main.tile[i, j].TileType = 70;
                                        else
                                            Main.tile[i, j].TileType = 60;
                                    }
                                    else if (Main.tile[i, j].TileType == 0 || Main.tile[i, j].TileType == 59) {
                                        Main.tile[i, j].TileType = 59;
                                    }
                                    else if (Main.tile[i, j].TileType != 633) {
                                        if (j > GenVars.lavaLine + genRand.Next(-2, 3) + 2)
                                            Main.tile[i, j].TileType = 230;
                                        else if (!WorldGen.remixWorldGen || (double)j > Main.worldSurface + (double)genRand.Next(-1, 2))
                                            Main.tile[i, j].TileType = 225;
                                    }
                                }
                            }
                        }
                    }
                }

                if (Main.tile[i, j].WallType != 15 && Main.tile[i, j].WallType != 64 && Main.tile[i, j].WallType != 204 && Main.tile[i, j].WallType != 205 && Main.tile[i, j].WallType != 206 && Main.tile[i, j].WallType != 207 && Main.tile[i, j].WallType != 23 && Main.tile[i, j].WallType != 24 && Main.tile[i, j].WallType != 42 && Main.tile[i, j].WallType != 10 && Main.tile[i, j].WallType != 21 && Main.tile[i, j].WallType != 82 && Main.tile[i, j].WallType != 187 && Main.tile[i, j].WallType != 216 && Main.tile[i, j].WallType != 34 && Main.tile[i, j].WallType != 244) {
                    if (Main.tile[i, j].WallType == 87) {
                        Tile tile = Main.tile[i, j];
                        tile.WallColor = 15;
                    }
                    else if (Main.wallDungeon[Main.tile[i, j].WallType]) {
                        Tile tile = Main.tile[i, j];
                        tile.WallColor = 14;
                    }
                    else if (Main.tile[i, j].WallType == 2)
                        Main.tile[i, j].WallType = 2;
                    else if (Main.tile[i, j].WallType == 196)
                        Main.tile[i, j].WallType = 196;
                    else if (Main.tile[i, j].WallType == 197)
                        Main.tile[i, j].WallType = 197;
                    else if (Main.tile[i, j].WallType == 198)
                        Main.tile[i, j].WallType = 198;
                    else if (Main.tile[i, j].WallType == 199)
                        Main.tile[i, j].WallType = 199;
                    else if (Main.tile[i, j].WallType == 63)
                        Main.tile[i, j].WallType = 64;
                    else if (Main.tile[i, j].WallType != 3 && Main.tile[i, j].WallType != 83 && Main.tile[i, j].WallType != 73 && Main.tile[i, j].WallType != 62 && Main.tile[i, j].WallType != 13 && Main.tile[i, j].WallType != 14 && Main.tile[i, j].WallType > 0 && (!WorldGen.remixWorldGen || (double)j > Main.worldSurface + (double)genRand.Next(-1, 2)))
                        Main.tile[i, j].WallType = 86;
                }

                if (Main.tile[i, j].LiquidAmount > 0 && j <= GenVars.lavaLine + 2) {
                    if ((double)j > Main.rockLayer && (i < WorldGen.beachDistance + 200 || i > Main.maxTilesX - WorldGen.beachDistance - 200)) {
                        Tile tile = Main.tile[i, j];
                        tile.LiquidType = LiquidID.Lava;
                    }
                    else if (Main.wallDungeon[Main.tile[i, j].WallType]) {
                        Tile tile = Main.tile[i, j];
                        tile.LiquidType = LiquidID.Lava;
                    }
                    else {
                        Tile tile = Main.tile[i, j];
                        tile.LiquidType = LiquidID.Honey;
                    }
                }
            }
        }
    }

    private void On_WorldGen_PaintTheLivingTrees(On_WorldGen.orig_PaintTheLivingTrees orig, byte livingTreePaintColor, byte livingTreeWallPaintColor) {
        orig(livingTreePaintColor, livingTreeWallPaintColor);

        livingTreePaintColor = PaintID.YellowPaint;
        livingTreeWallPaintColor = PaintID.YellowPaint;

        int elderwoodTileType = ModContent.TileType<LivingElderwood>();
        int elderwoodWallType = ModContent.WallType<ElderwoodWall3>();
        int vineTileType = ModContent.TileType<BackwoodsVines>();
        int vineTileType2 = ModContent.TileType<BackwoodsVinesFlower>();
        int elderwoodLeavesTileType = ModContent.TileType<LivingElderwoodlLeaves>();
        for (int i = 0; i < Main.maxTilesX; i++) {
            for (int j = 0; j < Main.maxTilesY; j++) {
                Tile tile = Main.tile[i, j];
                if (tile.HasTile) {
                    if (tile.TileType == ModContent.TileType<OvergrownAltar>()) {
                        tile.TileColor = PaintID.RedPaint;
                    }
                    else if (tile.TileType == ModContent.TileType<NexusGateway>()) {
                        tile.TileColor = PaintID.LimePaint;
                    }
                    else if (tile.WallType == elderwoodWallType) {
                        tile.TileColor = livingTreePaintColor;
                    }
                    else if (tile.TileType == elderwoodLeavesTileType || tile.TileType == elderwoodTileType) {
                        tile.TileColor = livingTreePaintColor;
                    }
                    else if (tile.TileType == vineTileType || tile.TileType == vineTileType2) {
                        int x = i;
                        int y = j;
                        WorldGenHelper.GetVineTop(i, j, out x, out y);
                        if (Main.tile[x, y].TileType == elderwoodLeavesTileType)
                            tile.TileColor = livingTreePaintColor;
                    }
                }

                if (tile.WallType == elderwoodWallType)
                    tile.WallColor = livingTreePaintColor;
            }
        }
    }


    private void On_WorldGen_Convert_int_int_int_int(On_WorldGen.orig_Convert_int_int_int_int orig, int i, int j, int conversionType, int size) {
        if (WorldGen.gen) {
            for (int k = i - size; k <= i + size; k++) {
                for (int l = j - size; l <= j + size; l++) {
                    if (!WorldGen.InWorld(k, l, 1) || Math.Abs(k - i) + Math.Abs(l - j) >= 6)
                        continue;

                    Tile tile = Main.tile[k, l];
                    int type = tile.TileType;
                    int wall = tile.TileType;
                    if (i > BackwoodsVars.BackwoodsCenterX - BackwoodsVars.BackwoodsHalfSizeX - 50 && i < BackwoodsVars.BackwoodsCenterX + BackwoodsVars.BackwoodsHalfSizeX + 50) {
                        return;
                    }
                    if (tile.TileType != TileID.Dirt && BackwoodsVars.BackwoodsTileTypes.Contains(tile.TileType)) {
                        return;
                    }
                    if (BackwoodsVars.BackwoodsWallTypes.Contains(tile.WallType)) {
                        return;
                    }
                }
            }
        }

        orig(i, j, conversionType, size);
    }

    //private void SpreadingGrass(GenerationProgress progress, GameConfiguration config) {
    //    if (!WorldGen.notTheBees || WorldGen.remixWorldGen) {
    //        progress.Message = Lang.gen[37].Value;
    //        for (int num414 = 50; num414 < Main.maxTilesX - 50; num414++) {
    //            for (int num415 = 50; (double)num415 <= Main.worldSurface; num415++) {
    //                if (Main.tile[num414, num415].HasTile) {
    //                    int type5 = Main.tile[num414, num415].TileType;
    //                    if (Main.tile[num414, num415].HasTile && type5 == 60) {
    //                        for (int num416 = num414 - 1; num416 <= num414 + 1; num416++) {
    //                            for (int num417 = num415 - 1; num417 <= num415 + 1; num417++) {
    //                                if (Main.tile[num416, num417].HasTile && Main.tile[num416, num417].TileType == 0) {
    //                                    if (!Main.tile[num416, num417 - 1].HasTile)
    //                                        Main.tile[num416, num417].TileType = 60;
    //                                    else
    //                                        Main.tile[num416, num417].TileType = 59;
    //                                }
    //                            }
    //                        }
    //                    }
    //                    else if (type5 == 1 || type5 == 40 || TileID.Sets.Ore[type5]) {
    //                        int num418 = 3;
    //                        bool flag22 = false;
    //                        ushort num419 = 0;
    //                        for (int num420 = num414 - num418; num420 <= num414 + num418; num420++) {
    //                            for (int num421 = num415 - num418; num421 <= num415 + num418; num421++) {
    //                                if (Main.tile[num420, num421].HasTile) {
    //                                    if (Main.tile[num420, num421].TileType == 53 || num419 == 53)
    //                                        num419 = 53;
    //                                    else if (Main.tile[num420, num421].TileType == 59 || Main.tile[num420, num421].TileType == 60 || Main.tile[num420, num421].TileType == 147 || Main.tile[num420, num421].TileType == 161 || Main.tile[num420, num421].TileType == 199 || Main.tile[num420, num421].TileType == 23)
    //                                        num419 = Main.tile[num420, num421].TileType;
    //                                }
    //                                else if (num421 < num415 && Main.tile[num420, num421].WallType == 0) {
    //                                    flag22 = true;
    //                                }
    //                            }
    //                        }

    //                        if (flag22) {
    //                            switch (num419) {
    //                                case 23:
    //                                case 199:
    //                                    if (Main.tile[num414, num415 - 1].HasTile)
    //                                        num419 = 0;
    //                                    break;
    //                                case 59:
    //                                case 60:
    //                                    if (num414 >= GenVars.jungleMinX && num414 <= GenVars.jungleMaxX)
    //                                        num419 = (ushort)(Main.tile[num414, num415 - 1].HasTile ? 59 : 60);
    //                                    break;
    //                            }

    //                            Main.tile[num414, num415].TileType = num419;
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        for (int num422 = 10; num422 < Main.maxTilesX - 10; num422++) {
    //            bool flag23 = true;
    //            for (int num423 = 0; (double)num423 < Main.worldSurface - 1.0; num423++) {
    //                if (Main.tile[num422, num423].HasTile) {
    //                    if (flag23 && Main.tile[num422, num423].TileType == 0) {
    //                        try {
    //                            WorldGen.grassSpread = 0;
    //                            WorldGen.SpreadGrass(num422, num423);
    //                        }
    //                        catch {
    //                            WorldGen.grassSpread = 0;
    //                            WorldGen.SpreadGrass(num422, num423, 0, 2, repeat: false);
    //                        }
    //                    }

    //                    if ((double)num423 > GenVars.worldSurfaceHigh)
    //                        break;

    //                    flag23 = false;
    //                }
    //                else if (Main.tile[num422, num423].WallType == 0) {
    //                    flag23 = true;
    //                }
    //            }
    //        }

    //        if (WorldGen.remixWorldGen) {
    //            for (int num424 = 5; num424 < Main.maxTilesX - 5; num424++) {
    //                for (int num425 = (int)GenVars.rockLayerLow + WorldGen.genRand.Next(-1, 2); num425 < Main.maxTilesY - 200; num425++) {
    //                    if (Main.tile[num424, num425].TileType == 0 && Main.tile[num424, num425].HasTile && (!Main.tile[num424 - 1, num425 - 1].HasTile || !Main.tile[num424, num425 - 1].HasTile || !Main.tile[num424 + 1, num425 - 1].HasTile || !Main.tile[num424 - 1, num425].HasTile || !Main.tile[num424 + 1, num425].HasTile || !Main.tile[num424 - 1, num425 + 1].HasTile || !Main.tile[num424, num425 + 1].HasTile || !Main.tile[num424 + 1, num425 + 1].HasTile))
    //                        Main.tile[num424, num425].TileType = 2;
    //                }
    //            }

    //            for (int num426 = 5; num426 < Main.maxTilesX - 5; num426++) {
    //                for (int num427 = (int)GenVars.rockLayerLow + WorldGen.genRand.Next(-1, 2); num427 < Main.maxTilesY - 200; num427++) {
    //                    if (Main.tile[num426, num427].TileType == 2 && !Main.tile[num426, num427 - 1].HasTile && WorldGen.genRand.Next(20) == 0)
    //                        WorldGen.PlaceTile(num426, num427 - 1, 27, mute: true);
    //                }
    //            }

    //            int conversionType = 1;
    //            if (WorldGen.crimson)
    //                conversionType = 4;

    //            var genRand = WorldGen.genRand;
    //            int num428 = Main.maxTilesX / 7;
    //            for (int num429 = 10; num429 < Main.maxTilesX - 10; num429++) {
    //                for (int num430 = 10; num430 < Main.maxTilesY - 10; num430++) {
    //                    if ((double)num430 < Main.worldSurface + (double)genRand.Next(3) || num429 < num428 + genRand.Next(3) || num429 >= Main.maxTilesX - num428 - genRand.Next(3)) {
    //                        if (WorldGen.drunkWorldGen) {
    //                            if (GenVars.crimsonLeft) {
    //                                if (num429 < Main.maxTilesX / 2 + genRand.Next(-2, 3))
    //                                    WorldGen.Convert(num429, num430, 4, 1);
    //                                else
    //                                    WorldGen.Convert(num429, num430, 1, 1);
    //                            }
    //                            else if (num429 < Main.maxTilesX / 2 + genRand.Next(-2, 3)) {
    //                                WorldGen.Convert(num429, num430, 1, 1);
    //                            }
    //                            else {
    //                                WorldGen.Convert(num429, num430, 4, 1);
    //                            }
    //                        }
    //                        else {
    //                            WorldGen.Convert(num429, num430, conversionType, 1);
    //                        }

    //                        Tile tile = Main.tile[num429, num430];
    //                        tile.TileColor = 0;
    //                        tile.WallColor = 0;
    //                    }
    //                }
    //            }

    //            if (WorldGen.remixWorldGen) {
    //                Main.tileSolid[225] = true;
    //                int num431 = (int)((double)Main.maxTilesX * 0.31);
    //                int num432 = (int)((double)Main.maxTilesX * 0.69);
    //                _ = Main.maxTilesY;
    //                int num433 = Main.maxTilesY - 135;
    //                _ = Main.maxTilesY;
    //                Liquid.QuickWater(-2);
    //                for (int num434 = num431; num434 < num432 + 15; num434++) {
    //                    for (int num435 = Main.maxTilesY - 200; num435 < num433; num435++) {
    //                        Main.tile[num434, num435].LiquidAmount = 0;
    //                    }
    //                }

    //                Main.tileSolid[225] = false;
    //                Main.tileSolid[484] = false;
    //            }
    //        }
    //    }
}