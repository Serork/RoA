using Ionic.Zlib;

using RoA.Content.Items.Weapons.Druidic;
using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Content.Items.Weapons.Druidic.Rods;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Common;

sealed class ExtraChestItems : ModSystem {
    private bool _cactiCactusRodAdded;
    private bool _iceRodAdded;
    private bool _mushroomStaffAdded;
    private bool _hellfireClawsAdded;

    public override void Load() {
        On_WorldGen.AddBuriedChest_int_int_int_bool_int_bool_ushort += On_WorldGen_AddBuriedChest_int_int_int_bool_int_bool_ushort;
    }

    public override void PostWorldGen() {
        _cactiCactusRodAdded = false;
        _iceRodAdded = false;
        _mushroomStaffAdded = false;
        _hellfireClawsAdded = false;
    }

    private static bool IsUndergroundDesert(int x, int y) {
        if ((double)y < Main.worldSurface)
            return false;

        if ((double)x < (double)Main.maxTilesX * 0.15 || (double)x > (double)Main.maxTilesX * 0.85)
            return false;

        if (WorldGen.remixWorldGen && (double)y > Main.rockLayer)
            return false;

        int num = 15;
        for (int i = x - num; i <= x + num; i++) {
            for (int j = y - num; j <= y + num; j++) {
                if (Main.tile[i, j].WallType == 187 || Main.tile[i, j].WallType == 216)
                    return true;
            }
        }

        return false;
    }

    private static bool IsDungeon(int x, int y) {
        if ((double)y < Main.worldSurface)
            return false;

        if (x < 0 || x > Main.maxTilesX)
            return false;

        if (Main.wallDungeon[Main.tile[x, y].WallType])
            return true;

        return false;
    }

    private bool On_WorldGen_AddBuriedChest_int_int_int_bool_int_bool_ushort(On_WorldGen.orig_AddBuriedChest_int_int_int_bool_int_bool_ushort orig, int i, int j, int contain, bool notNearOtherChests, int Style, bool trySlope, ushort chestTileType) {
        if (chestTileType == 0)
            chestTileType = 21;

        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        bool flag5 = false;
        bool flag6 = false;
        bool flag7 = false;
        bool flag8 = false;
        bool flag9 = false;
        bool flag10 = false;
        int num = 15;
        if (WorldGen.tenthAnniversaryWorldGen)
            num *= 3;

        for (int k = j; k < Main.maxTilesY - 10; k++) {
            int num2 = -1;
            int num3 = -1;
            if (Main.tile[i, k].LiquidType == LiquidID.Shimmer)
                return false;

            if (trySlope && Main.tile[i, k].HasTile && Main.tileSolid[Main.tile[i, k].TileType] && !Main.tileSolidTop[Main.tile[i, k].TileType]) {
                if (Style == 17) {
                    int num4 = 30;
                    for (int l = i - num4; l <= i + num4; l++) {
                        for (int m = k - num4; m <= k + num4; m++) {
                            if (!WorldGen.InWorld(l, m, 5))
                                return false;

                            if (Main.tile[l, m].HasTile && (Main.tile[l, m].TileType == 21 || Main.tile[l, m].TileType == 467))
                                return false;
                        }
                    }
                }

                if (Main.tile[i - 1, k].TopSlope) {
                    num2 = (int)Main.tile[i - 1, k].Slope;
                    Tile tile = Main.tile[i - 1, k];
                    tile.Slope = 0;
                }

                if (Main.tile[i, k].TopSlope) {
                    num3 = (int)Main.tile[i, k].Slope;
                    Tile tile = Main.tile[i, k];
                    tile.Slope = 0;
                }
            }

            if (WorldGen.remixWorldGen && (double)i > (double)Main.maxTilesX * 0.37 && (double)i < (double)Main.maxTilesX * 0.63 && k > Main.maxTilesY - 250)
                return false;

            int num5 = 2;
            for (int n = i - num5; n <= i + num5; n++) {
                for (int num6 = k - num5; num6 <= k + num5; num6++) {
                    if (Main.tile[n, num6].HasTile && (TileID.Sets.Boulders[Main.tile[n, num6].TileType] || Main.tile[n, num6].TileType == 26 || Main.tile[n, num6].TileType == 237))
                        return false;
                }
            }

            if (!WorldGen.SolidTile(i, k))
                continue;

            UnifiedRandom genRand = WorldGen.genRand;

            bool flag11 = false;
            int num7 = k;
            int num8 = -1;
            int num9 = 0;
            bool flag12 = (double)num7 >= Main.worldSurface + 25.0;
            if (WorldGen.remixWorldGen)
                flag12 = num7 < Main.maxTilesY - 400;

            if (flag12 || contain > 0)
                num9 = 1;

            if (Style >= 0)
                num9 = Style;

            if ((chestTileType == 467 && num9 == 10) || (contain == 0 && num7 <= Main.maxTilesY - 205 && IsUndergroundDesert(i, k))) {
                flag2 = true;
                num9 = 10;
                chestTileType = 467;
                bool added = false;
                if (!_cactiCactusRodAdded) {
                    contain = (short)ModContent.ItemType<CactiCaster>();
                    _cactiCactusRodAdded = true;
                    added = true;
                }
                if (!added) {
                    contain = ((num7 <= (GenVars.desertHiveHigh * 3 + GenVars.desertHiveLow * 4) / 7) ? Utils.SelectRandom(genRand, new short[5] {
                        (short)ModContent.ItemType<CactiCaster>(),
                        4056,
                        4055,
                        4262,
                        4263
                    }) : Utils.SelectRandom(genRand, new short[4] {
                        (short)ModContent.ItemType<CactiCaster>(),
                        4061,
                        4062,
                        4276
                    }));

                    if (WorldGen.getGoodWorldGen && genRand.Next(num) == 0)
                        contain = 52;
                }
            }

            if (chestTileType == 21 && (num9 == 11 || (contain == 0 && (double)num7 >= Main.worldSurface + 25.0 && num7 <= Main.maxTilesY - 205 && (Main.tile[i, k].TileType == 147 || Main.tile[i, k].TileType == 161 || Main.tile[i, k].TileType == 162)))) {
                flag = true;
                num9 = 11;
                bool added = false;
                short iceRodType = (short)ModContent.ItemType<SpikedIceStaff>();
                if (!_iceRodAdded) {
                    contain = iceRodType;
                    _iceRodAdded = true;
                    added = true;
                }
                if (!added) {
                    switch (genRand.Next(7)) {
                        case 0:
                            contain = 670;
                            break;
                        case 1:
                            contain = 724;
                            break;
                        case 2:
                            contain = 950;
                            break;
                        case 3:
                            contain = ((!WorldGen.remixWorldGen) ? 1319 : 725);
                            break;
                        case 4:
                            contain = 987;
                            break;
                        case 5:
                            contain = iceRodType;
                            break;
                        default:
                            contain = 1579;
                            break;
                    }

                    if (genRand.Next(20) == 0)
                        contain = 997;

                    if (genRand.Next(50) == 0)
                        contain = 669;

                    if (WorldGen.getGoodWorldGen && genRand.Next(num) == 0)
                        contain = 52;
                }
            }

            if (chestTileType == 21 && (Style == 10 || contain == 211 || contain == 212 || contain == 213 || contain == 753)) {
                flag3 = true;
                num9 = 10;
                if (WorldGen.getGoodWorldGen && genRand.Next(num) == 0)
                    contain = 52;
            }

            if (chestTileType == 21 && num7 > Main.maxTilesY - 205 && contain == 0) {
                flag7 = true;
                contain = GenVars.hellChestItem[GenVars.hellChest];
                int hellfireClawsType = ModContent.ItemType<HellfireClaws>();
                bool added = false;
                if (!_hellfireClawsAdded) {
                    contain = hellfireClawsType;
                    _hellfireClawsAdded = true;
                    added = true;
                }
                if (genRand.NextBool(GenVars.hellChestItem.Length)) {
                    contain = hellfireClawsType;
                }
                num9 = 4;
                flag11 = true;
                if (!added && WorldGen.getGoodWorldGen && genRand.Next(num) == 0)
                    contain = 52;
            }

            if (chestTileType == 21 && num9 == 17) {
                flag4 = true;
                if (WorldGen.getGoodWorldGen && genRand.Next(num) == 0)
                    contain = 52;
            }

            if (chestTileType == 21 && num9 == 12) {
                flag5 = true;
                if (WorldGen.getGoodWorldGen && genRand.Next(num) == 0)
                    contain = 52;
            }

            if (chestTileType == 21 && num9 == 32) {
                flag6 = true;
                if (WorldGen.getGoodWorldGen && genRand.Next(num) == 0)
                    contain = 52;
            }

            if (chestTileType == 21 && num9 != 0 && IsDungeon(i, k))
                flag8 = true;

            if (chestTileType == 21 && num9 != 0 && (contain == 848 || contain == 857 || contain == 934))
                flag9 = true;

            if (chestTileType == 21 && (num9 == 13 || contain == 159 || contain == 65 || contain == 158 || contain == 2219)) {
                flag10 = true;
                if (WorldGen.remixWorldGen && !WorldGen.getGoodWorldGen) {
                    if (WorldGen.crimson) {
                        num9 = 43;
                    }
                    else {
                        chestTileType = 467;
                        num9 = 3;
                    }
                }
            }

            if (WorldGen.noTrapsWorldGen && num9 == 1 && chestTileType == 21 && (!WorldGen.remixWorldGen || genRand.Next(3) == 0)) {
                num9 = 4;
                chestTileType = 467;
            }

            num8 = ((chestTileType != 467) ? WorldGen.PlaceChest(i - 1, num7 - 1, chestTileType, notNearOtherChests, num9) : WorldGen.PlaceChest(i - 1, num7 - 1, chestTileType, notNearOtherChests, num9));
            if (num8 >= 0) {
                if (flag11) {
                    GenVars.hellChest++;
                    if (GenVars.hellChest >= GenVars.hellChestItem.Length)
                        GenVars.hellChest = 0;
                }

                Chest chest = Main.chest[num8];
                int num10 = 0;
                while (num10 == 0) {
                    bool flag13 = (double)num7 < Main.worldSurface + 25.0;
                    if (WorldGen.remixWorldGen)
                        flag13 = (double)num7 >= (Main.rockLayer + (double)((Main.maxTilesY - 350) * 2)) / 3.0;

                    if ((num9 == 0 && flag13) || flag9) {
                        if (contain > 0) {
                            chest.item[num10].SetDefaults(contain);
                            chest.item[num10].Prefix(-1);
                            num10++;
                            switch (contain) {
                                case 848:
                                    chest.item[num10].SetDefaults(866);
                                    num10++;
                                    break;
                                case 832:
                                    chest.item[num10].SetDefaults(933);
                                    num10++;
                                    if (genRand.Next(6) == 0) {
                                        int num11 = genRand.Next(2);
                                        switch (num11) {
                                            case 0:
                                                num11 = 4429;
                                                break;
                                            case 1:
                                                num11 = 4427;
                                                break;
                                        }

                                        chest.item[num10].SetDefaults(num11);
                                        num10++;
                                    }
                                    break;
                            }

                            if (Main.tenthAnniversaryWorld && flag9) {
                                chest.item[num10++].SetDefaults(848);
                                chest.item[num10++].SetDefaults(866);
                            }
                        }
                        else {
                            int num12 = genRand.Next(11);
                            int mushroomStaffRodType = ModContent.ItemType<MushroomStaff>();
                            if (!_mushroomStaffAdded) {
                                num12 = 10;
                                _mushroomStaffAdded = true;
                            }
                            if (num12 == 0) {
                                chest.item[num10].SetDefaults(280);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num12 == 1) {
                                chest.item[num10].SetDefaults(281);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num12 == 2) {
                                chest.item[num10].SetDefaults(284);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num12 == 3) {
                                chest.item[num10].SetDefaults(285);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num12 == 4) {
                                chest.item[num10].SetDefaults(953);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num12 == 5) {
                                chest.item[num10].SetDefaults(946);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num12 == 6) {
                                chest.item[num10].SetDefaults(3068);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num12 == 7) {
                                if (WorldGen.remixWorldGen) {
                                    chest.item[num10].SetDefaults(517);
                                    chest.item[num10].Prefix(-1);
                                }
                                else {
                                    chest.item[num10].SetDefaults(3069);
                                    chest.item[num10].Prefix(-1);
                                }
                            }

                            if (num12 == 8) {
                                chest.item[num10].SetDefaults(3084);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num12 == 9) {
                                chest.item[num10].SetDefaults(4341);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num12 == 10) {
                                chest.item[num10].SetDefaults(mushroomStaffRodType);
                                chest.item[num10].Prefix(-1);
                            }

                            num10++;
                        }

                        if (genRand.Next(6) == 0) {
                            int stack = genRand.Next(40, 76);
                            chest.item[num10].SetDefaults(282);
                            chest.item[num10].stack = stack;
                            num10++;
                        }

                        if (genRand.Next(6) == 0) {
                            int stack2 = genRand.Next(150, 301);
                            chest.item[num10].SetDefaults(279);
                            chest.item[num10].stack = stack2;
                            num10++;
                        }

                        if (genRand.Next(6) == 0) {
                            chest.item[num10].SetDefaults(3093);
                            chest.item[num10].stack = 1;
                            if (genRand.Next(5) == 0)
                                chest.item[num10].stack += genRand.Next(2);

                            if (genRand.Next(10) == 0)
                                chest.item[num10].stack += genRand.Next(3);

                            num10++;
                        }

                        if (genRand.Next(6) == 0) {
                            chest.item[num10].SetDefaults(4345);
                            chest.item[num10].stack = 1;
                            if (genRand.Next(5) == 0)
                                chest.item[num10].stack += genRand.Next(2);

                            if (genRand.Next(10) == 0)
                                chest.item[num10].stack += genRand.Next(3);

                            num10++;
                        }

                        if (genRand.Next(3) == 0) {
                            chest.item[num10].SetDefaults(168);
                            chest.item[num10].stack = genRand.Next(3, 6);
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int num13 = genRand.Next(2);
                            int stack3 = genRand.Next(8) + 3;
                            if (num13 == 0)
                                chest.item[num10].SetDefaults(GenVars.copperBar);

                            if (num13 == 1)
                                chest.item[num10].SetDefaults(GenVars.ironBar);

                            chest.item[num10].stack = stack3;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int stack4 = genRand.Next(50, 101);
                            chest.item[num10].SetDefaults(965);
                            chest.item[num10].stack = stack4;
                            num10++;
                        }

                        if (genRand.Next(3) != 0) {
                            int num14 = genRand.Next(2);
                            int stack5 = genRand.Next(26) + 25;
                            if (num14 == 0)
                                chest.item[num10].SetDefaults(40);

                            if (num14 == 1)
                                chest.item[num10].SetDefaults(42);

                            chest.item[num10].stack = stack5;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int stack6 = genRand.Next(3) + 3;
                            chest.item[num10].SetDefaults(28);
                            chest.item[num10].stack = stack6;
                            num10++;
                        }

                        if (genRand.Next(3) != 0) {
                            chest.item[num10].SetDefaults(2350);
                            chest.item[num10].stack = genRand.Next(3, 6);
                            num10++;
                        }

                        if (genRand.Next(3) > 0) {
                            int num15 = genRand.Next(6);
                            int stack7 = genRand.Next(1, 3);
                            if (num15 == 0)
                                chest.item[num10].SetDefaults(292);

                            if (num15 == 1)
                                chest.item[num10].SetDefaults(298);

                            if (num15 == 2)
                                chest.item[num10].SetDefaults(299);

                            if (num15 == 3)
                                chest.item[num10].SetDefaults(290);

                            if (num15 == 4)
                                chest.item[num10].SetDefaults(2322);

                            if (num15 == 5)
                                chest.item[num10].SetDefaults(2325);

                            chest.item[num10].stack = stack7;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int num16 = genRand.Next(2);
                            int stack8 = genRand.Next(11) + 10;
                            if (num16 == 0)
                                chest.item[num10].SetDefaults(8);

                            if (num16 == 1)
                                chest.item[num10].SetDefaults(31);

                            chest.item[num10].stack = stack8;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            chest.item[num10].SetDefaults(72);
                            chest.item[num10].stack = genRand.Next(10, 30);
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            chest.item[num10].SetDefaults(9);
                            chest.item[num10].stack = genRand.Next(50, 100);
                            num10++;
                        }
                    }
                    else if ((!WorldGen.remixWorldGen && (double)num7 < Main.rockLayer) || (WorldGen.remixWorldGen && (double)num7 > Main.rockLayer && num7 < Main.maxTilesY - 250)) {
                        if (contain > 0) {
                            if (contain == 832) {
                                chest.item[num10].SetDefaults(933);
                                num10++;
                            }

                            chest.item[num10].SetDefaults(contain);
                            chest.item[num10].Prefix(-1);
                            num10++;
                            if (flag4) {
                                if (genRand.Next(2) == 0)
                                    chest.item[num10++].SetDefaults(4425);

                                if (genRand.Next(2) == 0)
                                    chest.item[num10++].SetDefaults(4460);
                            }

                            if (flag10 && genRand.Next(40) == 0)
                                chest.item[num10++].SetDefaults(4978);

                            if (flag5 && genRand.Next(10) == 0) {
                                int num17 = genRand.Next(2);
                                switch (num17) {
                                    case 0:
                                        num17 = 4429;
                                        break;
                                    case 1:
                                        num17 = 4427;
                                        break;
                                }

                                chest.item[num10].SetDefaults(num17);
                                num10++;
                            }

                            if (flag8 && (!GenVars.generatedShadowKey || genRand.Next(3) == 0)) {
                                GenVars.generatedShadowKey = true;
                                chest.item[num10].SetDefaults(329);
                                num10++;
                            }
                        }
                        else {
                            switch (genRand.Next(6)) {
                                case 0:
                                    chest.item[num10].SetDefaults(49);
                                    chest.item[num10].Prefix(-1);
                                    break;
                                case 1:
                                    chest.item[num10].SetDefaults(50);
                                    chest.item[num10].Prefix(-1);
                                    break;
                                case 2:
                                    chest.item[num10].SetDefaults(53);
                                    chest.item[num10].Prefix(-1);
                                    break;
                                case 3:
                                    chest.item[num10].SetDefaults(54);
                                    chest.item[num10].Prefix(-1);
                                    break;
                                case 4:
                                    chest.item[num10].SetDefaults(5011);
                                    chest.item[num10].Prefix(-1);
                                    break;
                                default:
                                    chest.item[num10].SetDefaults(975);
                                    chest.item[num10].Prefix(-1);
                                    break;
                            }

                            num10++;
                            if (genRand.Next(20) == 0) {
                                chest.item[num10].SetDefaults(997);
                                chest.item[num10].Prefix(-1);
                                num10++;
                            }
                            else if (genRand.Next(20) == 0) {
                                chest.item[num10].SetDefaults(930);
                                chest.item[num10].Prefix(-1);
                                num10++;
                                chest.item[num10].SetDefaults(931);
                                chest.item[num10].stack = genRand.Next(26) + 25;
                                num10++;
                            }

                            if (flag6 && genRand.Next(2) == 0) {
                                chest.item[num10].SetDefaults(4450);
                                num10++;
                            }

                            if (flag6 && genRand.Next(3) == 0) {
                                chest.item[num10].SetDefaults(4779);
                                num10++;
                                chest.item[num10].SetDefaults(4780);
                                num10++;
                                chest.item[num10].SetDefaults(4781);
                                num10++;
                            }
                        }

                        if (flag2) {
                            if (genRand.Next(3) == 0) {
                                chest.item[num10].SetDefaults(4423);
                                chest.item[num10].stack = genRand.Next(10, 20);
                                num10++;
                            }
                        }
                        else if (genRand.Next(3) == 0) {
                            chest.item[num10].SetDefaults(166);
                            chest.item[num10].stack = genRand.Next(10, 20);
                            num10++;
                        }

                        if (genRand.Next(5) == 0) {
                            chest.item[num10].SetDefaults(52);
                            num10++;
                        }

                        if (genRand.Next(3) == 0) {
                            int stack9 = genRand.Next(50, 101);
                            chest.item[num10].SetDefaults(965);
                            chest.item[num10].stack = stack9;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int num18 = genRand.Next(2);
                            int stack10 = genRand.Next(10) + 5;
                            if (num18 == 0)
                                chest.item[num10].SetDefaults(GenVars.ironBar);

                            if (num18 == 1)
                                chest.item[num10].SetDefaults(GenVars.silverBar);

                            chest.item[num10].stack = stack10;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int num19 = genRand.Next(2);
                            int stack11 = genRand.Next(25) + 25;
                            if (num19 == 0)
                                chest.item[num10].SetDefaults(40);

                            if (num19 == 1)
                                chest.item[num10].SetDefaults(42);

                            chest.item[num10].stack = stack11;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int stack12 = genRand.Next(3) + 3;
                            chest.item[num10].SetDefaults(28);
                            chest.item[num10].stack = stack12;
                            num10++;
                        }

                        if (genRand.Next(3) > 0) {
                            int num20 = genRand.Next(9);
                            int stack13 = genRand.Next(1, 3);
                            if (num20 == 0)
                                chest.item[num10].SetDefaults(289);

                            if (num20 == 1)
                                chest.item[num10].SetDefaults(298);

                            if (num20 == 2)
                                chest.item[num10].SetDefaults(299);

                            if (num20 == 3)
                                chest.item[num10].SetDefaults(290);

                            if (num20 == 4)
                                chest.item[num10].SetDefaults(303);

                            if (num20 == 5)
                                chest.item[num10].SetDefaults(291);

                            if (num20 == 6)
                                chest.item[num10].SetDefaults(304);

                            if (num20 == 7)
                                chest.item[num10].SetDefaults(2322);

                            if (num20 == 8)
                                chest.item[num10].SetDefaults(2329);

                            chest.item[num10].stack = stack13;
                            num10++;
                        }

                        if (genRand.Next(3) != 0) {
                            int stack14 = genRand.Next(2, 5);
                            chest.item[num10].SetDefaults(2350);
                            chest.item[num10].stack = stack14;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int stack15 = genRand.Next(11) + 10;
                            if (num9 == 11)
                                chest.item[num10].SetDefaults(974);
                            else
                                chest.item[num10].SetDefaults(8);

                            chest.item[num10].stack = stack15;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            chest.item[num10].SetDefaults(72);
                            chest.item[num10].stack = genRand.Next(50, 90);
                            num10++;
                        }
                    }
                    else if (num7 < Main.maxTilesY - 250 || (WorldGen.remixWorldGen && (Style == 7 || Style == 14))) {
                        if (contain > 0) {
                            chest.item[num10].SetDefaults(contain);
                            chest.item[num10].Prefix(-1);
                            num10++;
                            if (flag && genRand.Next(5) == 0) {
                                chest.item[num10].SetDefaults(3199);
                                num10++;
                            }

                            if (flag2) {
                                if (genRand.Next(7) == 0) {
                                    chest.item[num10].SetDefaults(4346);
                                    num10++;
                                }

                                if (genRand.Next(15) == 0) {
                                    chest.item[num10].SetDefaults(4066);
                                    num10++;
                                }
                            }

                            if (flag3 && genRand.Next(6) == 0) {
                                chest.item[num10++].SetDefaults(3360);
                                chest.item[num10++].SetDefaults(3361);
                            }

                            if (flag3 && genRand.Next(10) == 0)
                                chest.item[num10++].SetDefaults(4426);

                            if (flag4) {
                                if (genRand.Next(2) == 0)
                                    chest.item[num10++].SetDefaults(4425);

                                if (genRand.Next(2) == 0)
                                    chest.item[num10++].SetDefaults(4460);
                            }

                            if (flag8 && (!GenVars.generatedShadowKey || genRand.Next(3) == 0)) {
                                GenVars.generatedShadowKey = true;
                                chest.item[num10].SetDefaults(329);
                                num10++;
                            }
                        }
                        else {
                            int num21 = genRand.Next(7);
                            bool flag14 = num7 > GenVars.lavaLine;
                            if (WorldGen.remixWorldGen)
                                flag14 = (double)num7 > Main.worldSurface && (double)num7 < Main.rockLayer;

                            int maxValue = 20;
                            if (WorldGen.tenthAnniversaryWorldGen)
                                maxValue = 15;

                            if (genRand.Next(maxValue) == 0 && flag14) {
                                chest.item[num10].SetDefaults(906);
                                chest.item[num10].Prefix(-1);
                            }
                            else if (genRand.Next(15) == 0) {
                                chest.item[num10].SetDefaults(997);
                                chest.item[num10].Prefix(-1);
                            }
                            else {
                                if (num21 == 0) {
                                    chest.item[num10].SetDefaults(49);
                                    chest.item[num10].Prefix(-1);
                                }

                                if (num21 == 1) {
                                    chest.item[num10].SetDefaults(50);
                                    chest.item[num10].Prefix(-1);
                                }

                                if (num21 == 2) {
                                    chest.item[num10].SetDefaults(53);
                                    chest.item[num10].Prefix(-1);
                                }

                                if (num21 == 3) {
                                    chest.item[num10].SetDefaults(54);
                                    chest.item[num10].Prefix(-1);
                                }

                                if (num21 == 4) {
                                    chest.item[num10].SetDefaults(5011);
                                    chest.item[num10].Prefix(-1);
                                }

                                if (num21 == 5) {
                                    chest.item[num10].SetDefaults(975);
                                    chest.item[num10].Prefix(-1);
                                }

                                if (num21 == 6) {
                                    chest.item[num10].SetDefaults(930);
                                    chest.item[num10].Prefix(-1);
                                    num10++;
                                    chest.item[num10].SetDefaults(931);
                                    chest.item[num10].stack = genRand.Next(26) + 25;
                                }
                            }

                            num10++;
                            if (flag6) {
                                if (genRand.Next(2) == 0) {
                                    chest.item[num10].SetDefaults(4450);
                                    num10++;
                                }
                                else {
                                    chest.item[num10].SetDefaults(4779);
                                    num10++;
                                    chest.item[num10].SetDefaults(4780);
                                    num10++;
                                    chest.item[num10].SetDefaults(4781);
                                    num10++;
                                }
                            }
                        }

                        if (genRand.Next(5) == 0) {
                            chest.item[num10].SetDefaults(43);
                            num10++;
                        }

                        if (genRand.Next(3) == 0) {
                            chest.item[num10].SetDefaults(167);
                            num10++;
                        }

                        if (genRand.Next(4) == 0) {
                            chest.item[num10].SetDefaults(51);
                            chest.item[num10].stack = genRand.Next(26) + 25;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int num22 = genRand.Next(2);
                            int stack16 = genRand.Next(8) + 3;
                            if (num22 == 0)
                                chest.item[num10].SetDefaults(GenVars.goldBar);

                            if (num22 == 1)
                                chest.item[num10].SetDefaults(GenVars.silverBar);

                            chest.item[num10].stack = stack16;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int num23 = genRand.Next(2);
                            int stack17 = genRand.Next(26) + 25;
                            if (num23 == 0)
                                chest.item[num10].SetDefaults(41);

                            if (num23 == 1)
                                chest.item[num10].SetDefaults(279);

                            chest.item[num10].stack = stack17;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int stack18 = genRand.Next(3) + 3;
                            chest.item[num10].SetDefaults(188);
                            chest.item[num10].stack = stack18;
                            num10++;
                        }

                        if (genRand.Next(3) > 0) {
                            int num24 = genRand.Next(6);
                            int stack19 = genRand.Next(1, 3);
                            if (num24 == 0)
                                chest.item[num10].SetDefaults(296);

                            if (num24 == 1)
                                chest.item[num10].SetDefaults(295);

                            if (num24 == 2)
                                chest.item[num10].SetDefaults(299);

                            if (num24 == 3)
                                chest.item[num10].SetDefaults(302);

                            if (num24 == 4)
                                chest.item[num10].SetDefaults(303);

                            if (num24 == 5)
                                chest.item[num10].SetDefaults(305);

                            chest.item[num10].stack = stack19;
                            num10++;
                        }

                        if (genRand.Next(3) > 1) {
                            int num25 = genRand.Next(6);
                            int stack20 = genRand.Next(1, 3);
                            if (num25 == 0)
                                chest.item[num10].SetDefaults(301);

                            if (num25 == 1)
                                chest.item[num10].SetDefaults(297);

                            if (num25 == 2)
                                chest.item[num10].SetDefaults(304);

                            if (num25 == 3)
                                chest.item[num10].SetDefaults(2329);

                            if (num25 == 4)
                                chest.item[num10].SetDefaults(2351);

                            if (num25 == 5)
                                chest.item[num10].SetDefaults(2326);

                            chest.item[num10].stack = stack20;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int stack21 = genRand.Next(2, 5);
                            chest.item[num10].SetDefaults(2350);
                            chest.item[num10].stack = stack21;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int num26 = genRand.Next(2);
                            int stack22 = genRand.Next(15) + 15;
                            if (num26 == 0) {
                                if (num9 == 11)
                                    chest.item[num10].SetDefaults(974);
                                else
                                    chest.item[num10].SetDefaults(8);
                            }

                            if (num26 == 1)
                                chest.item[num10].SetDefaults(282);

                            chest.item[num10].stack = stack22;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            chest.item[num10].SetDefaults(73);
                            chest.item[num10].stack = genRand.Next(1, 3);
                            num10++;
                        }
                    }
                    else {
                        if (contain > 0) {
                            chest.item[num10].SetDefaults(contain);
                            chest.item[num10].Prefix(-1);
                            num10++;
                            if (flag7 && genRand.Next(5) == 0) {
                                chest.item[num10].SetDefaults(5010);
                                num10++;
                            }

                            if (flag7 && genRand.Next(10) == 0) {
                                chest.item[num10].SetDefaults(4443);
                                num10++;
                            }

                            if (flag7 && genRand.Next(10) == 0) {
                                chest.item[num10].SetDefaults(4737);
                                num10++;
                            }

                            if (flag7 && genRand.Next(10) == 0) {
                                chest.item[num10].SetDefaults(4551);
                                num10++;
                            }
                        }
                        else {
                            int num27 = genRand.Next(4);
                            if (num27 == 0) {
                                chest.item[num10].SetDefaults(49);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num27 == 1) {
                                chest.item[num10].SetDefaults(50);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num27 == 2) {
                                chest.item[num10].SetDefaults(53);
                                chest.item[num10].Prefix(-1);
                            }

                            if (num27 == 3) {
                                chest.item[num10].SetDefaults(54);
                                chest.item[num10].Prefix(-1);
                            }

                            num10++;
                        }

                        if (genRand.Next(3) == 0) {
                            chest.item[num10].SetDefaults(167);
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int num28 = genRand.Next(2);
                            int stack23 = genRand.Next(15) + 15;
                            if (num28 == 0)
                                chest.item[num10].SetDefaults(117);

                            if (num28 == 1)
                                chest.item[num10].SetDefaults(GenVars.goldBar);

                            chest.item[num10].stack = stack23;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int num29 = genRand.Next(2);
                            int stack24 = genRand.Next(25) + 50;
                            if (num29 == 0)
                                chest.item[num10].SetDefaults(265);

                            if (num29 == 1) {
                                if (WorldGen.SavedOreTiers.Silver == 168)
                                    chest.item[num10].SetDefaults(4915);
                                else
                                    chest.item[num10].SetDefaults(278);
                            }

                            chest.item[num10].stack = stack24;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int stack25 = genRand.Next(6) + 15;
                            chest.item[num10].SetDefaults(227);
                            chest.item[num10].stack = stack25;
                            num10++;
                        }

                        if (genRand.Next(4) > 0) {
                            int num30 = genRand.Next(8);
                            int stack26 = genRand.Next(1, 3);
                            if (num30 == 0)
                                chest.item[num10].SetDefaults(296);

                            if (num30 == 1)
                                chest.item[num10].SetDefaults(295);

                            if (num30 == 2)
                                chest.item[num10].SetDefaults(293);

                            if (num30 == 3)
                                chest.item[num10].SetDefaults(288);

                            if (num30 == 4)
                                chest.item[num10].SetDefaults(294);

                            if (num30 == 5)
                                chest.item[num10].SetDefaults(297);

                            if (num30 == 6)
                                chest.item[num10].SetDefaults(304);

                            if (num30 == 7)
                                chest.item[num10].SetDefaults(2323);

                            chest.item[num10].stack = stack26;
                            num10++;
                        }

                        if (genRand.Next(3) > 0) {
                            int num31 = genRand.Next(8);
                            int stack27 = genRand.Next(1, 3);
                            if (num31 == 0)
                                chest.item[num10].SetDefaults(305);

                            if (num31 == 1)
                                chest.item[num10].SetDefaults(301);

                            if (num31 == 2)
                                chest.item[num10].SetDefaults(302);

                            if (num31 == 3)
                                chest.item[num10].SetDefaults(288);

                            if (num31 == 4)
                                chest.item[num10].SetDefaults(300);

                            if (num31 == 5)
                                chest.item[num10].SetDefaults(2351);

                            if (num31 == 6)
                                chest.item[num10].SetDefaults(2348);

                            if (num31 == 7)
                                chest.item[num10].SetDefaults(2345);

                            chest.item[num10].stack = stack27;
                            num10++;
                        }

                        if (genRand.Next(3) == 0) {
                            int stack28 = genRand.Next(1, 3);
                            if (genRand.Next(2) == 0)
                                chest.item[num10].SetDefaults(2350);
                            else
                                chest.item[num10].SetDefaults(4870);

                            chest.item[num10].stack = stack28;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            int num32 = genRand.Next(2);
                            int stack29 = genRand.Next(15) + 15;
                            if (num32 == 0)
                                chest.item[num10].SetDefaults(8);

                            if (num32 == 1)
                                chest.item[num10].SetDefaults(282);

                            chest.item[num10].stack = stack29;
                            num10++;
                        }

                        if (genRand.Next(2) == 0) {
                            chest.item[num10].SetDefaults(73);
                            chest.item[num10].stack = genRand.Next(2, 5);
                            num10++;
                        }
                    }

                    if (num10 > 0 && chestTileType == 21) {
                        if (num9 == 10 && genRand.Next(4) == 0) {
                            chest.item[num10].SetDefaults(2204);
                            num10++;
                        }

                        if (num9 == 11 && genRand.Next(7) == 0) {
                            chest.item[num10].SetDefaults(2198);
                            num10++;
                        }

                        if (flag10 && genRand.Next(3) == 0) {
                            chest.item[num10].SetDefaults(2197);
                            num10++;
                        }

                        if (flag10) {
                            int num33 = genRand.Next(6);
                            if (num33 == 0)
                                chest.item[num10].SetDefaults(5258);

                            if (num33 == 1)
                                chest.item[num10].SetDefaults(5226);

                            if (num33 == 2)
                                chest.item[num10].SetDefaults(5254);

                            if (num33 == 3)
                                chest.item[num10].SetDefaults(5238);

                            if (num33 == 4)
                                chest.item[num10].SetDefaults(5255);

                            if (num33 == 5)
                                chest.item[num10].SetDefaults(5388);

                            num10++;
                        }

                        if (flag10) {
                            chest.item[num10].SetDefaults(751);
                            chest.item[num10].stack = genRand.Next(50, 101);
                            num10++;
                        }

                        if (num9 == 16) {
                            chest.item[num10].SetDefaults(2195);
                            num10++;
                        }

                        if (Main.wallDungeon[Main.tile[i, k].WallType] && genRand.Next(8) == 0) {
                            chest.item[num10].SetDefaults(2192);
                            num10++;
                        }

                        if ((num9 == 23 || num9 == 24 || num9 == 25 || num9 == 26 || num9 == 27) && genRand.Next(2) == 0) {
                            chest.item[num10].SetDefaults(5234);
                            num10++;
                        }

                        if (num9 == 16) {
                            if (genRand.Next(5) == 0) {
                                chest.item[num10].SetDefaults(2767);
                                num10++;
                            }
                            else {
                                chest.item[num10].SetDefaults(2766);
                                chest.item[num10].stack = genRand.Next(3, 8);
                                num10++;
                            }
                        }
                    }

                    if (num10 <= 0 || chestTileType != 467)
                        continue;

                    if (flag10 && genRand.Next(3) == 0) {
                        chest.item[num10].SetDefaults(2197);
                        num10++;
                    }

                    if (flag10) {
                        int num34 = genRand.Next(5);
                        if (num34 == 0)
                            chest.item[num10].SetDefaults(5258);

                        if (num34 == 1)
                            chest.item[num10].SetDefaults(5226);

                        if (num34 == 2)
                            chest.item[num10].SetDefaults(5254);

                        if (num34 == 3)
                            chest.item[num10].SetDefaults(5238);

                        if (num34 == 4)
                            chest.item[num10].SetDefaults(5255);

                        num10++;
                    }

                    if (flag10) {
                        chest.item[num10].SetDefaults(751);
                        chest.item[num10].stack = genRand.Next(50, 101);
                        num10++;
                    }

                    if (num9 == 13 && genRand.Next(2) == 0) {
                        chest.item[num10].SetDefaults(5234);
                        num10++;
                    }
                }

                return true;
            }

            if (trySlope) {
                if (num2 > -1) {
                    Tile tile = Main.tile[i - 1, k];
                    tile.Slope = (SlopeType)(byte)num2;
                }

                if (num3 > -1) {
                    Tile tile = Main.tile[i, k];
                    tile.Slope = (SlopeType)(byte)num3;
                }
            }

            return false;
        }

        return false;
    }

    public void Unload() { }
}
