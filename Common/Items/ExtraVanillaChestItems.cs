using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Items.Potions;
using RoA.Content.Items.Weapons.Nature.PreHardmode;
using RoA.Content.Items.Weapons.Nature.PreHardmode.Canes;
using RoA.Content.Items.Weapons.Nature.PreHardmode.Claws;
using RoA.Content.Tiles.Furniture;

using System;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Common.Items;

sealed class ExtraVanillaChestItems : ModSystem {
    private static bool _cactiCactusCaneAdded;
    private static bool _iceCaneAdded;
    private static bool _mushroomStaffAdded;
    private static bool _hellfireClawsAdded;
    internal static bool _giantTreeSaplingAdded;
    private static bool _feathersBottleAdded;
    private static bool _oniMaskAdded;

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "MakeDungeon_Lights")]
    public extern static void WorldGen_MakeDungeon_Lights(WorldGen worldGen, ushort tileType, ref int failCount, int failMax, ref int numAdd, int[] roomWall);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "MakeDungeon_Traps")]
    public extern static void WorldGen_MakeDungeon_Traps(WorldGen worldGen, ref int failCount, int failMax, ref int numAdd);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "MakeDungeon_GroundFurniture")]
    public extern static double WorldGen_MakeDungeon_GroundFurniture(WorldGen worldGen, int wallType);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "MakeDungeon_Pictures")]
    public extern static double WorldGen_MakeDungeon_Pictures(WorldGen worldGen, int[] roomWall, double count);

    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "MakeDungeon_Banners")]
    public extern static double WorldGen_MakeDungeon_Banners(WorldGen worldGen, int[] roomWall, double count);

    public override void Load() {
        On_WorldGen.AddBuriedChest_int_int_int_bool_int_bool_ushort += On_WorldGen_AddBuriedChest_int_int_int_bool_int_bool_ushort;
        On_WorldGen.MakeDungeon += On_WorldGen_MakeDungeon;
        On_WorldGen.GrowLivingTreePassageRoom += On_WorldGen_GrowLivingTreePassageRoom;
        //On_WorldGen.IslandHouse += On_WorldGen_IslandHouse;
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
        //if (Main.tile[num8, Y].TileType == 304) {
        //    _loomPlacedInWorld = true;
        //}
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
        if (genRand.Next(5) == 0) {
            contain = ModContent.ItemType<GiantTreeSapling>();
        }
        if (!summonStaffAdded && !_giantTreeSaplingAdded) {
            _giantTreeSaplingAdded = true;
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

    //private void On_WorldGen_IslandHouse(On_WorldGen.orig_IslandHouse orig, int i, int j, int islandStyle) {
    //    bool flag = WorldGen.crimson;
    //    if (WorldGen.drunkWorldGen)
    //        flag = GenVars.crimsonLeft && i < Main.maxTilesX / 2 || (!GenVars.crimsonLeft && i > Main.maxTilesX / 2 ? true : false);

    //    byte type = 202;
    //    byte wall = 82;
    //    Vector2D vector2D = new Vector2D(i, j);
    //    int num = 1;
    //    if (WorldGen.genRand.Next(2) == 0)
    //        num = -1;

    //    int num2 = WorldGen.genRand.Next(7, 12);
    //    int num3 = WorldGen.genRand.Next(5, 7);
    //    vector2D.X = i + (num2 + 2) * num;
    //    for (int k = j - 15; k < j + 30; k++) {
    //        if (Main.tile[(int)vector2D.X, k].HasTile) {
    //            vector2D.Y = k - 1;
    //            break;
    //        }
    //    }

    //    vector2D.X = i;
    //    int num4 = (int)(vector2D.X - num2 - 1.0);
    //    int num5 = (int)(vector2D.X + num2 + 1.0);
    //    int num6 = (int)(vector2D.Y - num3 - 1.0);
    //    int num7 = (int)(vector2D.Y + 2.0);
    //    if (num4 < 0)
    //        num4 = 0;

    //    if (num5 > Main.maxTilesX)
    //        num5 = Main.maxTilesX;

    //    if (num6 < 0)
    //        num6 = 0;

    //    if (num7 > Main.maxTilesY)
    //        num7 = Main.maxTilesY;

    //    for (int l = num4; l <= num5; l++) {
    //        for (int m = num6 - 1; m < num7 + 1; m++) {
    //            if (m != num6 - 1 || l != num4 && l != num5) {
    //                Tile tile = Main.tile[l, m];
    //                tile.HasTile = true;
    //                tile.LiquidAmount = 0;
    //                tile.TileType = type;
    //                tile.WallType = 0;
    //                tile.IsHalfBlock = false;
    //                tile.Slope = 0;
    //            }
    //        }
    //    }

    //    num4 = (int)(vector2D.X - num2);
    //    num5 = (int)(vector2D.X + num2);
    //    num6 = (int)(vector2D.Y - num3);
    //    num7 = (int)(vector2D.Y + 1.0);
    //    if (num4 < 0)
    //        num4 = 0;

    //    if (num5 > Main.maxTilesX)
    //        num5 = Main.maxTilesX;

    //    if (num6 < 0)
    //        num6 = 0;

    //    if (num7 > Main.maxTilesY)
    //        num7 = Main.maxTilesY;

    //    for (int n = num4; n <= num5; n++) {
    //        for (int num8 = num6; num8 < num7; num8++) {
    //            if ((num8 != num6 || n != num4 && n != num5) && Main.tile[n, num8].WallType == 0) {
    //                Tile tile = Main.tile[n, num8];
    //                tile.HasTile = false;
    //                Main.tile[n, num8].WallType = wall;
    //            }
    //        }
    //    }

    //    int num9 = i + (num2 + 1) * num;
    //    int num10 = (int)vector2D.Y;
    //    for (int num11 = num9 - 2; num11 <= num9 + 2; num11++) {
    //        Tile tile = Main.tile[num11, num10];
    //        tile.HasTile = false;
    //        tile = Main.tile[num11, num10 - 1];
    //        tile.HasTile = false;
    //        tile = Main.tile[num11, num10 - 2];
    //        tile.HasTile = false;
    //    }

    //    if (WorldGen.remixWorldGen) {
    //        if (flag)
    //            WorldGen.PlaceTile(num9, num10, 10, mute: true, forced: false, -1, 5);
    //        else
    //            WorldGen.PlaceTile(num9, num10, 10, mute: true, forced: false, -1, 38);
    //    }
    //    else {
    //        WorldGen.PlaceTile(num9, num10, 10, mute: true, forced: false, -1, 9);
    //    }

    //    num9 = i + (num2 + 1) * -num - num;
    //    for (int num12 = num6; num12 <= num7 + 1; num12++) {
    //        Tile tile = Main.tile[num9, num12];
    //        tile.HasTile = true;
    //        tile.LiquidAmount = 0;
    //        tile.TileType = type;
    //        tile.WallType = 0;
    //        tile.IsHalfBlock = false;
    //        tile.Slope = 0;
    //    }

    //    int contain = 0;
    //    int num13 = GenVars.skyIslandHouseCount;
    //    if (num13 > 3) {
    //        num13 = WorldGen.genRand.Next(4/*5*/);
    //    }

    //    //if (!_feathersBottleAdded) {
    //    //    num13 = 4;
    //    //    _feathersBottleAdded = true;
    //    //}

    //    switch (num13) {
    //        case 0:
    //            contain = 159;
    //            break;
    //        case 1:
    //            contain = 65;
    //            break;
    //        case 2:
    //            contain = 158;
    //            break;
    //        case 3:
    //            contain = 2219;
    //            break;
    //            //case 4:
    //            //    contain = ModContent.ItemType<FeathersInABottle>();
    //            //    break;
    //    }

    //    if (WorldGen.getGoodWorldGen)
    //        WorldGen.AddBuriedChest(i, num10 - 3, contain, notNearOtherChests: false, 2, trySlope: false, 0);
    //    else
    //        WorldGen.AddBuriedChest(i, num10 - 3, contain, notNearOtherChests: false, 13, trySlope: false, 0);

    //    if (islandStyle > 0) {
    //        for (int num14 = 0; num14 < 100000; num14++) {
    //            int num15 = i + WorldGen.genRand.Next(-50, 51);
    //            int num16 = num10 + WorldGen.genRand.Next(21);
    //            if ((num14 >= 50000 || Main.tile[num15, num16].TileType != 202) && !Main.tile[num15, num16].HasTile) {
    //                WorldGen.Place2xX(num15, num16, 207, islandStyle);
    //                if (Main.tile[num15, num16].HasTile) {
    //                    WorldGen.SwitchFountain(num15, num16);
    //                    break;
    //                }
    //            }
    //        }
    //    }

    //    GenVars.skyIslandHouseCount++;
    //    if (!WorldGen.remixWorldGen) {
    //        int num17 = i - num2 / 2 + 1;
    //        int num18 = i + num2 / 2 - 1;
    //        int num19 = 1;
    //        if (num2 > 10)
    //            num19 = 2;

    //        int num20 = (num6 + num7) / 2 - 1;
    //        for (int num21 = num17 - num19; num21 <= num17 + num19; num21++) {
    //            for (int num22 = num20 - 1; num22 <= num20 + 1; num22++) {
    //                Main.tile[num21, num22].WallType = 21;
    //            }
    //        }

    //        for (int num23 = num18 - num19; num23 <= num18 + num19; num23++) {
    //            for (int num24 = num20 - 1; num24 <= num20 + 1; num24++) {
    //                Main.tile[num23, num24].WallType = 21;
    //            }
    //        }
    //    }

    //    int num25 = i + (num2 / 2 + 1) * -num;
    //    if (WorldGen.remixWorldGen) {
    //        if (flag) {
    //            WorldGen.PlaceTile(num25, num7 - 1, 14, mute: true, forced: false, -1, 5);
    //            WorldGen.PlaceTile(num25 - 2, num7 - 1, 15, mute: true, forced: false, 0, 8);
    //            WorldGen.PlaceTile(num25 + 2, num7 - 1, 15, mute: true, forced: false, 0, 8);
    //        }
    //        else {
    //            WorldGen.PlaceTile(num25, num7 - 1, 469, mute: true, forced: false, -1, 2);
    //            WorldGen.PlaceTile(num25 - 2, num7 - 1, 15, mute: true, forced: false, 0, 38);
    //            WorldGen.PlaceTile(num25 + 2, num7 - 1, 15, mute: true, forced: false, 0, 38);
    //        }
    //    }
    //    else {
    //        WorldGen.PlaceTile(num25, num7 - 1, 14, mute: true, forced: false, -1, 7);
    //        WorldGen.PlaceTile(num25 - 2, num7 - 1, 15, mute: true, forced: false, 0, 10);
    //        WorldGen.PlaceTile(num25 + 2, num7 - 1, 15, mute: true, forced: false, 0, 10);
    //    }

    //    Main.tile[num25 - 2, num7 - 1].TileFrameX += 18;
    //    Main.tile[num25 - 2, num7 - 2].TileFrameX += 18;
    //    if (!WorldGen.remixWorldGen) {
    //        int i2 = num4 + 1;
    //        int j2 = num6;
    //        WorldGen.PlaceTile(i2, j2, 91, mute: true, forced: false, -1, WorldGen.genRand.Next(7, 10));
    //        i2 = num5 - 1;
    //        j2 = num6;
    //        WorldGen.PlaceTile(i2, j2, 91, mute: true, forced: false, -1, WorldGen.genRand.Next(7, 10));
    //        if (num > 0) {
    //            i2 = num4;
    //            j2 = num6 + 1;
    //        }
    //        else {
    //            i2 = num5;
    //            j2 = num6 + 1;
    //        }

    //        WorldGen.PlaceTile(i2, j2, 91, mute: true, forced: false, -1, WorldGen.genRand.Next(7, 10));
    //    }

    //    if (islandStyle != 1)
    //        return;

    //    int num26 = WorldGen.genRand.Next(3, 6);
    //    for (int num27 = 0; num27 < 100000; num27++) {
    //        int num28 = i + WorldGen.genRand.Next(-50, 51);
    //        int num29 = num10 + WorldGen.genRand.Next(-10, 21);
    //        if (!Main.tile[num28, num29].HasTile) {
    //            WorldGen.GrowPalmTree(num28, num29 + 1);
    //            if (Main.tile[num28, num29].HasTile)
    //                num26--;
    //        }

    //        if (num26 <= 0)
    //            break;
    //    }
    //}

    private void On_WorldGen_MakeDungeon(On_WorldGen.orig_MakeDungeon orig, int x, int y) {
        GenVars.dEnteranceX = 0;
        GenVars.numDRooms = 0;
        GenVars.numDDoors = 0;
        GenVars.numDungeonPlatforms = 0;
        UnifiedRandom genRand = WorldGen.genRand;
        int num = genRand.Next(3);
        genRand.Next(3);
        if (WorldGen.remixWorldGen)
            num = WorldGen.crimson ? 2 : 0;

        ushort num2;
        int num3;
        switch (num) {
            case 0:
                num2 = 41;
                num3 = 7;
                GenVars.crackedType = 481;
                break;
            case 1:
                num2 = 43;
                num3 = 8;
                GenVars.crackedType = 482;
                break;
            default:
                num2 = 44;
                num3 = 9;
                GenVars.crackedType = 483;
                break;
        }

        Main.tileSolid[GenVars.crackedType] = false;
        GenVars.dungeonLake = true;
        GenVars.numDDoors = 0;
        GenVars.numDungeonPlatforms = 0;
        GenVars.numDRooms = 0;
        GenVars.dungeonX = x;
        GenVars.dungeonY = y;
        GenVars.dMinX = x;
        GenVars.dMaxX = x;
        GenVars.dMinY = y;
        GenVars.dMaxY = y;
        GenVars.dxStrength1 = genRand.Next(25, 30);
        GenVars.dyStrength1 = genRand.Next(20, 25);
        GenVars.dxStrength2 = genRand.Next(35, 50);
        GenVars.dyStrength2 = genRand.Next(10, 15);
        double num4 = Main.maxTilesX / 60;
        num4 += genRand.Next(0, (int)(num4 / 3.0));
        double num5 = num4;
        int num6 = 5;
        WorldGen.DungeonRoom(GenVars.dungeonX, GenVars.dungeonY, num2, num3);
        while (num4 > 0.0) {
            if (GenVars.dungeonX < GenVars.dMinX)
                GenVars.dMinX = GenVars.dungeonX;

            if (GenVars.dungeonX > GenVars.dMaxX)
                GenVars.dMaxX = GenVars.dungeonX;

            if (GenVars.dungeonY > GenVars.dMaxY)
                GenVars.dMaxY = GenVars.dungeonY;

            num4 -= 1.0;
            Main.statusText = Lang.gen[58].Value + " " + (int)((num5 - num4) / num5 * 60.0) + "%";
            if (num6 > 0)
                num6--;

            if (num6 == 0 & genRand.Next(3) == 0) {
                num6 = 5;
                if (genRand.Next(2) == 0) {
                    int dungeonX = GenVars.dungeonX;
                    int dungeonY = GenVars.dungeonY;
                    WorldGen.DungeonHalls(GenVars.dungeonX, GenVars.dungeonY, num2, num3);
                    if (genRand.Next(2) == 0)
                        WorldGen.DungeonHalls(GenVars.dungeonX, GenVars.dungeonY, num2, num3);

                    WorldGen.DungeonRoom(GenVars.dungeonX, GenVars.dungeonY, num2, num3);
                    GenVars.dungeonX = dungeonX;
                    GenVars.dungeonY = dungeonY;
                }
                else {
                    WorldGen.DungeonRoom(GenVars.dungeonX, GenVars.dungeonY, num2, num3);
                }
            }
            else {
                WorldGen.DungeonHalls(GenVars.dungeonX, GenVars.dungeonY, num2, num3);
            }
        }

        WorldGen.DungeonRoom(GenVars.dungeonX, GenVars.dungeonY, num2, num3);
        int num7 = GenVars.dRoomX[0];
        int num8 = GenVars.dRoomY[0];
        for (int i = 0; i < GenVars.numDRooms; i++) {
            if (GenVars.dRoomY[i] < num8) {
                num7 = GenVars.dRoomX[i];
                num8 = GenVars.dRoomY[i];
            }
        }

        GenVars.dungeonX = num7;
        GenVars.dungeonY = num8;
        GenVars.dEnteranceX = num7;
        GenVars.dSurface = false;
        num6 = 5;
        if (WorldGen.drunkWorldGen)
            GenVars.dSurface = true;

        while (!GenVars.dSurface) {
            if (num6 > 0)
                num6--;

            if (num6 == 0 && genRand.Next(5) == 0 && GenVars.dungeonY > Main.worldSurface + 100.0) {
                num6 = 10;
                int dungeonX2 = GenVars.dungeonX;
                int dungeonY2 = GenVars.dungeonY;
                WorldGen.DungeonHalls(GenVars.dungeonX, GenVars.dungeonY, num2, num3, forceX: true);
                WorldGen.DungeonRoom(GenVars.dungeonX, GenVars.dungeonY, num2, num3);
                GenVars.dungeonX = dungeonX2;
                GenVars.dungeonY = dungeonY2;
            }

            WorldGen.DungeonStairs(GenVars.dungeonX, GenVars.dungeonY, num2, num3);
        }

        WorldGen.DungeonEnt(GenVars.dungeonX, GenVars.dungeonY, num2, num3);
        Main.statusText = Lang.gen[58].Value + " 65%";
        int num9 = Main.maxTilesX * 2;
        int num10;
        for (num10 = 0; num10 < num9; num10++) {
            int i2 = genRand.Next(GenVars.dMinX, GenVars.dMaxX);
            int num11 = GenVars.dMinY;
            if (num11 < Main.worldSurface)
                num11 = (int)Main.worldSurface;

            int j = genRand.Next(num11, GenVars.dMaxY);
            num10 = !WorldGen.DungeonPitTrap(i2, j, num2, num3) ? num10 + 1 : num10 + 1500;
        }

        for (int k = 0; k < GenVars.numDRooms; k++) {
            for (int l = GenVars.dRoomL[k]; l <= GenVars.dRoomR[k]; l++) {
                if (!Main.tile[l, GenVars.dRoomT[k] - 1].HasTile) {
                    GenVars.dungeonPlatformX[GenVars.numDungeonPlatforms] = l;
                    GenVars.dungeonPlatformY[GenVars.numDungeonPlatforms] = GenVars.dRoomT[k] - 1;
                    GenVars.numDungeonPlatforms++;
                    break;
                }
            }

            for (int m = GenVars.dRoomL[k]; m <= GenVars.dRoomR[k]; m++) {
                if (!Main.tile[m, GenVars.dRoomB[k] + 1].HasTile) {
                    GenVars.dungeonPlatformX[GenVars.numDungeonPlatforms] = m;
                    GenVars.dungeonPlatformY[GenVars.numDungeonPlatforms] = GenVars.dRoomB[k] + 1;
                    GenVars.numDungeonPlatforms++;
                    break;
                }
            }

            for (int n = GenVars.dRoomT[k]; n <= GenVars.dRoomB[k]; n++) {
                if (!Main.tile[GenVars.dRoomL[k] - 1, n].HasTile) {
                    GenVars.DDoorX[GenVars.numDDoors] = GenVars.dRoomL[k] - 1;
                    GenVars.DDoorY[GenVars.numDDoors] = n;
                    GenVars.DDoorPos[GenVars.numDDoors] = -1;
                    GenVars.numDDoors++;
                    break;
                }
            }

            for (int num12 = GenVars.dRoomT[k]; num12 <= GenVars.dRoomB[k]; num12++) {
                if (!Main.tile[GenVars.dRoomR[k] + 1, num12].HasTile) {
                    GenVars.DDoorX[GenVars.numDDoors] = GenVars.dRoomR[k] + 1;
                    GenVars.DDoorY[GenVars.numDDoors] = num12;
                    GenVars.DDoorPos[GenVars.numDDoors] = 1;
                    GenVars.numDDoors++;
                    break;
                }
            }
        }

        Main.statusText = Lang.gen[58].Value + " 70%";
        int num13 = 0;
        int num14 = 1000;
        int num15 = 0;
        int num16 = Main.maxTilesX / 100;
        if (WorldGen.getGoodWorldGen)
            num16 *= 3;

        while (num15 < num16) {
            num13++;
            int num17 = genRand.Next(GenVars.dMinX, GenVars.dMaxX);
            int num18 = genRand.Next((int)Main.worldSurface + 25, GenVars.dMaxY);
            if (WorldGen.drunkWorldGen)
                num18 = genRand.Next(GenVars.dungeonY + 25, GenVars.dMaxY);

            int num19 = num17;
            if (Main.tile[num17, num18].WallType == num3 && !Main.tile[num17, num18].HasTile) {
                int num20 = 1;
                if (genRand.Next(2) == 0)
                    num20 = -1;

                for (; !Main.tile[num17, num18].HasTile; num18 += num20) {
                }

                if (Main.tile[num17 - 1, num18].HasTile && Main.tile[num17 + 1, num18].HasTile && Main.tile[num17 - 1, num18].TileType != GenVars.crackedType && !Main.tile[num17 - 1, num18 - num20].HasTile && !Main.tile[num17 + 1, num18 - num20].HasTile) {
                    num15++;
                    int num21 = genRand.Next(5, 13);
                    while (Main.tile[num17 - 1, num18].HasTile && Main.tile[num17 - 1, num18].TileType != GenVars.crackedType && Main.tile[num17, num18 + num20].HasTile && Main.tile[num17, num18].HasTile && !Main.tile[num17, num18 - num20].HasTile && num21 > 0) {
                        Main.tile[num17, num18].TileType = 48;
                        if (!Main.tile[num17 - 1, num18 - num20].HasTile && !Main.tile[num17 + 1, num18 - num20].HasTile) {
                            Main.tile[num17, num18 - num20].Clear(TileDataType.Slope);
                            Main.tile[num17, num18 - num20].TileType = 48;
                            Tile tile3 = Main.tile[num17, num18 - num20];
                            tile3.HasTile = true;
                            Main.tile[num17, num18 - num20 * 2].Clear(TileDataType.Slope);
                            Main.tile[num17, num18 - num20 * 2].TileType = 48;
                            tile3 = Main.tile[num17, num18 - num20 * 2];
                            tile3.HasTile = true;
                        }

                        num17--;
                        num21--;
                    }

                    num21 = genRand.Next(5, 13);
                    num17 = num19 + 1;
                    while (Main.tile[num17 + 1, num18].HasTile && Main.tile[num17 + 1, num18].TileType != GenVars.crackedType && Main.tile[num17, num18 + num20].HasTile && Main.tile[num17, num18].HasTile && !Main.tile[num17, num18 - num20].HasTile && num21 > 0) {
                        Main.tile[num17, num18].TileType = 48;
                        if (!Main.tile[num17 - 1, num18 - num20].HasTile && !Main.tile[num17 + 1, num18 - num20].HasTile) {
                            Main.tile[num17, num18 - num20].Clear(TileDataType.Slope);
                            Main.tile[num17, num18 - num20].TileType = 48;
                            Tile tile3 = Main.tile[num17, num18 - num20];
                            tile3.HasTile = true;
                            Main.tile[num17, num18 - num20 * 2].Clear(TileDataType.Slope);
                            Main.tile[num17, num18 - num20 * 2].TileType = 48;
                            tile3 = Main.tile[num17, num18 - num20 * 2];
                            tile3.HasTile = true;
                        }

                        num17++;
                        num21--;
                    }
                }
            }

            if (num13 > num14) {
                num13 = 0;
                num15++;
            }
        }

        num13 = 0;
        num14 = 1000;
        num15 = 0;
        Main.statusText = Lang.gen[58].Value + " 75%";
        while (num15 < num16) {
            num13++;
            int num22 = genRand.Next(GenVars.dMinX, GenVars.dMaxX);
            int num23 = genRand.Next((int)Main.worldSurface + 25, GenVars.dMaxY);
            int num24 = num23;
            if (Main.tile[num22, num23].WallType == num3 && !Main.tile[num22, num23].HasTile) {
                int num25 = 1;
                if (genRand.Next(2) == 0)
                    num25 = -1;

                for (; num22 > 5 && num22 < Main.maxTilesX - 5 && !Main.tile[num22, num23].HasTile; num22 += num25) {
                }

                if (Main.tile[num22, num23 - 1].HasTile && Main.tile[num22, num23 + 1].HasTile && Main.tile[num22, num23 - 1].TileType != GenVars.crackedType && !Main.tile[num22 - num25, num23 - 1].HasTile && !Main.tile[num22 - num25, num23 + 1].HasTile) {
                    num15++;
                    int num26 = genRand.Next(5, 13);
                    while (Main.tile[num22, num23 - 1].HasTile && Main.tile[num22, num23 - 1].TileType != GenVars.crackedType && Main.tile[num22 + num25, num23].HasTile && Main.tile[num22, num23].HasTile && !Main.tile[num22 - num25, num23].HasTile && num26 > 0) {
                        Main.tile[num22, num23].TileType = 48;
                        if (!Main.tile[num22 - num25, num23 - 1].HasTile && !Main.tile[num22 - num25, num23 + 1].HasTile) {
                            Main.tile[num22 - num25, num23].TileType = 48;
                            Tile tile3 = Main.tile[num22 - num25, num23];
                            tile3.HasTile = true;
                            Main.tile[num22 - num25, num23].Clear(TileDataType.Slope);
                            Main.tile[num22 - num25 * 2, num23].TileType = 48;
                            tile3 = Main.tile[num22 - num25 * 2, num23];
                            tile3.HasTile = true;
                            Main.tile[num22 - num25 * 2, num23].Clear(TileDataType.Slope);
                        }

                        num23--;
                        num26--;
                    }

                    num26 = genRand.Next(5, 13);
                    num23 = num24 + 1;
                    while (Main.tile[num22, num23 + 1].HasTile && Main.tile[num22, num23 + 1].TileType != GenVars.crackedType && Main.tile[num22 + num25, num23].HasTile && Main.tile[num22, num23].HasTile && !Main.tile[num22 - num25, num23].HasTile && num26 > 0) {
                        Main.tile[num22, num23].TileType = 48;
                        if (!Main.tile[num22 - num25, num23 - 1].HasTile && !Main.tile[num22 - num25, num23 + 1].HasTile) {
                            Main.tile[num22 - num25, num23].TileType = 48;
                            Tile tile3 = Main.tile[num22 - num25, num23];
                            tile3.HasTile = true;
                            Main.tile[num22 - num25, num23].Clear(TileDataType.Slope);
                            Main.tile[num22 - num25 * 2, num23].TileType = 48;
                            tile3 = Main.tile[num22 - num25 * 2, num23];
                            tile3.HasTile = true;
                            Main.tile[num22 - num25 * 2, num23].Clear(TileDataType.Slope);
                        }

                        num23++;
                        num26--;
                    }
                }
            }

            if (num13 > num14) {
                num13 = 0;
                num15++;
            }
        }

        Main.statusText = Lang.gen[58].Value + " 80%";
        for (int num27 = 0; num27 < GenVars.numDDoors; num27++) {
            int num28 = GenVars.DDoorX[num27] - 10;
            int num29 = GenVars.DDoorX[num27] + 10;
            int num30 = 100;
            int num31 = 0;
            int num32 = 0;
            int num33 = 0;
            for (int num34 = num28; num34 < num29; num34++) {
                bool flag = true;
                int num35 = GenVars.DDoorY[num27];
                while (num35 > 10 && !Main.tile[num34, num35].HasTile) {
                    num35--;
                }

                if (!Main.tileDungeon[Main.tile[num34, num35].TileType])
                    flag = false;

                num32 = num35;
                for (num35 = GenVars.DDoorY[num27]; !Main.tile[num34, num35].HasTile; num35++) {
                }

                if (!Main.tileDungeon[Main.tile[num34, num35].TileType])
                    flag = false;

                num33 = num35;
                if (num33 - num32 < 3)
                    continue;

                int num36 = num34 - 20;
                int num37 = num34 + 20;
                int num38 = num33 - 10;
                int num39 = num33 + 10;
                for (int num40 = num36; num40 < num37; num40++) {
                    for (int num41 = num38; num41 < num39; num41++) {
                        if (Main.tile[num40, num41].HasTile && Main.tile[num40, num41].TileType == 10) {
                            flag = false;
                            break;
                        }
                    }
                }

                if (flag) {
                    for (int num42 = num33 - 3; num42 < num33; num42++) {
                        for (int num43 = num34 - 3; num43 <= num34 + 3; num43++) {
                            if (Main.tile[num43, num42].HasTile) {
                                flag = false;
                                break;
                            }
                        }
                    }
                }

                if (flag && num33 - num32 < 20) {
                    bool flag2 = false;
                    if (GenVars.DDoorPos[num27] == 0 && num33 - num32 < num30)
                        flag2 = true;

                    if (GenVars.DDoorPos[num27] == -1 && num34 > num31)
                        flag2 = true;

                    if (GenVars.DDoorPos[num27] == 1 && (num34 < num31 || num31 == 0))
                        flag2 = true;

                    if (flag2) {
                        num31 = num34;
                        num30 = num33 - num32;
                    }
                }
            }

            if (num30 >= 20)
                continue;

            int num44 = num31;
            int num45 = GenVars.DDoorY[num27];
            int num46 = num45;
            for (; !Main.tile[num44, num45].HasTile; num45++) {
                Tile tile3 = Main.tile[num44, num45];
                tile3.HasTile = false;
            }

            while (!Main.tile[num44, num46].HasTile) {
                num46--;
            }

            num45--;
            num46++;
            for (int num47 = num46; num47 < num45 - 2; num47++) {
                Main.tile[num44, num47].Clear(TileDataType.Slope);
                Tile tile3 = Main.tile[num44, num47];
                tile3.HasTile = true;
                Main.tile[num44, num47].TileType = num2;
                if (Main.tile[num44 - 1, num47].TileType == num2) {
                    tile3 = Main.tile[num44 - 1, num47];
                    tile3.HasTile = false;
                    Main.tile[num44 - 1, num47].ClearEverything();
                    Main.tile[num44 - 1, num47].WallType = (ushort)num3;
                }

                if (Main.tile[num44 - 2, num47].TileType == num2) {
                    tile3 = Main.tile[num44 - 2, num47];
                    tile3.HasTile = false;
                    Main.tile[num44 - 2, num47].ClearEverything();
                    Main.tile[num44 - 2, num47].WallType = (ushort)num3;
                }

                if (Main.tile[num44 + 1, num47].TileType == num2) {
                    tile3 = Main.tile[num44 + 1, num47];
                    tile3.HasTile = false;
                    Main.tile[num44 + 1, num47].ClearEverything();
                    Main.tile[num44 + 1, num47].WallType = (ushort)num3;
                }

                if (Main.tile[num44 + 2, num47].TileType == num2) {
                    tile3 = Main.tile[num44 + 2, num47];
                    tile3.HasTile = false;
                    Main.tile[num44 + 2, num47].ClearEverything();
                    Main.tile[num44 + 2, num47].WallType = (ushort)num3;
                }
            }

            int style = 13;
            if (genRand.Next(3) == 0) {
                switch (num3) {
                    case 7:
                        style = 16;
                        break;
                    case 8:
                        style = 17;
                        break;
                    case 9:
                        style = 18;
                        break;
                }
            }

            WorldGen.PlaceTile(num44, num45, 10, mute: true, forced: false, -1, style);
            num44--;
            int num48 = num45 - 3;
            while (!Main.tile[num44, num48].HasTile) {
                num48--;
            }

            if (num45 - num48 < num45 - num46 + 5 && Main.tileDungeon[Main.tile[num44, num48].TileType]) {
                for (int num49 = num45 - 4 - genRand.Next(3); num49 > num48; num49--) {
                    Main.tile[num44, num49].Clear(TileDataType.Slope);
                    Tile tile3 = Main.tile[num44, num49];
                    tile3.HasTile = true;
                    Main.tile[num44, num49].TileType = num2;
                    if (Main.tile[num44 - 1, num49].TileType == num2) {
                        tile3 = Main.tile[num44 - 1, num49];
                        tile3.HasTile = false;
                        Main.tile[num44 - 1, num49].ClearEverything();
                        Main.tile[num44 - 1, num49].WallType = (ushort)num3;
                    }

                    if (Main.tile[num44 - 2, num49].TileType == num2) {
                        tile3 = Main.tile[num44 - 2, num49];
                        tile3.HasTile = false;
                        Main.tile[num44 - 2, num49].ClearEverything();
                        Main.tile[num44 - 2, num49].WallType = (ushort)num3;
                    }
                }
            }

            num44 += 2;
            num48 = num45 - 3;
            while (!Main.tile[num44, num48].HasTile) {
                num48--;
            }

            if (num45 - num48 < num45 - num46 + 5 && Main.tileDungeon[Main.tile[num44, num48].TileType]) {
                for (int num50 = num45 - 4 - genRand.Next(3); num50 > num48; num50--) {
                    Tile tile3 = Main.tile[num44, num50];
                    tile3.HasTile = true;
                    Main.tile[num44, num50].Clear(TileDataType.Slope);
                    Main.tile[num44, num50].TileType = num2;
                    if (Main.tile[num44 + 1, num50].TileType == num2) {
                        tile3 = Main.tile[num44 + 1, num50];
                        tile3.HasTile = false;
                        Main.tile[num44 + 1, num50].ClearEverything();
                        Main.tile[num44 + 1, num50].WallType = (ushort)num3;
                    }

                    if (Main.tile[num44 + 2, num50].TileType == num2) {
                        tile3 = Main.tile[num44 + 2, num50];
                        tile3.HasTile = false;
                        Main.tile[num44 + 2, num50].ClearEverything();
                        Main.tile[num44 + 2, num50].WallType = (ushort)num3;
                    }
                }
            }

            num45++;
            num44--;
            for (int num51 = num45 - 8; num51 < num45; num51++) {
                if (Main.tile[num44 + 2, num51].TileType == num2) {
                    Tile tile3 = Main.tile[num44 + 2, num51];
                    tile3.HasTile = false;
                    Main.tile[num44 + 2, num51].ClearEverything();
                    Main.tile[num44 + 2, num51].WallType = (ushort)num3;
                }

                if (Main.tile[num44 + 3, num51].TileType == num2) {
                    Tile tile3 = Main.tile[num44 + 3, num51];
                    tile3.HasTile = false;
                    Main.tile[num44 + 3, num51].ClearEverything();
                    Main.tile[num44 + 3, num51].WallType = (ushort)num3;
                }

                if (Main.tile[num44 - 2, num51].TileType == num2) {
                    Tile tile3 = Main.tile[num44 - 2, num51];
                    tile3.HasTile = false;
                    Main.tile[num44 - 2, num51].ClearEverything();
                    Main.tile[num44 - 2, num51].WallType = (ushort)num3;
                }

                if (Main.tile[num44 - 3, num51].TileType == num2) {
                    Tile tile3 = Main.tile[num44 - 3, num51];
                    tile3.HasTile = false;
                    Main.tile[num44 - 3, num51].ClearEverything();
                    Main.tile[num44 - 3, num51].WallType = (ushort)num3;
                }
            }

            Tile tile2 = Main.tile[num44 - 1, num45];
            tile2.HasTile = true;
            Main.tile[num44 - 1, num45].TileType = num2;
            Main.tile[num44 - 1, num45].Clear(TileDataType.Slope);
            Tile tile = Main.tile[num44 + 1, num45];
            tile.HasTile = true;
            Main.tile[num44 + 1, num45].TileType = num2;
            Main.tile[num44 + 1, num45].Clear(TileDataType.Slope);
        }

        int[] array = new int[3];
        switch (num3) {
            case 7:
                array[0] = 7;
                array[1] = 94;
                array[2] = 95;
                break;
            case 9:
                array[0] = 9;
                array[1] = 96;
                array[2] = 97;
                break;
            default:
                array[0] = 8;
                array[1] = 98;
                array[2] = 99;
                break;
        }

        for (int num52 = 0; num52 < 5; num52++) {
            for (int num53 = 0; num53 < 3; num53++) {
                int num54 = genRand.Next(40, 240);
                int num55 = genRand.Next(GenVars.dMinX, GenVars.dMaxX);
                int num56 = genRand.Next(GenVars.dMinY, GenVars.dMaxY);
                for (int num57 = num55 - num54; num57 < num55 + num54; num57++) {
                    for (int num58 = num56 - num54; num58 < num56 + num54; num58++) {
                        if (num58 > Main.worldSurface) {
                            double num59 = Math.Abs(num55 - num57);
                            double num60 = Math.Abs(num56 - num58);
                            if (Math.Sqrt(num59 * num59 + num60 * num60) < num54 * 0.4 && Main.wallDungeon[Main.tile[num57, num58].WallType])
                                WorldGen.Spread.WallDungeon(num57, num58, array[num53]);
                        }
                    }
                }
            }
        }

        Main.statusText = Lang.gen[58].Value + " 85%";
        for (int num61 = 0; num61 < GenVars.numDungeonPlatforms; num61++) {
            int num62 = GenVars.dungeonPlatformX[num61];
            int num63 = GenVars.dungeonPlatformY[num61];
            int num64 = Main.maxTilesX;
            int num65 = 10;
            if (num63 < Main.worldSurface + 50.0)
                num65 = 20;

            for (int num66 = num63 - 5; num66 <= num63 + 5; num66++) {
                int num67 = num62;
                int num68 = num62;
                bool flag3 = false;
                if (Main.tile[num67, num66].HasTile) {
                    flag3 = true;
                }
                else {
                    while (!Main.tile[num67, num66].HasTile) {
                        num67--;
                        if (!Main.tileDungeon[Main.tile[num67, num66].TileType] || num67 == 0) {
                            flag3 = true;
                            break;
                        }
                    }

                    while (!Main.tile[num68, num66].HasTile) {
                        num68++;
                        if (!Main.tileDungeon[Main.tile[num68, num66].TileType] || num68 == Main.maxTilesX - 1) {
                            flag3 = true;
                            break;
                        }
                    }
                }

                if (flag3 || num68 - num67 > num65)
                    continue;

                bool flag4 = true;
                int num69 = num62 - num65 / 2 - 2;
                int num70 = num62 + num65 / 2 + 2;
                int num71 = num66 - 5;
                int num72 = num66 + 5;
                for (int num73 = num69; num73 <= num70; num73++) {
                    for (int num74 = num71; num74 <= num72; num74++) {
                        if (Main.tile[num73, num74].HasTile && Main.tile[num73, num74].TileType == 19) {
                            flag4 = false;
                            break;
                        }
                    }
                }

                for (int num75 = num66 + 3; num75 >= num66 - 5; num75--) {
                    if (Main.tile[num62, num75].HasTile) {
                        flag4 = false;
                        break;
                    }
                }

                if (flag4) {
                    num64 = num66;
                    break;
                }
            }

            if (num64 <= num63 - 10 || num64 >= num63 + 10)
                continue;

            int num76 = num62;
            int num77 = num64;
            int num78 = num62 + 1;
            while (!Main.tile[num76, num77].HasTile) {
                Tile tile = Main.tile[num76, num77];
                tile.HasTile = true;
                Main.tile[num76, num77].TileType = 19;
                Main.tile[num76, num77].Clear(TileDataType.Slope);
                switch (num3) {
                    case 7:
                        Main.tile[num76, num77].TileFrameY = 108;
                        break;
                    case 8:
                        Main.tile[num76, num77].TileFrameY = 144;
                        break;
                    default:
                        Main.tile[num76, num77].TileFrameY = 126;
                        break;
                }

                WorldGen.TileFrame(num76, num77);
                num76--;
            }

            for (; !Main.tile[num78, num77].HasTile; num78++) {
                Tile tile = Main.tile[num78, num77];
                tile.HasTile = true;
                Main.tile[num78, num77].TileType = 19;
                Main.tile[num78, num77].Clear(TileDataType.Slope);
                switch (num3) {
                    case 7:
                        Main.tile[num78, num77].TileFrameY = 108;
                        break;
                    case 8:
                        Main.tile[num78, num77].TileFrameY = 144;
                        break;
                    default:
                        Main.tile[num78, num77].TileFrameY = 126;
                        break;
                }

                WorldGen.TileFrame(num78, num77);
            }
        }

        int num79 = 5;
        if (WorldGen.drunkWorldGen)
            num79 = 6;

        num79 += 1;

        for (int num80 = 0; num80 < num79; num80++) {
            bool flag5 = false;
            while (!flag5) {
                int num81 = genRand.Next(GenVars.dMinX, GenVars.dMaxX);
                int num82 = genRand.Next((int)Main.worldSurface, GenVars.dMaxY);
                if (!Main.wallDungeon[Main.tile[num81, num82].WallType] || Main.tile[num81, num82].HasTile)
                    continue;

                ushort chestTileType = 21;
                int contain = 0;
                int style2 = 0;
                switch (num80) {
                    case 0:
                        style2 = 23;
                        contain = 1156;
                        break;
                    case 1:
                        if (!WorldGen.crimson) {
                            style2 = 24;
                            contain = 1571;
                        }
                        else {
                            style2 = 25;
                            contain = 1569;
                        }
                        break;
                    case 5:
                        if (WorldGen.crimson) {
                            style2 = 24;
                            contain = 1571;
                        }
                        else {
                            style2 = 25;
                            contain = 1569;
                        }
                        break;
                    case 2:
                        style2 = 26;
                        contain = 1260;
                        break;
                    case 3:
                        style2 = 27;
                        contain = 1572;
                        break;
                    case 4:
                        chestTileType = 467;
                        style2 = 13;
                        contain = 4607;
                        break;
                }

                bool flag0 = false;
                if (num80 == (WorldGen.drunkWorldGen ? 6 : 5)) {
                    chestTileType = (ushort)ModContent.TileType<BackwoodsDungeonChest>();
                    contain = ModContent.ItemType<IOU>();
                    flag0 = true;
                }

                if (flag0) {
                    flag5 = WorldGen.AddBuriedChest(num81, num82, contain, notNearOtherChests: false, 0, trySlope: false, chestTileType);
                }
                else {
                    flag5 = WorldGen.AddBuriedChest(num81, num82, contain, notNearOtherChests: false, style2, trySlope: false, chestTileType);
                }
            }
        }

        int[] array2 = new int[3] {
            genRand.Next(9, 13),
            genRand.Next(9, 13),
            0
        };

        while (array2[1] == array2[0]) {
            array2[1] = genRand.Next(9, 13);
        }

        array2[2] = genRand.Next(9, 13);
        while (array2[2] == array2[0] || array2[2] == array2[1]) {
            array2[2] = genRand.Next(9, 13);
        }

        Main.statusText = Lang.gen[58].Value + " 90%";
        num13 = 0;
        num14 = 1000;
        num15 = 0;
        while (num15 < Main.maxTilesX / 20) {
            num13++;
            int num83 = genRand.Next(GenVars.dMinX, GenVars.dMaxX);
            int num84 = genRand.Next(GenVars.dMinY, GenVars.dMaxY);
            bool flag6 = true;
            if (Main.wallDungeon[Main.tile[num83, num84].WallType] && !Main.tile[num83, num84].HasTile) {
                int num85 = 1;
                if (genRand.Next(2) == 0)
                    num85 = -1;

                while (flag6 && !Main.tile[num83, num84].HasTile) {
                    num83 -= num85;
                    if (num83 < 5 || num83 > Main.maxTilesX - 5)
                        flag6 = false;
                    else if (Main.tile[num83, num84].HasTile && !Main.tileDungeon[Main.tile[num83, num84].TileType])
                        flag6 = false;
                }

                if (flag6 && Main.tile[num83, num84].HasTile && Main.tileDungeon[Main.tile[num83, num84].TileType] && Main.tile[num83, num84 - 1].HasTile && Main.tileDungeon[Main.tile[num83, num84 - 1].TileType] && Main.tile[num83, num84 + 1].HasTile && Main.tileDungeon[Main.tile[num83, num84 + 1].TileType]) {
                    num83 += num85;
                    for (int num86 = num83 - 3; num86 <= num83 + 3; num86++) {
                        for (int num87 = num84 - 3; num87 <= num84 + 3; num87++) {
                            if (Main.tile[num86, num87].HasTile && Main.tile[num86, num87].TileType == 19) {
                                flag6 = false;
                                break;
                            }
                        }
                    }

                    if (flag6 && !Main.tile[num83, num84 - 1].HasTile & !Main.tile[num83, num84 - 2].HasTile & !Main.tile[num83, num84 - 3].HasTile) {
                        int num88 = num83;
                        int num89 = num83;
                        for (; num88 > GenVars.dMinX && num88 < GenVars.dMaxX && !Main.tile[num88, num84].HasTile && !Main.tile[num88, num84 - 1].HasTile && !Main.tile[num88, num84 + 1].HasTile; num88 += num85) {
                        }

                        num88 = Math.Abs(num83 - num88);
                        bool flag7 = false;
                        if (genRand.Next(2) == 0)
                            flag7 = true;

                        if (num88 > 5) {
                            for (int num90 = genRand.Next(1, 4); num90 > 0; num90--) {
                                Tile tile = Main.tile[num83, num84];
                                tile.HasTile = true;
                                Main.tile[num83, num84].Clear(TileDataType.Slope);
                                Main.tile[num83, num84].TileType = 19;
                                if (Main.tile[num83, num84].WallType == array[0])
                                    Main.tile[num83, num84].TileFrameY = (short)(18 * array2[0]);
                                else if (Main.tile[num83, num84].WallType == array[1])
                                    Main.tile[num83, num84].TileFrameY = (short)(18 * array2[1]);
                                else
                                    Main.tile[num83, num84].TileFrameY = (short)(18 * array2[2]);

                                WorldGen.TileFrame(num83, num84);
                                if (flag7) {
                                    WorldGen.PlaceTile(num83, num84 - 1, 50, mute: true);
                                    if (genRand.Next(50) == 0 && num84 > (Main.worldSurface + Main.rockLayer) / 2.0 && Main.tile[num83, num84 - 1].TileType == 50)
                                        Main.tile[num83, num84 - 1].TileFrameX = 90;
                                }

                                num83 += num85;
                            }

                            num13 = 0;
                            num15++;
                            if (!flag7 && genRand.Next(2) == 0) {
                                num83 = num89;
                                num84--;
                                int num91 = 0;
                                if (genRand.Next(4) == 0)
                                    num91 = 1;

                                switch (num91) {
                                    case 0:
                                        num91 = 13;
                                        break;
                                    case 1:
                                        num91 = 49;
                                        break;
                                }

                                WorldGen.PlaceTile(num83, num84, num91, mute: true);
                                if (Main.tile[num83, num84].TileType == 13) {
                                    if (genRand.Next(2) == 0)
                                        Main.tile[num83, num84].TileFrameX = 18;
                                    else
                                        Main.tile[num83, num84].TileFrameX = 36;
                                }
                            }
                        }
                    }
                }
            }

            if (num13 > num14) {
                num13 = 0;
                num15++;
            }
        }

        Main.statusText = Lang.gen[58].Value + " 95%";
        int num92 = 1;
        for (int num93 = 0; num93 < GenVars.numDRooms; num93++) {
            int num94 = 0;
            while (num94 < 1000) {
                int num95 = (int)(GenVars.dRoomSize[num93] * 0.4);
                int i3 = GenVars.dRoomX[num93] + genRand.Next(-num95, num95 + 1);
                int num96 = GenVars.dRoomY[num93] + genRand.Next(-num95, num95 + 1);
                int num97 = 0;
                int style3 = 2;
                if (num92 == 1)
                    num92++;

                switch (num92) {
                    case 2:
                        num97 = 155;
                        break;
                    case 3:
                        num97 = 156;
                        break;
                    case 4:
                        num97 = !WorldGen.remixWorldGen ? 157 : 2623;
                        break;
                    case 5:
                        num97 = 163;
                        break;
                    case 6:
                        num97 = 113;
                        break;
                    case 7:
                        num97 = 3317;
                        break;
                    case 8:
                        num97 = 327;
                        style3 = 0;
                        break;
                    case 9:
                        num97 = ModContent.ItemType<RagingBoots>();
                        break;
                    default:
                        num97 = 164;
                        num92 = 0;
                        break;
                }

                if (num96 < Main.worldSurface + 50.0) {
                    num97 = 327;
                    style3 = 0;
                }

                if (num97 == 0 && genRand.Next(2) == 0) {
                    num94 = 1000;
                    continue;
                }

                if (WorldGen.AddBuriedChest(i3, num96, num97, notNearOtherChests: false, style3, trySlope: false, 0)) {
                    num94 += 1000;
                    num92++;
                }

                num94++;
            }
        }

        GenVars.dMinX -= 25;
        GenVars.dMaxX += 25;
        GenVars.dMinY -= 25;
        GenVars.dMaxY += 25;
        if (GenVars.dMinX < 0)
            GenVars.dMinX = 0;

        if (GenVars.dMaxX > Main.maxTilesX)
            GenVars.dMaxX = Main.maxTilesX;

        if (GenVars.dMinY < 0)
            GenVars.dMinY = 0;

        if (GenVars.dMaxY > Main.maxTilesY)
            GenVars.dMaxY = Main.maxTilesY;

        num13 = 0;
        num14 = 1000;
        num15 = 0;
        WorldGen_MakeDungeon_Lights(null, num2, ref num13, num14, ref num15, array);
        num13 = 0;
        num14 = 1000;
        num15 = 0;
        WorldGen_MakeDungeon_Traps(null, ref num13, num14, ref num15);
        double count = WorldGen_MakeDungeon_GroundFurniture(null, num3);
        count = WorldGen_MakeDungeon_Pictures(null, array, count);
        count = WorldGen_MakeDungeon_Banners(null, array, count);
    }

    public override void PostWorldGen() {
        _cactiCactusCaneAdded = false;
        _iceCaneAdded = false;
        _mushroomStaffAdded = false;
        _hellfireClawsAdded = false;
        _giantTreeSaplingAdded = false;
        _feathersBottleAdded = false;
        _oniMaskAdded = false;
    }

    private static bool IsUndergroundDesert(int x, int y) {
        if (y < Main.worldSurface)
            return false;

        if (x < Main.maxTilesX * 0.15 || x > Main.maxTilesX * 0.85)
            return false;

        if (WorldGen.remixWorldGen && y > Main.rockLayer)
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
        if (y < Main.worldSurface)
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

            if (WorldGen.remixWorldGen && i > Main.maxTilesX * 0.37 && i < Main.maxTilesX * 0.63 && k > Main.maxTilesY - 250)
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
            bool flag12 = num7 >= Main.worldSurface + 25.0;
            if (WorldGen.remixWorldGen)
                flag12 = num7 < Main.maxTilesY - 400;

            if (flag12 || contain > 0)
                num9 = 1;

            if (Style >= 0)
                num9 = Style;

            if (chestTileType == 467 && num9 == 10 || contain == 0 && num7 <= Main.maxTilesY - 205 && IsUndergroundDesert(i, k)) {
                flag2 = true;
                num9 = 10;
                chestTileType = 467;
                bool added = false;
                if (!_cactiCactusCaneAdded) {
                    contain = (short)ModContent.ItemType<CactiCaster>();
                    _cactiCactusCaneAdded = true;
                    added = true;
                }
                if (!added) {
                    contain = num7 <= (GenVars.desertHiveHigh * 3 + GenVars.desertHiveLow * 4) / 7 ? Utils.SelectRandom(genRand, new short[5] {
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
                    });

                    if (WorldGen.getGoodWorldGen && genRand.Next(num) == 0)
                        contain = 52;
                }
            }

            if (chestTileType == 21 && (num9 == 11 || contain == 0 && num7 >= Main.worldSurface + 25.0 && num7 <= Main.maxTilesY - 205 && (Main.tile[i, k].TileType == 147 || Main.tile[i, k].TileType == 161 || Main.tile[i, k].TileType == 162))) {
                flag = true;
                num9 = 11;
                bool added = false;
                short iceCaneType = (short)ModContent.ItemType<SpikedIceStaff>();
                if (!_iceCaneAdded) {
                    contain = iceCaneType;
                    _iceCaneAdded = true;
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
                            contain = !WorldGen.remixWorldGen ? 1319 : 725;
                            break;
                        case 4:
                            contain = 987;
                            break;
                        case 5:
                            contain = iceCaneType;
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

            num8 = chestTileType != 467 ? WorldGen.PlaceChest(i - 1, num7 - 1, chestTileType, notNearOtherChests, num9) : WorldGen.PlaceChest(i - 1, num7 - 1, chestTileType, notNearOtherChests, num9);
            if (num8 >= 0) {
                if (flag11) {
                    GenVars.hellChest++;
                    if (GenVars.hellChest >= GenVars.hellChestItem.Length)
                        GenVars.hellChest = 0;
                }

                Chest chest = Main.chest[num8];
                int num10 = 0;
                while (num10 == 0) {
                    bool flag13 = num7 < Main.worldSurface + 25.0;
                    if (WorldGen.remixWorldGen)
                        flag13 = num7 >= (Main.rockLayer + (Main.maxTilesY - 350) * 2) / 3.0;

                    if (num9 == 0 && flag13 || flag9) {
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

                            if (contain == ModContent.ItemType<IOU>()) {
                                chest.item[num10].SetDefaults(ModContent.ItemType<Hedgehog>());
                                num10++;
                            }

                            if (Main.tenthAnniversaryWorld && flag9) {
                                chest.item[num10++].SetDefaults(848);
                                chest.item[num10++].SetDefaults(866);
                            }
                        }
                        else {
                            int num12 = genRand.Next(11);
                            int mushroomStaffRodType = ModContent.ItemType<MushroomStaff>();
                            if (!_mushroomStaffAdded && genRand.NextBool(2)) {
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
                    else if (!WorldGen.remixWorldGen && num7 < Main.rockLayer || WorldGen.remixWorldGen && num7 > Main.rockLayer && num7 < Main.maxTilesY - 250) {
                        if (contain > 0) {
                            if (contain == 832) {
                                chest.item[num10].SetDefaults(933);
                                num10++;
                            }

                            chest.item[num10].SetDefaults(contain);
                            chest.item[num10].Prefix(-1);
                            num10++;

                            if (contain == ModContent.ItemType<IOU>()) {
                                chest.item[num10].SetDefaults(ModContent.ItemType<Hedgehog>());
                                num10++;
                            }

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

                            bool flag15 = chestTileType == (ushort)ModContent.TileType<ElderwoodChest>();
                            if (flag15) {
                                if (!_oniMaskAdded || (_oniMaskAdded && genRand.NextBool(5))) {
                                    _oniMaskAdded = true;
                                    chest.item[num10].SetDefaults(ModContent.ItemType<OniMask>());
                                    chest.item[num10].Prefix(-1);
                                    num10++;
                                }
                            }

                            if (genRand.Next(20) == 0) {
                                chest.item[num10].SetDefaults(genRand.NextBool(4) ? ModContent.ItemType<TanningRack>() : 997);
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
                            int num20 = genRand.Next(10);
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

                            if (num20 == 9)
                                chest.item[num10].SetDefaults(ModContent.ItemType<WillpowerPotion>());

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
                    else if (num7 < Main.maxTilesY - 250 || WorldGen.remixWorldGen && (Style == 7 || Style == 14)) {
                        if (contain > 0) {
                            chest.item[num10].SetDefaults(contain);
                            chest.item[num10].Prefix(-1);
                            num10++;

                            if (contain == ModContent.ItemType<IOU>()) {
                                chest.item[num10].SetDefaults(ModContent.ItemType<Hedgehog>());
                                num10++;
                            }

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
                                flag14 = num7 > Main.worldSurface && num7 < Main.rockLayer;

                            int maxValue = 20;
                            if (WorldGen.tenthAnniversaryWorldGen)
                                maxValue = 15;

                            if (genRand.Next(maxValue) == 0 && flag14) {
                                chest.item[num10].SetDefaults(906);
                                chest.item[num10].Prefix(-1);
                            }
                            else if (genRand.Next(15) == 0) {
                                chest.item[num10].SetDefaults(genRand.NextBool(4) ? ModContent.ItemType<TanningRack>() : 997);
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

                            if (chestTileType == (ushort)ModContent.TileType<ElderwoodChest>()) {
                                if (!_oniMaskAdded || (_oniMaskAdded && genRand.NextBool(5))) {
                                    _oniMaskAdded = true;
                                    chest.item[num10].SetDefaults(ModContent.ItemType<OniMask>());
                                    chest.item[num10].Prefix(-1);
                                    num10++;
                                }
                            }

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
                            int num24 = genRand.Next(7);
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

                            if (num24 == 6)
                                chest.item[num10].SetDefaults(ModContent.ItemType<ResiliencePotion>());

                            chest.item[num10].stack = stack19;
                            num10++;
                        }

                        if (genRand.Next(3) > 1) {
                            int num25 = genRand.Next(8);
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

                            if (num25 == 6)
                                chest.item[num10].SetDefaults(ModContent.ItemType<DryadBloodPotion>());

                            if (num25 == 7)
                                chest.item[num10].SetDefaults(ModContent.ItemType<WeightPotion>());

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

                            if (contain == ModContent.ItemType<IOU>()) {
                                chest.item[num10].SetDefaults(ModContent.ItemType<Hedgehog>());
                                num10++;
                            }

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

                        if (chestTileType == (ushort)ModContent.TileType<ElderwoodChest>()) {
                            if (!_oniMaskAdded || (_oniMaskAdded && genRand.NextBool(5))) {
                                _oniMaskAdded = true;
                                chest.item[num10].SetDefaults(ModContent.ItemType<OniMask>());
                                chest.item[num10].Prefix(-1);
                                num10++;
                            }
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
                            int num30 = genRand.Next(9);
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

                            if (num30 == 8)
                                chest.item[num10].SetDefaults(ModContent.ItemType<BloodlustPotion>());

                            chest.item[num10].stack = stack26;
                            num10++;
                        }

                        if (genRand.Next(3) > 0) {
                            int num31 = genRand.Next(9);
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

                            if (num31 == 8)
                                chest.item[num10].SetDefaults(ModContent.ItemType<DeathWardPotion>());

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

                        if ((num9 == 23 || num9 == 24 || num9 == 25 || num9 == 26 || num9 == 27 || chestTileType == ModContent.TileType<BackwoodsDungeonChest>()) && genRand.Next(2) == 0) {
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

                    if ((num9 == 13 || chestTileType == ModContent.TileType<BackwoodsDungeonChest>()) && genRand.Next(2) == 0) {
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
