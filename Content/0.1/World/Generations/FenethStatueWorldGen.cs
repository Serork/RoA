using RoA.Content.Tiles.Ambient;
using RoA.Content.Tiles.Station;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.World.Generations;

sealed class FenethStatueWorldGen : ModSystem {
    private static bool _fenethStatuePlaced;

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Underworld"));
        tasks.RemoveAt(genIndex);

        tasks.Insert(genIndex, new PassLegacy("Underworld", UnderworldGenerator, 9213.443f));

        if (ModLoader.HasMod("TheDepths")) {
            genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Underworld"));
            tasks.Insert(genIndex + 2, new PassLegacy(string.Empty, TheDepthFenethStatueWorldGen, 10));
        }
    }

    public static ushort GetUnderworldGrassTileType() {
        ushort tileType = TileID.AshGrass;
        if (ModLoader.TryGetMod("TheDepths", out Mod theDepths) && theDepths.TryFind<ModTile>("NightmareGrass", out ModTile NightmareGrass)) {
            tileType = NightmareGrass.Type;
        }
        return tileType;
    }

    private void TheDepthFenethStatueWorldGen(GenerationProgress progress, GameConfiguration configuration) {
        _fenethStatuePlaced = false;
        int jungleSide = (GenVars.JungleX < Main.maxTilesX / 2).ToInt();
        bool hasSpooky = false;
        bool hasCalamity = false;
        var genRand = WorldGen.genRand;
        int startX = hasSpooky ? jungleSide != 1 ? 100 : Main.maxTilesX / 2 : hasCalamity ? Main.maxTilesX / 2 : 100;
        int endX = hasSpooky ? jungleSide != 1 ? Main.maxTilesX / 2 : (Main.maxTilesX - 100) : (Main.maxTilesX - 100);
        ushort grassTileType = GetUnderworldGrassTileType();
        int attempts = 1000;
        while (!_fenethStatuePlaced && attempts-- > 0) {
            for (int num868 = startX;
                num868 < endX; num868++) {
                if ((double)num868 < (double)Main.maxTilesX * 0.17 || (double)num868 > (double)Main.maxTilesX * 0.83) {
                    if (_fenethStatuePlaced) {
                        break;
                    }
                    for (int num869 = Main.maxTilesY - 170; num869 < Main.maxTilesY - 140; num869++) {
                        if (Main.tile[num868, num869].LiquidAmount <= 0 && Main.tile[num868, num869].TileType == grassTileType && Main.tile[num868, num869].HasTile && !Main.tile[num868, num869 - 1].HasTile && genRand.Next(30) == 0) {
                            ushort type = (ushort)ModContent.TileType<FenethStatue>();
                            WorldGenHelper.Place3x4(num868, num869 - 1, type, genRand.NextBool().ToInt(), PaintID.DeepPurplePaint);
                            if (Main.tile[num868, num869 - 1].TileType == type) {
                                _fenethStatuePlaced = true;
                                for (int i = num868 - 20; i < num868 + 21; i++) {
                                    for (int j = num869 - 20; j < num869 + 21; j++) {
                                        Main.tile[i, j].LiquidAmount = 0;
                                    }
                                }
                                for (int i = num868 - 10; i < num868 + 11; i++) {
                                    for (int j = num869 - 10; j < num869 + 11; j++) {
                                        if (genRand.NextChance(0.7f) && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == grassTileType &&
                                            !Main.tile[i, j].IsHalfBlock && Main.tile[i, j].Slope == 0) {
                                            if (genRand.NextBool(10)) {
                                                WorldGen.PlaceTile(i, j - 1, TileID.BloomingHerbs, style: 5);
                                            }
                                            else {
                                                int flowersType = ModContent.TileType<FenethStatueFlowers>();
                                                WorldGen.PlaceTile(i, j - 1, flowersType, style: genRand.Next(4));
                                                Tile tile = Main.tile[i, j - 1];
                                                if (tile.TileType == flowersType) {
                                                    tile.TileColor = PaintID.DeepPurplePaint;
                                                }
                                            }
                                        }
                                    }
                                }
                                //Console.WriteLine(num868 + " " + num869);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void UnderworldGenerator(GenerationProgress progress, GameConfiguration configuration) {
        _fenethStatuePlaced = false;

        progress.Message = Lang.gen[18].Value;
        progress.Set(0.0);
        var genRand = WorldGen.genRand;
        int num838 = Main.maxTilesY - genRand.Next(150, 190);
        for (int num839 = 0; num839 < Main.maxTilesX; num839++) {
            num838 += genRand.Next(-3, 4);
            if (num838 < Main.maxTilesY - 190)
                num838 = Main.maxTilesY - 190;

            if (num838 > Main.maxTilesY - 160)
                num838 = Main.maxTilesY - 160;

            for (int num840 = num838 - 20 - genRand.Next(3); num840 < Main.maxTilesY; num840++) {
                if (num840 >= num838) {
                    Tile tile = Main.tile[num839, num840];
                    tile.HasTile = false;
                    Main.tile[num839, num840].LiquidAmount = 0;
                }
                else {
                    Main.tile[num839, num840].TileType = 57;
                }
            }
        }

        int num841 = Main.maxTilesY - genRand.Next(40, 70);
        for (int num842 = 10; num842 < Main.maxTilesX - 10; num842++) {
            num841 += genRand.Next(-10, 11);
            if (num841 > Main.maxTilesY - 60)
                num841 = Main.maxTilesY - 60;

            if (num841 < Main.maxTilesY - 100)
                num841 = Main.maxTilesY - 120;

            for (int num843 = num841; num843 < Main.maxTilesY - 10; num843++) {
                if (!Main.tile[num842, num843].HasTile) {
                    Tile tile = Main.tile[num842, num843];
                    tile.LiquidType = LiquidID.Lava;
                    Main.tile[num842, num843].LiquidAmount = byte.MaxValue;
                }
            }
        }

        for (int num844 = 0; num844 < Main.maxTilesX; num844++) {
            if (genRand.Next(50) == 0) {
                int num845 = Main.maxTilesY - 65;
                while (!Main.tile[num844, num845].HasTile && num845 > Main.maxTilesY - 135) {
                    num845--;
                }

                WorldGen.TileRunner(genRand.Next(0, Main.maxTilesX), num845 + genRand.Next(20, 50), genRand.Next(15, 20), 1000, 57, addTile: true, 0.0, genRand.Next(1, 3), noYChange: true);
            }
        }

        Liquid.QuickWater(-2);
        for (int num846 = 0; num846 < Main.maxTilesX; num846++) {
            double num847 = (double)num846 / (double)(Main.maxTilesX - 1);
            progress.Set(num847 / 2.0 + 0.5);
            if (genRand.Next(13) == 0) {
                int num848 = Main.maxTilesY - 65;
                while ((Main.tile[num846, num848].LiquidAmount > 0 || Main.tile[num846, num848].HasTile) && num848 > Main.maxTilesY - 140) {
                    num848--;
                }

                if ((!WorldGen.drunkWorldGen && !WorldGen.remixWorldGen) || genRand.Next(3) == 0 || !((double)num846 > (double)Main.maxTilesX * 0.4) || !((double)num846 < (double)Main.maxTilesX * 0.6))
                    WorldGen.TileRunner(num846, num848 - genRand.Next(2, 5), genRand.Next(5, 30), 1000, 57, addTile: true, 0.0, genRand.Next(1, 3), noYChange: true);

                double num849 = genRand.Next(1, 3);
                if (genRand.Next(3) == 0)
                    num849 *= 0.5;

                if ((!WorldGen.drunkWorldGen && !WorldGen.remixWorldGen) || genRand.Next(3) == 0 || !((double)num846 > (double)Main.maxTilesX * 0.4) || !((double)num846 < (double)Main.maxTilesX * 0.6)) {
                    if (genRand.Next(2) == 0)
                        WorldGen.TileRunner(num846, num848 - genRand.Next(2, 5), (int)((double)genRand.Next(5, 15) * num849), (int)((double)genRand.Next(10, 15) * num849), 57, addTile: true, 1.0, 0.3);

                    if (genRand.Next(2) == 0) {
                        num849 = genRand.Next(1, 3);
                        WorldGen.TileRunner(num846, num848 - genRand.Next(2, 5), (int)((double)genRand.Next(5, 15) * num849), (int)((double)genRand.Next(10, 15) * num849), 57, addTile: true, -1.0, 0.3);
                    }
                }

                WorldGen.TileRunner(num846 + genRand.Next(-10, 10), num848 + genRand.Next(-10, 10), genRand.Next(5, 15), genRand.Next(5, 10), -2, addTile: false, genRand.Next(-1, 3), genRand.Next(-1, 3));
                if (genRand.Next(3) == 0)
                    WorldGen.TileRunner(num846 + genRand.Next(-10, 10), num848 + genRand.Next(-10, 10), genRand.Next(10, 30), genRand.Next(10, 20), -2, addTile: false, genRand.Next(-1, 3), genRand.Next(-1, 3));

                if (genRand.Next(5) == 0)
                    WorldGen.TileRunner(num846 + genRand.Next(-15, 15), num848 + genRand.Next(-15, 10), genRand.Next(15, 30), genRand.Next(5, 20), -2, addTile: false, genRand.Next(-1, 3), genRand.Next(-1, 3));
            }
        }

        for (int num850 = 0; num850 < Main.maxTilesX; num850++) {
            WorldGen.TileRunner(genRand.Next(20, Main.maxTilesX - 20), genRand.Next(Main.maxTilesY - 180, Main.maxTilesY - 10), genRand.Next(2, 7), genRand.Next(2, 7), -2);
        }

        if (WorldGen.drunkWorldGen || WorldGen.remixWorldGen) {
            for (int num851 = 0; num851 < Main.maxTilesX * 2; num851++) {
                WorldGen.TileRunner(genRand.Next((int)((double)Main.maxTilesX * 0.35), (int)((double)Main.maxTilesX * 0.65)), genRand.Next(Main.maxTilesY - 180, Main.maxTilesY - 10), genRand.Next(5, 20), genRand.Next(5, 10), -2);
            }
        }

        for (int num852 = 0; num852 < Main.maxTilesX; num852++) {
            if (!Main.tile[num852, Main.maxTilesY - 145].HasTile) {
                Main.tile[num852, Main.maxTilesY - 145].LiquidAmount = byte.MaxValue;
                Tile tile = Main.tile[num852, Main.maxTilesY - 145];
                tile.LiquidType = LiquidID.Lava;
            }

            if (!Main.tile[num852, Main.maxTilesY - 144].HasTile) {
                Main.tile[num852, Main.maxTilesY - 144].LiquidAmount = byte.MaxValue;
                Tile tile = Main.tile[num852, Main.maxTilesY - 144];
                tile.LiquidType = LiquidID.Lava;
            }
        }

        for (int num853 = 0; num853 < (int)((double)(Main.maxTilesX * Main.maxTilesY) * 0.0008); num853++) {
            WorldGen.TileRunner(genRand.Next(0, Main.maxTilesX), genRand.Next(Main.maxTilesY - 140, Main.maxTilesY), genRand.Next(2, 7), genRand.Next(3, 7), 58);
        }

        if (WorldGen.remixWorldGen) {
            int num854 = (int)((double)Main.maxTilesX * 0.38);
            int num855 = (int)((double)Main.maxTilesX * 0.62);
            int num856 = num854;
            int num857 = Main.maxTilesY - 1;
            int num858 = Main.maxTilesY - 135;
            int num859 = Main.maxTilesY - 160;
            bool flag55 = false;
            Liquid.QuickWater(-2);
            for (; num857 < Main.maxTilesY - 1 || num856 < num855; num856++) {
                if (!flag55) {
                    num857 -= genRand.Next(1, 4);
                    if (num857 < num858)
                        flag55 = true;
                }
                else if (num856 >= num855) {
                    num857 += genRand.Next(1, 4);
                    if (num857 > Main.maxTilesY - 1)
                        num857 = Main.maxTilesY - 1;
                }
                else {
                    if ((num856 <= Main.maxTilesX / 2 - 5 || num856 >= Main.maxTilesX / 2 + 5) && genRand.Next(4) == 0) {
                        if (genRand.Next(3) == 0)
                            num857 += genRand.Next(-1, 2);
                        else if (genRand.Next(6) == 0)
                            num857 += genRand.Next(-2, 3);
                        else if (genRand.Next(8) == 0)
                            num857 += genRand.Next(-4, 5);
                    }

                    if (num857 < num859)
                        num857 = num859;

                    if (num857 > num858)
                        num857 = num858;
                }

                for (int num860 = num857; num860 > num857 - 20; num860--) {
                    Main.tile[num856, num860].LiquidAmount = 0;
                }

                for (int num861 = num857; num861 < Main.maxTilesY; num861++) {
                    /*
                    Main.tile[num856, num861] = new Tile();
                    */
                    Tile tile = Main.tile[num856, num861];
                    tile.Clear(TileDataType.All);
                    tile.HasTile = true;
                    tile.TileType = 57;
                }
            }

            Liquid.QuickWater(-2);
            for (int num862 = num854; num862 < num855 + 15; num862++) {
                for (int num863 = Main.maxTilesY - 300; num863 < num858 + 20; num863++) {
                    Main.tile[num862, num863].LiquidAmount = 0;
                    if (Main.tile[num862, num863].TileType == 57 && Main.tile[num862, num863].HasTile && (!Main.tile[num862 - 1, num863 - 1].HasTile || !Main.tile[num862, num863 - 1].HasTile || !Main.tile[num862 + 1, num863 - 1].HasTile || !Main.tile[num862 - 1, num863].HasTile || !Main.tile[num862 + 1, num863].HasTile || !Main.tile[num862 - 1, num863 + 1].HasTile || !Main.tile[num862, num863 + 1].HasTile || !Main.tile[num862 + 1, num863 + 1].HasTile))
                        Main.tile[num862, num863].TileType = 633;
                }
            }

            for (int num864 = num854; num864 < num855 + 15; num864++) {
                for (int num865 = Main.maxTilesY - 200; num865 < num858 + 20; num865++) {
                    if (Main.tile[num864, num865].TileType == 633 && Main.tile[num864, num865].HasTile && !Main.tile[num864, num865 - 1].HasTile && genRand.Next(3) == 0)
                        WorldGen.TryGrowingTreeByType(634, num864, num865);
                }
            }
        }
        else if (!WorldGen.drunkWorldGen) {
            for (int num866 = 25; num866 < Main.maxTilesX - 25; num866++) {
                if ((double)num866 < (double)Main.maxTilesX * 0.17 || (double)num866 > (double)Main.maxTilesX * 0.83) {
                    for (int num867 = Main.maxTilesY - 300; num867 < Main.maxTilesY - 100 + genRand.Next(-1, 2); num867++) {
                        if (Main.tile[num866, num867].TileType == 57 && Main.tile[num866, num867].HasTile && (!Main.tile[num866 - 1, num867 - 1].HasTile || !Main.tile[num866, num867 - 1].HasTile || !Main.tile[num866 + 1, num867 - 1].HasTile || !Main.tile[num866 - 1, num867].HasTile || !Main.tile[num866 + 1, num867].HasTile || !Main.tile[num866 - 1, num867 + 1].HasTile || !Main.tile[num866, num867 + 1].HasTile || !Main.tile[num866 + 1, num867 + 1].HasTile))
                            Main.tile[num866, num867].TileType = 633;
                    }
                }
            }
            bool hasCalamity = ModLoader.HasMod("CalamityMod");
            bool hasSpooky = ModLoader.HasMod("Spooky");
            int jungleSide = (GenVars.JungleX < Main.maxTilesX / 2).ToInt();
            int startX = hasSpooky ? jungleSide != 1 ? 100 : Main.maxTilesX / 2 : hasCalamity ? Main.maxTilesX / 2 : 100;
            int endX = hasSpooky ? jungleSide != 1 ? Main.maxTilesX / 2 : (Main.maxTilesX - 100) : (Main.maxTilesX - 100);
            while (!_fenethStatuePlaced) {
                for (int num868 = startX;
                    num868 < endX; num868++) {
                    if ((double)num868 < (double)Main.maxTilesX * 0.17 || (double)num868 > (double)Main.maxTilesX * 0.83) {
                        if (_fenethStatuePlaced) {
                            break;
                        }
                        for (int num869 = Main.maxTilesY - 170; num869 < Main.maxTilesY - 140; num869++) {
                            if (Main.tile[num868, num869].LiquidAmount <= 0 && Main.tile[num868, num869].TileType == 633 && Main.tile[num868, num869].HasTile && !Main.tile[num868, num869 - 1].HasTile && genRand.Next(30) == 0) {
                                ushort type = (ushort)ModContent.TileType<FenethStatue>();
                                WorldGen.Place3x4(num868, num869 - 1, type, genRand.NextBool().ToInt());
                                if (Main.tile[num868, num869 - 1].TileType == type) {
                                    _fenethStatuePlaced = true;
                                    for (int i = num868 - 20; i < num868 + 21; i++) {
                                        for (int j = num869 - 20; j < num869 + 21; j++) {
                                            Main.tile[i, j].LiquidAmount = 0;
                                        }
                                    }
                                    for (int i = num868 - 10; i < num868 + 11; i++) {
                                        for (int j = num869 - 10; j < num869 + 11; j++) {
                                            if (genRand.NextChance(0.7f) && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == TileID.AshGrass &&
                                                !Main.tile[i, j].IsHalfBlock && Main.tile[i, j].Slope == 0) {
                                                if (genRand.NextBool(10)) {
                                                    WorldGen.PlaceTile(i, j - 1, TileID.BloomingHerbs, style: 5);
                                                }
                                                else {
                                                    WorldGen.PlaceTile(i, j - 1, ModContent.TileType<FenethStatueFlowers>(), style: genRand.Next(4));
                                                }
                                            }
                                        }
                                    }
                                    //Console.WriteLine(num868 + " " + num869);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            while (!_fenethStatuePlaced) {
                for (int num868 = 100;
                    num868 < (Main.maxTilesX - 100); num868++) {
                    if ((double)num868 < (double)Main.maxTilesX * 0.34 || (double)num868 > (double)Main.maxTilesX * 0.66) {
                        if (_fenethStatuePlaced) {
                            break;
                        }
                        for (int num869 = Main.maxTilesY - 170; num869 < Main.maxTilesY - 140; num869++) {
                            if (Main.tile[num868, num869].LiquidAmount <= 0 && Main.tile[num868, num869].TileType == TileID.Ash && Main.tile[num868, num869].HasTile && !Main.tile[num868, num869 - 1].HasTile && genRand.Next(30) == 0) {
                                ushort type = (ushort)ModContent.TileType<FenethStatue>();
                                WorldGen.Place3x4(num868, num869 - 1, type, genRand.NextBool().ToInt());
                                if (Main.tile[num868, num869 - 1].TileType == type) {
                                    _fenethStatuePlaced = true;
                                    for (int i = num868 - 20; i < num868 + 21; i++) {
                                        for (int j = num869 - 20; j < num869 + 21; j++) {
                                            Main.tile[i, j].LiquidAmount = 0;
                                        }
                                    }
                                    for (int i = num868 - 10; i < num868 + 11; i++) {
                                        for (int j = num869 - 10; j < num869 + 11; j++) {
                                            if (genRand.NextChance(0.7f) && Main.tile[i, j].HasTile && Main.tile[i, j].TileType == TileID.Ash &&
                                                !Main.tile[i, j].IsHalfBlock && Main.tile[i, j].Slope == 0) {
                                                if (genRand.NextBool(10)) {
                                                    WorldGen.PlaceTile(i, j - 1, TileID.BloomingHerbs, style: 5);
                                                }
                                                else {
                                                    WorldGen.PlaceTile(i, j - 1, ModContent.TileType<FenethStatueFlowers>(), style: genRand.Next(4));
                                                }
                                            }
                                        }
                                    }
                                    //Console.WriteLine(num868 + " " + num869);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            for (int num868 = 25; num868 < Main.maxTilesX - 25; num868++) {
                if ((double)num868 < (double)Main.maxTilesX * 0.17 || (double)num868 > (double)Main.maxTilesX * 0.83) {
                    for (int num869 = Main.maxTilesY - 200; num869 < Main.maxTilesY - 50; num869++) {
                        if (Main.tile[num868, num869].TileType == 633 && Main.tile[num868, num869].HasTile && !Main.tile[num868, num869 - 1].HasTile && genRand.Next(3) == 0)
                            WorldGen.TryGrowingTreeByType(634, num868, num869);
                    }
                }
            }
        }

        WorldGen.AddHellHouses();
        if (WorldGen.drunkWorldGen) {
            for (int num870 = 25; num870 < Main.maxTilesX - 25; num870++) {
                for (int num871 = Main.maxTilesY - 300; num871 < Main.maxTilesY - 100 + genRand.Next(-1, 2); num871++) {
                    if (Main.tile[num870, num871].TileType == 57 && Main.tile[num870, num871].HasTile && (!Main.tile[num870 - 1, num871 - 1].HasTile || !Main.tile[num870, num871 - 1].HasTile || !Main.tile[num870 + 1, num871 - 1].HasTile || !Main.tile[num870 - 1, num871].HasTile || !Main.tile[num870 + 1, num871].HasTile || !Main.tile[num870 - 1, num871 + 1].HasTile || !Main.tile[num870, num871 + 1].HasTile || !Main.tile[num870 + 1, num871 + 1].HasTile))
                        Main.tile[num870, num871].TileType = 633;
                }
            }

            for (int num872 = 25; num872 < Main.maxTilesX - 25; num872++) {
                for (int num873 = Main.maxTilesY - 200; num873 < Main.maxTilesY - 50; num873++) {
                    if (Main.tile[num872, num873].TileType == 633 && Main.tile[num872, num873].HasTile && !Main.tile[num872, num873 - 1].HasTile && genRand.Next(3) == 0)
                        WorldGen.TryGrowingTreeByType(634, num872, num873);
                }
            }
        }
    }
}
