using Microsoft.Xna.Framework;

using ReLogic.Utilities;

using RoA.Content.Tiles.Decorations;
using RoA.Content.Tiles.Miscellaneous;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class DryadEntrance : ModSystem, IPostSetupContent {
    void IPostSetupContent.PostSetupContent() {
        TileHelper.MergeWith(TileID.Mud, TileID.LivingMahoganyLeaves);
        if (ModLoader.HasMod("SpiritReforged")) {
            TileHelper.MergeWith(GetSavannaLeafTileType(), GetSavannaDirtTileType());
        }
    }

    public override void Load() {
        On_WorldGen.CaveOpenater += On_WorldGen_CaveOpenater;
        On_WorldGen.Cavinator += On_WorldGen_Cavinator;
    }

    private void On_WorldGen_Cavinator(On_WorldGen.orig_Cavinator orig, int i, int j, int steps) {
        if (ModLoader.HasMod("SpiritReforged") && i == GenVars.mCaveX[_dryadEntrancemCave] && j == GenVars.mCaveY[_dryadEntrancemCave]) {
            return;
        }

        orig(i, j, steps);
    }

    private void On_WorldGen_CaveOpenater(On_WorldGen.orig_CaveOpenater orig, int i, int j) {
        if (ModLoader.HasMod("SpiritReforged") && i == GenVars.mCaveX[_dryadEntrancemCave] && j == GenVars.mCaveY[_dryadEntrancemCave]) {
            return;
        }

        orig(i, j);
    }

    private static int _dryadEntranceX, _dryadEntranceY, _dryadEntrancemCave;
    private static Point _bigRubblePosition = Point.Zero;
    internal static bool _dryadStructureGenerated;

    public static bool HasSpiritModAndSavannahSeed => ModLoader.HasMod("SpiritReforged") && (WorldGen.currentWorldSeed.Equals("savanna", StringComparison.CurrentCultureIgnoreCase) || WorldGen.currentWorldSeed.Equals("savannah", StringComparison.CurrentCultureIgnoreCase));

    public override void ClearWorld() {
        _dryadStructureGenerated = false;
    }

    public override void SaveWorldData(TagCompound tag) {
        if (_dryadStructureGenerated) {
            tag[RoA.ModName + "_dryadStructureGenerated"] = true;
        }
    }

    public override void LoadWorldData(TagCompound tag) {
        _dryadStructureGenerated = tag.ContainsKey(RoA.ModName + "_dryadStructureGenerated");
    }

    public override void NetSend(BinaryWriter writer) {
        var flags = new BitsByte();
        flags[0] = _dryadStructureGenerated;
        writer.Write(flags);
    }

    public override void NetReceive(BinaryReader reader) {
        BitsByte flags = reader.ReadByte();
        _dryadStructureGenerated = flags[0];
    }

    private static ushort PlaceholderTileType => (ushort)ModContent.TileType<LivingElderwood>();
    private static ushort PlaceholderWallType => (ushort)ModContent.WallType<ElderwoodWall3>();

    public override void PostWorldGen() {
        _bigRubblePosition = Point.Zero;
    }

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        bool hasRemnants = ModLoader.HasMod("Remnants");

        int indexOffset = HasSpiritModAndSavannahSeed ? 36 : 0;

        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Mount Caves"));
        tasks.RemoveAt(genIndex);

        string pass = hasRemnants ? "Mount Caves, Dryad Entrance" : "Mount Caves";
        tasks.Insert(genIndex, new PassLegacy(pass, ExtraMountCavesGenerator, 49.9993f));
        if (HasSpiritModAndSavannahSeed) {
            tasks.Insert(genIndex + indexOffset, new PassLegacy(pass, ExtraMountCavesGenerator2, 49.9993f));
        }

        genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Mountain Caves"));
        if (!ModLoader.HasMod("SpiritReforged")) {
            tasks.RemoveAt(genIndex);
        }

        int genIndex2 = tasks.FindIndex(genpass => genpass.Name.Equals("Grass Wall"));
        tasks.RemoveAt(genIndex2);
        tasks.Insert(genIndex2, new PassLegacy("Grass Wall", GrassWallUpdated, 512.8323f));

        pass = hasRemnants ? "Mountain Caves, Dryad Entrance" : "Mountain Caves";
        tasks.Insert(genIndex, new PassLegacy(pass, DryadEntranceGenerator, 14.2958f));
        if (HasSpiritModAndSavannahSeed) {
            tasks.Insert(genIndex + indexOffset / 2, new PassLegacy(pass, DryadEntranceGenerator2, 14.2958f));
        }

        tasks.Insert(tasks.Count - 5, new PassLegacy(string.Empty, DryadEntranceCleanUp));
        tasks.Insert(tasks.Count - 5, new PassLegacy(string.Empty, DryadEntranceLoomPlacement));
    }

    private void GrassWallUpdated(GenerationProgress progress, GameConfiguration configuration) {
        WorldGen.maxTileCount = 3500;
        progress.Set(1.0);
        for (int num298 = 50; num298 < Main.maxTilesX - 50; num298++) {
            for (int num299 = 0; (double)num299 < Main.worldSurface - 10.0; num299++) {
                if (WorldGen.genRand.Next(4) == 0) {
                    bool flag8 = false;
                    int num300 = -1;
                    int num301 = -1;
                    if (Main.tile[num298, num299].HasTile && Main.tile[num298, num299].TileType == 2 && (Main.tile[num298, num299].WallType == 2 || Main.tile[num298, num299].WallType == 63)) {
                        for (int num302 = num298 - 1; num302 <= num298 + 1; num302++) {
                            for (int num303 = num299 - 1; num303 <= num299 + 1; num303++) {
                                if (Main.tile[num302, num303].WallType == 0 && !WorldGen.SolidTile(num302, num303))
                                    flag8 = true;
                            }
                        }

                        if (flag8) {
                            for (int num304 = num298 - 1; num304 <= num298 + 1; num304++) {
                                for (int num305 = num299 - 1; num305 <= num299 + 1; num305++) {
                                    if ((Main.tile[num304, num305].WallType == 2 || Main.tile[num304, num305].WallType == 15) && !WorldGen.SolidTile(num304, num305)) {
                                        num300 = num304;
                                        num301 = num305;
                                    }
                                }
                            }
                        }
                    }

                    if (flag8 && num300 > -1 && num301 > -1 && WorldGen.countDirtTiles(num300, num301) < WorldGen.maxTileCount) {
                        bool flag48 = false;
                        if (Math.Abs(num300 - GenVars.mCaveX[_dryadEntrancemCave]) < 100) {
                            flag48 = true;
                        }
                        if (!flag48) {
                            try {
                                ushort wallType = 63;
                                if (WorldGen.dontStarveWorldGen && WorldGen.genRand.Next(3) != 0)
                                    wallType = 62;

                                WorldGen.Spread.Wall2(num300, num301, wallType);
                            }
                            catch {
                            }
                        }
                    }
                }
            }
        }

        for (int num306 = 5; num306 < Main.maxTilesX - 5; num306++) {
            for (int num307 = 10; (double)num307 < Main.worldSurface - 1.0; num307++) {
                if (Main.tile[num306, num307].WallType == 63 && WorldGen.genRand.Next(10) == 0)
                    Main.tile[num306, num307].WallType = 65;

                if (Main.tile[num306, num307].HasTile && Main.tile[num306, num307].TileType == 0) {
                    bool flag9 = false;
                    for (int num308 = num306 - 1; num308 <= num306 + 1; num308++) {
                        for (int num309 = num307 - 1; num309 <= num307 + 1; num309++) {
                            if (Main.tile[num308, num309].WallType == 63 || Main.tile[num308, num309].WallType == 65) {
                                flag9 = true;
                                break;
                            }
                        }
                    }

                    bool flag48 = false;
                    if (Math.Abs(num306 - GenVars.mCaveX[_dryadEntrancemCave]) < 100) {
                        flag48 = true;
                    }
                    if (!flag48 && flag9)
                        WorldGen.SpreadGrass(num306, num307);
                }
            }
        }
    }

    private void DryadEntranceLoomPlacement(GenerationProgress progress, GameConfiguration configuration) {
        bool flag = false;
        for (int i = 0; i < Main.maxTilesX; i++) {
            for (int j = 0; j < Main.maxTilesY; j++) {
                if (WorldGenHelper.ActiveTile(i, j, 304)) {
                    flag = true;
                }
            }
        }
        //if (!flag) {
        //    _loomPlacedInWorld = false;
        //}

        if (WorldGen.notTheBees) {
            flag = false;
        }

        if (flag/* || _bigRubblePosition == Point.Zero*/) {
            return;
        }

        for (int x = 0; x < 2; x++) {
            for (int y = -2; y <= 0; y++) {
                WorldGen.KillTile(_bigRubblePosition.X + x, _bigRubblePosition.Y + y);
            }
        }

        for (int x = 0; x < 3; x++) {
            for (int y = -2; y <= 0; y++) {
                WorldGen.PlaceTile(_bigRubblePosition.X + x, _bigRubblePosition.Y + y, 304, mute: true);
            }
        }
    }

    private void DryadEntranceCleanUp(GenerationProgress progress, GameConfiguration configuration) {
        int distance = 100;
        ushort woodTileType = WorldGen.notTheBees ? TileID.LivingMahogany : HasSpiritModAndSavannahSeed ? GetSavannaWoodTileType() : TileID.LivingWood;
        ushort woodWallType = WorldGen.notTheBees ? WallID.LivingWood : HasSpiritModAndSavannahSeed ? GetSavannaWoodWallType() : WallID.LivingWoodUnsafe;
        ushort leafBlockTileType = WorldGen.notTheBees ? TileID.LivingMahoganyLeaves : HasSpiritModAndSavannahSeed ? GetSavannaLeafTileType() : TileID.LeafBlock;
        ushort dirtTileType = WorldGen.notTheBees ? TileID.Mud : HasSpiritModAndSavannahSeed ? GetSavannaDirtTileType() : TileID.Dirt;
        for (int x2 = _dryadEntranceX - distance / 2; x2 < _dryadEntranceX + distance / 2; x2++) {
            for (int y2 = _dryadEntranceY - distance / 2; y2 < _dryadEntranceY + distance / 2; y2++) {
                if (Main.tile[x2, y2].TileType == PlaceholderTileType) {
                    Main.tile[x2, y2].TileType = woodTileType;
                }
                if (Main.tile[x2, y2].WallType == PlaceholderWallType) {
                    Main.tile[x2, y2].WallType = woodWallType;
                }
                if (Main.tile[x2, y2].TileType == leafBlockTileType) {
                    for (int grassX = x2 - 2; grassX < x2 + 3; grassX++) {
                        for (int grassY = y2 - 2; grassY < y2 + 3; grassY++) {
                            if (Main.tile[grassX, grassY].TileType == TileID.Grass || (WorldGen.notTheBees && Main.tile[grassX, grassY].TileType == TileID.JungleGrass) || (HasSpiritModAndSavannahSeed && Main.tile[grassX, grassY].TileType == GetSavannaGrassTileType())) {
                                Main.tile[grassX, grassY].TileType = dirtTileType;
                                WorldGen.SquareTileFrame(grassX, grassY);
                            }
                        }
                    }
                }
                //if (Main.tile[x2, y2].WallType == ModContent.WallType<LivingBackwoodsLeavesWall2>()) {
                //    Main.tile[x2, y2].WallType = WallID.LivingWoodUnsafe;
                //}
            }
        }

        {
            Point16 vector2D = new(_dryadEntranceX, _dryadEntranceY);
            double num = 50;
            int vinenum5 = (int)(vector2D.X - num * 1);
            int vinenum6 = (int)(vector2D.X + num * 1);
            int vinenum7 = (int)(vector2D.Y - num * 0.5);
            int vinenum8 = (int)(vector2D.Y + num * 0.5);
            for (int vineX = vinenum5; vineX < vinenum6; vineX++) {
                for (int vineY = vinenum7; vineY < vinenum8; vineY++) {
                    int x2 = vineX, y2 = vineY;
                    var genRand = WorldGen.genRand;
                    if (Main.tile[x2, y2].HasTile && Main.tile[x2, y2].TileType == leafBlockTileType &&
                        !Main.tile[x2, y2 + 1].HasTile && genRand.NextBool(2)) {
                        bool flag5 = true;
                        ushort type7 = WorldGen.notTheBees ? (ushort)ModContent.TileType<MahoganyVines>() : HasSpiritModAndSavannahSeed ? GetSavannaVinesTileType() : Main.tile[x2, y2].WallType == WallID.FlowerUnsafe ? TileID.VineFlowers : TileID.Vines;
                        for (int num35 = y2; num35 > y2 - 10; num35--) {
                            if (Main.tile[x2, num35].BottomSlope) {
                                flag5 = false;
                                break;
                            }

                            if (Main.tile[x2, num35].HasTile && Main.tile[x2, num35].TileType == leafBlockTileType && !Main.tile[x2, num35].BottomSlope) {
                                flag5 = true;
                                break;
                            }
                        }
                        if (flag5) {
                            int height = genRand.Next(4, 9);
                            for (int num35 = y2; num35 < y2 + height; num35++) {
                                int num36 = num35 + 1;
                                Tile tile = Main.tile[x2, num36];
                                if (!tile.HasTile) {
                                    if (Main.tile[x2, num36 - 1].TileType == leafBlockTileType || Main.tile[x2, num36 - 1].TileType == type7) {
                                        tile.TileType = type7;
                                        tile.HasTile = true;
                                    }
                                }
                            }
                        }
                    }
                    if (Main.tile[x2, y2].TileType == 192 && genRand.NextBool(4)) {
                        WorldGen.PlaceTile(x2, y2 - 1, 187, mute: true, forced: false, -1, genRand.Next(50, 52));
                    }
                    if (genRand.NextBool(3)) {
                        WorldGen.PlacePot(x2, y2 - 1, style: WorldGen.notTheBees ? genRand.Next(7, 10) : genRand.Next(4));
                    }
                }
            }
        }

        if (!HasSpiritModAndSavannahSeed && WorldGen.tenthAnniversaryWorldGen) {
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

    private void ExtraMountCavesGenerator2(GenerationProgress progress, GameConfiguration configuration) {
        if (HasSpiritModAndSavannahSeed) {
            bool flag = true;
            while (flag) {
                int num1052 = 0;
                bool flag59 = false;
                bool flag60 = false;
                int fluff = 225/* * WorldGenHelper.WorldSize*/;
                if (ModLoader.TryGetMod("SpiritReforged", out Mod mod)) {
                    Rectangle savannaArea = (Rectangle)mod.Call("GetSavannaArea");
                    fluff = Math.Clamp(savannaArea.Width / 2 - 50, 50, 225);
                }
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
                    for (int num1055 = WorldGenHelper.SafeFloatingIslandY; (double)num1055 < Main.worldSurface; num1055++) {
                        if (Main.tile[num1053, num1055].HasTile) {
                            if (!flag59) {
                                Mountinater2_Savanna(num1053, num1055);
                                GenVars.mCaveX[GenVars.numMCaves] = num1053;
                                GenVars.mCaveY[GenVars.numMCaves] = num1055;
                                _dryadEntranceX = num1053;
                                _dryadEntranceY = num1055;
                                _dryadEntrancemCave = GenVars.numMCaves;
                                GenVars.numMCaves++;
                                flag = false;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void ExtraMountCavesGenerator(GenerationProgress progress, GameConfiguration configuration) {
        GenVars.numMCaves = 0;
        progress.Message = Lang.gen[2].Value;

        if (!HasSpiritModAndSavannahSeed) {
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
                                _dryadEntrancemCave = GenVars.numMCaves;
                                GenVars.numMCaves++;
                                flag = false;
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (!ModLoader.HasMod("Remnants")) {
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
    }

    private static void Mountinater2_Savanna(int i, int j) {
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
                        ushort savannaDirtTileType = GetSavannaDirtTileType();
                        Main.tile[k, l].TileType = savannaDirtTileType;
                        //bool flag = false;
                        //for (int checkX = k - 1; checkX < k + 2; checkX++) {
                        //    if (flag) {
                        //        break;
                        //    }
                        //    for (int checkY = l - 1; checkY < l + 2; checkY++) {
                        //        if (!Main.tile[checkX, checkY].HasTile) {
                        //            flag = true;
                        //            break;
                        //        }
                        //    }
                        //}
                        //if (!flag) {
                        //    Main.tile[k, l].WallType = savannaDirtWallType;
                        //}
                        //else {
                        //    Main.tile[k, l].TileType = savannaGrassTileType;
                        //}
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

    private static ushort GetSavannaVinesTileType() {
        ushort tileType = TileID.Vines;
        if (ModLoader.GetMod("SpiritReforged").TryFind<ModTile>("SavannaVine", out ModTile SavannaVine)) {
            tileType = SavannaVine.Type;
        }
        return tileType;
    }

    private static ushort GetSavannaLeafTileType() {
        ushort tileType = TileID.LeafBlock;
        if (ModLoader.GetMod("SpiritReforged").TryFind<ModTile>("LivingBaobabLeaf", out ModTile LivingBaobabLeaf)) {
            tileType = LivingBaobabLeaf.Type;
        }
        return tileType;
    }

    private static ushort GetSavannaWoodTileType() {
        ushort tileType = TileID.LivingWood;
        if (ModLoader.GetMod("SpiritReforged").TryFind<ModTile>("LivingBaobab", out ModTile LivingBaobab)) {
            tileType = LivingBaobab.Type;
        }
        return tileType;
    }

    private static ushort GetSavannaGrassTileType() {
        ushort tileType = TileID.HardenedSand;
        if (ModLoader.GetMod("SpiritReforged").TryFind<ModTile>("SavannaGrass", out ModTile SavannaGrass)) {
            tileType = SavannaGrass.Type;
        }
        return tileType;
    }

    private static ushort GetSavannaDirtTileType() {
        ushort tileType = TileID.Sand;
        if (ModLoader.GetMod("SpiritReforged").TryFind<ModTile>("SavannaDirt", out ModTile SavannaDirt)) {
            tileType = SavannaDirt.Type;
        }
        return tileType;
    }

    private static ushort GetSavannaWoodWallType() {
        ushort wallType = WallID.LivingWoodUnsafe;
        if (ModLoader.GetMod("SpiritReforged").TryFind<ModWall>("LivingBaobabWall", out ModWall LivingBaobabWall)) {
            wallType = LivingBaobabWall.Type;
        }
        return wallType;
    }

    private static ushort GetSavannaLeafWallType() {
        ushort wallType = WallID.LivingLeaf;
        if (ModLoader.GetMod("SpiritReforged").TryFind<ModWall>("LivingBaobabLeafWall", out ModWall LivingBaobabLeafWall)) {
            wallType = LivingBaobabLeafWall.Type;
        }
        return wallType;
    }

    private static ushort GetSavannaDirtWallType() {
        ushort wallType = WallID.HardenedSandEcho;
        if (ModLoader.GetMod("SpiritReforged").TryFind<ModWall>("SavannaDirtWallUnsafe", out ModWall SavannaDirtWallUnsafe)) {
            wallType = SavannaDirtWallUnsafe.Type;
        }
        return wallType;
    }

    private static ushort GetSpiritTileType(string name) {
        ushort tileType = TileID.Adamantite;
        if (ModLoader.GetMod("SpiritReforged").TryFind<ModTile>(name, out ModTile spiritTile)) {
            tileType = spiritTile.Type;
        }
        return tileType;
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
                        ushort dirtTileType = WorldGen.notTheBees ? TileID.Mud : HasSpiritModAndSavannahSeed ? GetSavannaDirtTileType() : TileID.Dirt;
                        Main.tile[k, l].TileType = dirtTileType;
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
        if (!HasSpiritModAndSavannahSeed) {
            Mountinater3_Inner(i, j, denom, ignoreWalls);
            return;
        }
        Mountinater3_Inner_Savanna(i, j, denom, ignoreWalls);
    }

    private static void Mountinater3_Inner_Savanna(int i, int j, int denom = 4, int[] ignoreWalls = null) {
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
                        ushort tileType = GetSavannaDirtTileType();
                        ushort wallType = GetSavannaDirtWallType();
                        Main.tile[k, l].TileType = tileType;
                        bool check = false;
                        for (int grassX = k - 1; grassX < k + 2; grassX++) {
                            if (check) {
                                break;
                            }
                            for (int grassY = l - 1; grassY < l + 2; grassY++) {
                                if (!Main.tile[grassX, grassY].HasTile) {
                                    check = true;
                                    break;
                                }
                            }
                        }
                        if (!check) {
                            Main.tile[k, l].WallType = wallType;
                        }
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

    private static void Mountinater3_Inner(int i, int j, int denom = 4, int[] ignoreWalls = null) {
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
                        ushort dirtTileType = WorldGen.notTheBees ? TileID.Mud : HasSpiritModAndSavannahSeed ? GetSavannaDirtTileType() : TileID.Dirt;
                        Main.tile[k, l].TileType = dirtTileType;
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
        if (!ModLoader.HasMod("Remnants")) {
            for (int num749 = 0; num749 < GenVars.numMCaves; num749++) {
                if (!ModLoader.HasMod("SpiritReforged") || num749 == _dryadEntrancemCave) {
                    if (num749 == _dryadEntrancemCave) {
                        int i3 = GenVars.mCaveX[num749];
                        int j5 = GenVars.mCaveY[num749];
                        CaveOpenater2(i3, j5);
                        Cavinator2(i3, j5, WorldGen.genRand.Next(40, 50));
                    }
                    else {
                        int i3 = GenVars.mCaveX[num749];
                        int j5 = GenVars.mCaveY[num749];
                        WorldGen.CaveOpenater(i3, j5);
                        WorldGen.Cavinator(i3, j5, WorldGen.genRand.Next(40, 50));
                    }
                }
            }
        }
        else if (!HasSpiritModAndSavannahSeed) {
            int i3 = GenVars.mCaveX[_dryadEntrancemCave];
            int j5 = GenVars.mCaveY[_dryadEntrancemCave];
            CaveOpenater2(i3, j5);
            Cavinator2(i3, j5, WorldGen.genRand.Next(40, 50));
        }

        if (!HasSpiritModAndSavannahSeed) {
            BuildDryadEntrance(_dryadEntranceX, _dryadEntranceY, progress);
        }
    }

    public static int FindGround(int i, ref int j) {
        while (j > 20 && WorldGen.SolidOrSlopedTile(i, j - 1))
            j--; //Up

        while (j < Main.maxTilesY - 20 && !WorldGen.SolidOrSlopedTile(i, j))
            j++; //Down

        return j;
    }

    private void DryadEntranceGenerator2(GenerationProgress progress, GameConfiguration configuration) {
        progress.Message = Lang.gen[21].Value;
        int i3 = GenVars.mCaveX[_dryadEntrancemCave];
        int j5 = GenVars.mCaveY[_dryadEntrancemCave];
        ushort savannaDirtTiletype = GetSavannaDirtTileType();
        ushort savannaDirtWallType = GetSavannaDirtWallType();
        ushort savannaGrassTileType = GetSavannaGrassTileType();

        for (int k = i3 - 50; k < i3 + 51; k++) {
            for (int l = j5 - 150; l < j5 + 20; l++) {
                if (Main.tile[k, l].TileType == savannaDirtTiletype || Main.tile[k, l].TileType == savannaGrassTileType) {
                    bool flag = false;
                    for (int checkX = k - 1; checkX < k + 2; checkX++) {
                        if (flag) {
                            break;
                        }
                        for (int checkY = l - 1; checkY < l + 2; checkY++) {
                            if (!Main.tile[checkX, checkY].HasTile) {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag) {
                        Main.tile[k, l].WallType = savannaDirtWallType;
                    }
                }
            }
        }

        CaveOpenater2(i3, j5);
        Cavinator2(i3, j5, WorldGen.genRand.Next(40, 50));

        var _random = WorldGen.genRand;
        for (int num696 = i3 - 50; num696 < i3 + 51; num696++) {
            double num697 = (double)num696 / (double)Main.maxTilesX;
            bool flag43 = true;
            for (int num698 = 0; (double)num698 < Main.worldSurface; num698++) {
                if (flag43) {
                    if (Main.tile[num696, num698].WallType == savannaDirtWallType || Main.tile[num696, num698].WallType == 40 || Main.tile[num696, num698].WallType == 64 || Main.tile[num696, num698].WallType == 86)
                        Main.tile[num696, num698].WallType = 0;

                    if (Main.tile[num696, num698].TileType != 53 && Main.tile[num696, num698].TileType != 112 && Main.tile[num696, num698].TileType != 234) {
                        if (Main.tile[num696 - 1, num698].WallType == savannaDirtWallType || Main.tile[num696 - 1, num698].WallType == 40 || Main.tile[num696 - 1, num698].WallType == 40)
                            Main.tile[num696 - 1, num698].WallType = 0;

                        if ((Main.tile[num696 - 2, num698].WallType == savannaDirtWallType || Main.tile[num696 - 2, num698].WallType == 40 || Main.tile[num696 - 2, num698].WallType == 40) && _random.Next(2) == 0)
                            Main.tile[num696 - 2, num698].WallType = 0;

                        if ((Main.tile[num696 - 3, num698].WallType == savannaDirtWallType || Main.tile[num696 - 3, num698].WallType == 40 || Main.tile[num696 - 3, num698].WallType == 40) && _random.Next(2) == 0)
                            Main.tile[num696 - 3, num698].WallType = 0;

                        if (Main.tile[num696 + 1, num698].WallType == savannaDirtWallType || Main.tile[num696 + 1, num698].WallType == 40 || Main.tile[num696 + 1, num698].WallType == 40)
                            Main.tile[num696 + 1, num698].WallType = 0;

                        if ((Main.tile[num696 + 2, num698].WallType == savannaDirtWallType || Main.tile[num696 + 2, num698].WallType == 40 || Main.tile[num696 + 2, num698].WallType == 40) && _random.Next(2) == 0)
                            Main.tile[num696 + 2, num698].WallType = 0;

                        if ((Main.tile[num696 + 3, num698].WallType == 2 || Main.tile[num696 + 3, num698].WallType == 40 || Main.tile[num696 + 3, num698].WallType == 40) && _random.Next(2) == 0)
                            Main.tile[num696 + 3, num698].WallType = 0;

                        if (Main.tile[num696, num698].HasTile)
                            flag43 = false;
                    }
                }
                else if (Main.tile[num696, num698].WallType == 0 && Main.tile[num696, num698 + 1].WallType == 0 && Main.tile[num696, num698 + 2].WallType == 0 && Main.tile[num696, num698 + 3].WallType == 0 && Main.tile[num696, num698 + 4].WallType == 0 && Main.tile[num696 - 1, num698].WallType == 0 && Main.tile[num696 + 1, num698].WallType == 0 && Main.tile[num696 - 2, num698].WallType == 0 && Main.tile[num696 + 2, num698].WallType == 0 && !Main.tile[num696, num698].HasTile && !Main.tile[num696, num698 + 1].HasTile && !Main.tile[num696, num698 + 2].HasTile && !Main.tile[num696, num698 + 3].HasTile) {
                    flag43 = true;
                }
            }
        }

        for (int num699 = i3 + 51; num699 >= i3 - 51; num699--) {
            double num700 = (double)num699 / (double)Main.maxTilesX;
            bool flag44 = true;
            for (int num701 = 0; (double)num701 < Main.worldSurface; num701++) {
                if (flag44) {
                    if (Main.tile[num699, num701].WallType == savannaDirtWallType || Main.tile[num699, num701].WallType == 40 || Main.tile[num699, num701].WallType == 64)
                        Main.tile[num699, num701].WallType = 0;

                    if (Main.tile[num699, num701].TileType != 53) {
                        if (Main.tile[num699 - 1, num701].WallType == savannaDirtWallType || Main.tile[num699 - 1, num701].WallType == 40 || Main.tile[num699 - 1, num701].WallType == 40)
                            Main.tile[num699 - 1, num701].WallType = 0;

                        if ((Main.tile[num699 - 2, num701].WallType == savannaDirtWallType || Main.tile[num699 - 2, num701].WallType == 40 || Main.tile[num699 - 2, num701].WallType == 40) && _random.Next(2) == 0)
                            Main.tile[num699 - 2, num701].WallType = 0;

                        if ((Main.tile[num699 - 3, num701].WallType == savannaDirtWallType || Main.tile[num699 - 3, num701].WallType == 40 || Main.tile[num699 - 3, num701].WallType == 40) && _random.Next(2) == 0)
                            Main.tile[num699 - 3, num701].WallType = 0;

                        if (Main.tile[num699 + 1, num701].WallType == savannaDirtWallType || Main.tile[num699 + 1, num701].WallType == 40 || Main.tile[num699 + 1, num701].WallType == 40)
                            Main.tile[num699 + 1, num701].WallType = 0;

                        if ((Main.tile[num699 + 2, num701].WallType == savannaDirtWallType || Main.tile[num699 + 2, num701].WallType == 40 || Main.tile[num699 + 2, num701].WallType == 40) && _random.Next(2) == 0)
                            Main.tile[num699 + 2, num701].WallType = 0;

                        if ((Main.tile[num699 + 3, num701].WallType == savannaDirtWallType || Main.tile[num699 + 3, num701].WallType == 40 || Main.tile[num699 + 3, num701].WallType == 40) && _random.Next(2) == 0)
                            Main.tile[num699 + 3, num701].WallType = 0;

                        if (Main.tile[num699, num701].HasTile)
                            flag44 = false;
                    }
                }
                else if (Main.tile[num699, num701].WallType == 0 && Main.tile[num699, num701 + 1].WallType == 0 && Main.tile[num699, num701 + 2].WallType == 0 && Main.tile[num699, num701 + 3].WallType == 0 && Main.tile[num699, num701 + 4].WallType == 0 && Main.tile[num699 - 1, num701].WallType == 0 && Main.tile[num699 + 1, num701].WallType == 0 && Main.tile[num699 - 2, num701].WallType == 0 && Main.tile[num699 + 2, num701].WallType == 0 && !Main.tile[num699, num701].HasTile && !Main.tile[num699, num701 + 1].HasTile && !Main.tile[num699, num701 + 2].HasTile && !Main.tile[num699, num701 + 3].HasTile) {
                    flag44 = true;
                }
            }
        }

        if (HasSpiritModAndSavannahSeed) {
            BuildDryadEntrance(_dryadEntranceX, _dryadEntranceY, progress);
        }

        for (int k = i3 - 50; k < i3 + 51; k++) {
            for (int l = j5 - 150; l < j5 + 20; l++) {
                if (Main.tile[k, l].TileType == savannaDirtTiletype) {
                    bool flag = false;
                    for (int checkX = k - 1; checkX < k + 2; checkX++) {
                        if (flag) {
                            break;
                        }
                        for (int checkY = l - 1; checkY < l + 2; checkY++) {
                            if (!Main.tile[checkX, checkY].HasTile) {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag) {
                    }
                    else {
                        Main.tile[k, l].TileType = savannaGrassTileType;
                    }
                }
                if (Main.tile[k, l].TileType == savannaGrassTileType) {

                }
            }
        }

        for (int k = i3 - 50; k < i3 + 51; k++) {
            for (int l = j5 - 150; l < j5 + 20; l++) {
                if (Main.tile[k, l].TileType == savannaDirtTiletype) {
                    bool flag = false;
                    for (int checkX = k - 1; checkX < k + 2; checkX++) {
                        if (flag) {
                            break;
                        }
                        for (int checkY = l - 1; checkY < l + 2; checkY++) {
                            if (!Main.tile[checkX, checkY].HasTile) {
                                flag = true;
                                break;
                            }
                        }
                    }
                    if (!flag) {
                    }
                    else {
                        Main.tile[k, l].TileType = savannaGrassTileType;
                    }
                }
                if (Main.tile[k, l].TileType == savannaGrassTileType && WorldGen.genRand.NextBool(2)) {
                    int i = k, j = l - 1;
                    if (WorldGen.genRand.NextBool(8)) //Surface pots
                        WorldGen.PlaceTile(i, j, TileID.Pots, true, true, style: 7);

                    if (WorldGen.genRand.NextBool(13)) //Elephant grass patch
                        CreatePatch(WorldGen.genRand.Next(5, 11), 0, WorldGen.genRand.Next([0, 5]), GetSpiritTileType("ElephantGrass"));

                    if (WorldGen.genRand.NextBool(9)) //Foliage patch
                        CreatePatch(WorldGen.genRand.Next(6, 13), 2, types: GetSpiritTileType("SavannaFoliage"));

                    if (WorldGen.genRand.NextBool(45)) //Termite mound
                    {
                        int type = WorldGen.genRand.NextFromList(GetSpiritTileType("TermiteMoundSmall"),
                            GetSpiritTileType("TermiteMoundMedium"), GetSpiritTileType("TermiteMoundLarge"));
                        int style = WorldGen.genRand.Next(TileObjectData.GetTileData(type, 0).RandomStyleRange);

                        WorldGen.PlaceTile(i, j, type, true, true, style: style);
                    }

                    void CreatePatch(int size, int chance, int alt = 0, params int[] types) {
                        for (int x = i - size / 2; x < i + size / 2; x++) {
                            if (chance > 1 && !WorldGen.genRand.NextBool(chance))
                                continue;

                            int y = j;
                            FindGround(x, ref y);

                            int type = types[WorldGen.genRand.Next(types.Length)];
                            int style = alt + WorldGen.genRand.Next(TileObjectData.GetTileData(type, 0, alt).RandomStyleRange);

                            WorldGen.PlaceTile(x, y - 1, type, true, style: style);
                        }
                    }
                }
            }
        }
    }

    public static void Cavinator2(int i, int j, int steps, bool randomDirection = false) {
        var genRand = WorldGen.genRand;
        double num = genRand.Next(7, 15);
        double num2 = num;
        int num3 = (i > Main.maxTilesX / 2).ToDirectionInt();

        if (randomDirection) {
            num3 = 1;
            if (genRand.Next(2) == 0)
                num3 = -1;
        }

        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = j;
        int num4 = genRand.Next(20, 40);
        int num42 = num4;
        Vector2D vector2D2 = default(Vector2D);
        vector2D2.Y = (double)genRand.Next(10, 20) * 0.01;
        vector2D2.X = num3;
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

            num2 = num * (double)genRand.Next(80, 120) * 0.01;
            for (int k = num5; k < num6; k++) {
                for (int l = num7; l < num8; l++) {
                    double num9 = Math.Abs((double)k - vector2D.X);
                    double num10 = Math.Abs((double)l - vector2D.Y);
                    if (Math.Sqrt(num9 * num9 + num10 * num10) < num2 * 0.4) {
                        if (HasSpiritModAndSavannahSeed || (TileID.Sets.CanBeClearedDuringGeneration[Main.tile[k, l].TileType] && Main.tile[k, l].TileType != TileID.Sand)) {
                            Tile tile2 = Main.tile[k, l];
                            tile2.HasTile = false;
                        }
                    }
                }
            }

            vector2D += vector2D2;
            vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
            if (num4 < num42 * 0.75f) {
                vector2D2.Y += (double)genRand.Next(-5, 11) * 0.1;
            }
            else {
            }
            if (vector2D2.X > (double)num3 + 0.5)
                vector2D2.X = (double)num3 + 0.5;

            if (vector2D2.X < (double)num3 - 0.5)
                vector2D2.X = (double)num3 - 0.5;

            if (vector2D2.Y > 2.0)
                vector2D2.Y = 2.0;

            if (vector2D2.Y < 0.0)
                vector2D2.Y = 0.0;
        }

        if (steps > 0 && (double)(int)vector2D.Y < Main.rockLayer + 50.0)
            Cavinator2((int)vector2D.X, (int)vector2D.Y, steps - 1, true);
    }

    public static void CaveOpenater2(int i, int j) {
        var genRand = WorldGen.genRand;
        double num = genRand.Next(7, 12);
        double num2 = num;
        int num3 = 1;
        if (genRand.Next(2) == 0)
            num3 = -1;

        if (genRand.Next(10) != 0)
            num3 = ((i < Main.maxTilesX / 2) ? 1 : (-1));

        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = j;
        int num4 = 100;
        int num42 = num4;
        Vector2D vector2D2 = default(Vector2D);
        vector2D2.Y = 0.0;
        vector2D2.X = num3;
        while (num4 > 0) {
            Tile tile = Main.tile[(int)vector2D.X, (int)vector2D.Y];
            if (tile.WallType == 0 || (tile.HasTile && !TileID.Sets.CanBeClearedDuringGeneration[tile.TileType]))
                num4 = 0;

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

            num2 = num * (double)genRand.Next(80, 120) * 0.01;
            for (int k = num5; k < num6; k++) {
                for (int l = num7; l < num8; l++) {
                    double num9 = Math.Abs((double)k - vector2D.X);
                    double num10 = Math.Abs((double)l - vector2D.Y);
                    if (Math.Sqrt(num9 * num9 + num10 * num10) < num2 * 0.4) {
                        Tile tile2 = Main.tile[k, l];
                        tile2.HasTile = false;
                    }
                }
            }

            vector2D += vector2D2;
            vector2D2.X += (double)genRand.Next(-10, 11) * 0.05;
            if (num4 < num42 * 0.75f) {
                vector2D2.Y += (double)genRand.Next(-10, 11) * 0.05;
            }
            else {
                vector2D2.Y += (double)genRand.Next(-10, 11) * 0.025;
            }
            if (vector2D2.X > (double)num3 + 0.5)
                vector2D2.X = (double)num3 + 0.5;

            if (vector2D2.X < (double)num3 - 0.5)
                vector2D2.X = (double)num3 - 0.5;

            if (vector2D2.Y > 0.0)
                vector2D2.Y = 0.0;

            if (vector2D2.Y < -0.5)
                vector2D2.Y = -0.5;
        }
    }

    private void Samples(int i, int j, out int result, out Vector2D result2) {
        double num = 10;
        double num2 = num;
        UnifiedRandom genRand = WorldGen.genRand;
        Vector2D vector2D2 = default(Vector2D);
        int size = 30;
        result = 0;
        result2 = default(Vector2D);
        int directions = 5;
        for (int k2 = 0; k2 < 3; k2++) {
            for (int x = -directions; x < directions + 2; x++) {
                Vector2D vector2D = default(Vector2D);
                vector2D.X = i;
                vector2D.Y = j;
                vector2D2.X = x * 0.01;
                vector2D2.Y = 20 * -0.05;
                //Vector2D vector2D22 = default(Vector2D);
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

    private void BuildDryadEntrance(int i, int j, GenerationProgress progress) {
        UnifiedRandom genRand = WorldGen.genRand;
        double num = 50;
        double num2 = num;
        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = j;
        ushort tileType = PlaceholderTileType;
        ushort wallType = PlaceholderWallType;
        int progressNum = 0;
        ushort leafBlockTileType = WorldGen.notTheBees ? TileID.LivingMahoganyLeaves : HasSpiritModAndSavannahSeed ? GetSavannaLeafTileType() : TileID.LeafBlock;
        ushort dirtTileType = WorldGen.notTheBees ? TileID.Mud : HasSpiritModAndSavannahSeed ? GetSavannaDirtTileType() : TileID.Dirt;
        WorldGenHelper.ModifiedTileRunner(i, j, num, (int)num / 3, leafBlockTileType, onIteration: () => {
            WorldGenHelper.TopSizeFactor = () => 0.55f;
            WorldGenHelper.BottomSizeFactor = () => 0.55f;
        }, onTilePlacement: (tilePosition) => {
            progressNum++;
            progress.Set((float)progressNum / num);

            int checkX = tilePosition.X, checkY = tilePosition.Y;
            int length = WorldGen.genRand.Next(1, 3);
            WorldGen.genRand.Next();
            for (int grassX = checkX - 2; grassX < checkX + 3; grassX++) {
                for (int grassY = checkY - 2; grassY < checkY + 3; grassY++) {
                    if (Main.tile[grassX, grassY].TileType == TileID.Grass || (WorldGen.notTheBees && Main.tile[grassX, grassY].TileType == TileID.JungleGrass) || (HasSpiritModAndSavannahSeed && Main.tile[grassX, grassY].TileType == GetSavannaGrassTileType())) {
                        Main.tile[grassX, grassY].TileType = dirtTileType;
                    }
                }
            }
            if (TileHelper.GetDistanceToFirstEmptyTileAround(checkX, checkY, checkDistance: (ushort)num) > length) {
                Main.tile[checkX, checkY].TileType = dirtTileType;
                return false;
            }
            if (TileHelper.HasNoDuplicateNeighbors(checkX, checkY, leafBlockTileType)) {
                Vector2D startPosition = tilePosition.ToVector2D(),
                         destination = new Point16(i, j).ToVector2D();
                startPosition = Vector2D.Lerp(startPosition, destination, 0.9f);
                while (Vector2D.Distance(startPosition, destination) > 2) {
                    startPosition = Vector2D.Lerp(startPosition, destination, 0.1f);
                    WorldGenHelper.ModifiedTileRunner((int)startPosition.X, (int)startPosition.Y, 4, 10, wallType:
                        WorldGen.notTheBees ? WallID.JungleUnsafe : HasSpiritModAndSavannahSeed ? GetSavannaLeafWallType() : WallID.FlowerUnsafe);
                }
            }
            return true;
        });

        num = 10;
        num2 = num;
        int num3 = 1;

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
                if (value > 0.5f && sizeValue < 0.75f) {
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
                            tile.WallType = wallType;
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
                        ushort dirtWall = WorldGen.notTheBees ? WallID.MudUnsafe : HasSpiritModAndSavannahSeed ? GetSavannaDirtWallType() : WallID.DirtUnsafe;
                        ushort dirtBlock = WorldGen.notTheBees ? TileID.Mud : HasSpiritModAndSavannahSeed ? GetSavannaDirtTileType() : TileID.Dirt;
                        bool check = false;
                        for (int grassX = x2 - 1; grassX < x2 + 2; grassX++) {
                            if (check) {
                                break;
                            }
                            for (int grassY = y3 - 1; grassY < y3 + 2; grassY++) {
                                if (!Main.tile[grassX, grassY].HasTile) {
                                    check = true;
                                    break;
                                }
                            }
                        }
                        if (!check) {
                            WorldGenHelper.ReplaceWall(x2, y3, dirtWall);
                        }
                        WorldGenHelper.ReplaceTile(x2, y3, dirtBlock);
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
                        bool check = false;
                        for (int grassX = x2 - 1; grassX < x2 + 2; grassX++) {
                            if (check) {
                                break;
                            }
                            for (int grassY = y3 - 1; grassY < y3 + 2; grassY++) {
                                if (!Main.tile[grassX, grassY].HasTile) {
                                    check = true;
                                    break;
                                }
                            }
                        }
                        if (!check) {
                            WorldGenHelper.ReplaceWall(x2, y3, wallType);
                        }
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
                for (int k = -1; k < 3; k++) {
                    if (!Main.tile[i + k, j + 1].HasTile) {
                        flag5 = false;
                    }
                }
                if (flag5) {
                    int altered = genRand.NextBool() ? 2 : 0;
                    WorldGen.Place2xX(i, j, treeDryad, altered);
                    if (Main.tile[i, j].TileType == treeDryad) {
                        Tile checkTile = Main.tile[i, j + 1];
                        checkTile.Slope = 0;
                        checkTile = Main.tile[i + 1, j + 1];
                        checkTile.Slope = 0;
                        checkTile = Main.tile[i - 1, j + 1];
                        checkTile.Slope = 0;
                        checkTile = Main.tile[i + 2, j + 1];
                        checkTile.Slope = 0;

                        flag4 = true;
                        _dryadStructureGenerated = true;
                    }
                }
            }
        }
        ushort tileType2 = WorldGen.notTheBees ? TileID.LivingMahoganyLeaves : HasSpiritModAndSavannahSeed ? GetSavannaLeafTileType() : TileID.LeafBlock;
        ushort wallType2 = WorldGen.notTheBees ? WallID.LivingWood : HasSpiritModAndSavannahSeed ? GetSavannaLeafWallType() : WallID.LivingLeaf;
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
                        ushort type7 = WorldGen.notTheBees ? (ushort)ModContent.TileType<MahoganyVines>() : HasSpiritModAndSavannahSeed ? GetSavannaVinesTileType() : TileID.Vines;
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
                                    if (Main.tile[x2, num36 - 1].TileType == leafBlockTileType || Main.tile[x2, num36 - 1].TileType == type7) {
                                        tile.TileType = type7;
                                        tile.HasTile = true;
                                    }
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
                    if (WorldGen.notTheBees) {
                        int tileType3 = ModContent.TileType<TreeDryadDecoration3_Jungle>();
                        WorldGen.PlaceTile(x2, y3 - 1, tileType3, mute: true, forced: false, -1, genRand.Next(0, 3));
                        if (Main.tile[x2, y3 - 1].TileType == tileType3) {
                            bigPlaced = true;
                            _bigRubblePosition = new Point(x2, y3 - 1);
                        }
                    }
                    else if (HasSpiritModAndSavannahSeed) {
                        int tileType3 = ModContent.TileType<TreeDryadDecoration3_Spirit>();
                        WorldGen.PlaceTile(x2, y3 - 1, tileType3, mute: true, forced: false, -1, genRand.Next(0, 3));
                        if (Main.tile[x2, y3 - 1].TileType == tileType3) {
                            bigPlaced = true;
                            _bigRubblePosition = new Point(x2, y3 - 1);
                        }
                    }
                    else {
                        WorldGen.PlaceTile(x2, y3 - 1, 187, mute: true, forced: false, -1, genRand.Next(50, 53) - 3);
                        if (Main.tile[x2, y3 - 1].TileType == 187) {
                            bigPlaced = true;
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
                    if ((HasSpiritModAndSavannahSeed || WorldGen.notTheBees) || (placedIndex % 2 == 0 && (!mediumPlaced1 || (genRand.NextBool(2) && mediumPlaced1)))) {
                        ushort type = WorldGen.notTheBees ? (ushort)ModContent.TileType<TreeDryadDecoration2_Jungle>() : HasSpiritModAndSavannahSeed ? (ushort)ModContent.TileType<TreeDryadDecoration2_Spirit>() : (ushort)ModContent.TileType<TreeDryadDecoration2>();
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
                    if ((HasSpiritModAndSavannahSeed || WorldGen.notTheBees) || (placedIndex % 2 == 0 && (!smallPlaced1 || (genRand.NextBool(2) && smallPlaced1)))) {
                        WorldGen.PlaceSmallPile(x2, y3 - 1, genRand.Next(2), 0, WorldGen.notTheBees ? (ushort)ModContent.TileType<TreeDryadDecoration1_Jungle>() : HasSpiritModAndSavannahSeed ? (ushort)ModContent.TileType<TreeDryadDecoration1_Spirit>() : (ushort)ModContent.TileType<TreeDryadDecoration1>());
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
