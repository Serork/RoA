using ReLogic.Utilities;

using RoA.Common.BackwoodsSystems;
using RoA.Core;

using System;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class Backwoods_MakeDungeonSkipBackwoods : IInitializer {
    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "lastMaxTilesX")]
    public extern static ref int WorldGen_lastMaxTilesX(WorldGen worldGen);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "lastMaxTilesY")]
    public extern static ref int WorldGen_lastMaxTilesY(WorldGen worldGen);

    void ILoadable.Load(Mod mod) {
        On_WorldGen.DungeonHalls += On_WorldGen_DungeonHalls;
    }

    private void On_WorldGen_DungeonHalls(On_WorldGen.orig_DungeonHalls orig, int i, int j, ushort tileType, int wallType, bool forceX) {
        Vector2D zero = Vector2D.Zero;
        var genRand = WorldGen.genRand;
        double num = genRand.Next(4, 6);
        double num2 = num;
        Vector2D zero2 = Vector2D.Zero;
        Vector2D zero3 = Vector2D.Zero;
        int num3 = 1;
        Vector2D vector2D = default(Vector2D);
        vector2D.X = i;
        vector2D.Y = j;
        int num4 = genRand.Next(35, 80);
        bool flag = false;
        if (genRand.Next(6) == 0)
            flag = true;

        if (forceX) {
            num4 += 20;
            GenVars.lastDungeonHall = Vector2D.Zero;
        }
        else if (genRand.Next(5) == 0) {
            num *= 2.0;
            num4 /= 2;
        }

        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = true;
        bool flag5 = false;
        while (!flag2) {
            flag5 = false;
            if (flag4 && !forceX) {
                bool flag6 = true;
                bool flag7 = true;
                bool flag8 = true;
                bool flag9 = true;
                int num5 = num4;
                bool flag10 = false;
                for (int num6 = j; num6 > j - num5; num6--) {
                    if (Main.tile[i, num6].WallType == wallType) {
                        if (flag10) {
                            flag6 = false;
                            break;
                        }
                    }
                    else {
                        flag10 = true;
                    }
                }

                flag10 = false;
                for (int k = j; k < j + num5; k++) {
                    if (Main.tile[i, k].WallType == wallType) {
                        if (flag10) {
                            flag7 = false;
                            break;
                        }
                    }
                    else {
                        flag10 = true;
                    }
                }

                flag10 = false;
                for (int num7 = i; num7 > i - num5; num7--) {
                    if (Main.tile[num7, j].WallType == wallType) {
                        if (flag10) {
                            flag8 = false;
                            break;
                        }
                    }
                    else {
                        flag10 = true;
                    }
                }

                flag10 = false;
                for (int l = i; l < i + num5; l++) {
                    if (Main.tile[l, j].WallType == wallType) {
                        if (flag10) {
                            flag9 = false;
                            break;
                        }
                    }
                    else {
                        flag10 = true;
                    }
                }

                if (!flag8 && !flag9 && !flag6 && !flag7) {
                    num3 = ((genRand.Next(2) != 0) ? 1 : (-1));
                    if (genRand.Next(2) == 0)
                        flag5 = true;
                }
                else {
                    int num8 = genRand.Next(4);
                    do {
                        num8 = genRand.Next(4);
                    } while (!(num8 == 0 && flag6) && !(num8 == 1 && flag7) && !(num8 == 2 && flag8) && !(num8 == 3 && flag9));

                    switch (num8) {
                        case 0:
                            num3 = -1;
                            break;
                        case 1:
                            num3 = 1;
                            break;
                        default:
                            flag5 = true;
                            num3 = ((num8 != 2) ? 1 : (-1));
                            break;
                    }
                }
            }
            else {
                num3 = ((genRand.Next(2) != 0) ? 1 : (-1));
                if (genRand.Next(2) == 0)
                    flag5 = true;
            }

            flag4 = false;
            if (forceX)
                flag5 = true;

            if (flag5) {
                zero2.Y = 0.0;
                zero2.X = num3;
                zero3.Y = 0.0;
                zero3.X = -num3;
                zero.Y = 0.0;
                zero.X = num3;
                if (genRand.Next(3) == 0) {
                    if (genRand.Next(2) == 0)
                        zero.Y = -0.2;
                    else
                        zero.Y = 0.2;
                }
            }
            else {
                num += 1.0;
                zero.Y = num3;
                zero.X = 0.0;
                zero2.X = 0.0;
                zero2.Y = num3;
                zero3.X = 0.0;
                zero3.Y = -num3;
                if (genRand.Next(3) != 0) {
                    flag3 = true;
                    if (genRand.Next(2) == 0)
                        zero.X = (double)genRand.Next(10, 20) * 0.1;
                    else
                        zero.X = (double)(-genRand.Next(10, 20)) * 0.1;
                }
                else if (genRand.Next(2) == 0) {
                    if (genRand.Next(2) == 0)
                        zero.X = (double)genRand.Next(20, 40) * 0.01;
                    else
                        zero.X = (double)(-genRand.Next(20, 40)) * 0.01;
                }
                else {
                    num4 /= 2;
                }
            }

            if (GenVars.lastDungeonHall != zero3)
                flag2 = true;
        }

        int num9 = 0;
        bool flag11 = vector2D.Y < Main.rockLayer + 100.0;
        if (WorldGen.remixWorldGen)
            flag11 = vector2D.Y < Main.worldSurface + 100.0;

        bool inBackwoods = vector2D.X > BackwoodsVars.BackwoodsStartX - BackwoodsVars.BackwoodsHalfSizeX * 2 && vector2D.X < BackwoodsVars.BackwoodsEndX + BackwoodsVars.BackwoodsHalfSizeX * 2 &&
            vector2D.Y < BackwoodsVars.BackwoodsEndY;

        if (!forceX) {
            if (vector2D.X > (double)(WorldGen_lastMaxTilesX(null) - 200) || (inBackwoods && vector2D.X < BackwoodsVars.BackwoodsCenterX)) {
                num3 = -1;
                zero2.Y = 0.0;
                zero2.X = num3;
                zero.Y = 0.0;
                zero.X = num3;
                if (genRand.Next(3) == 0) {
                    if (genRand.Next(2) == 0)
                        zero.Y = -0.2;
                    else
                        zero.Y = 0.2;
                }
            }
            else if (vector2D.X < 200.0 || (inBackwoods && vector2D.X > BackwoodsVars.BackwoodsCenterX)) {
                num3 = 1;
                zero2.Y = 0.0;
                zero2.X = num3;
                zero.Y = 0.0;
                zero.X = num3;
                if (genRand.Next(3) == 0) {
                    if (genRand.Next(2) == 0)
                        zero.Y = -0.2;
                    else
                        zero.Y = 0.2;
                }
            }
            else if (vector2D.Y > (double)(WorldGen_lastMaxTilesY(null) - 300)) {
                num3 = -1;
                num += 1.0;
                zero.Y = num3;
                zero.X = 0.0;
                zero2.X = 0.0;
                zero2.Y = num3;
                if (genRand.Next(2) == 0) {
                    if (genRand.Next(2) == 0)
                        zero.X = (double)genRand.Next(20, 50) * 0.01;
                    else
                        zero.X = (double)(-genRand.Next(20, 50)) * 0.01;
                }
            }
            else if (flag11) {
                num3 = 1;
                num += 1.0;
                zero.Y = num3;
                zero.X = 0.0;
                zero2.X = 0.0;
                zero2.Y = num3;
                if (genRand.Next(3) != 0) {
                    flag3 = true;
                    if (genRand.Next(2) == 0)
                        zero.X = (double)genRand.Next(10, 20) * 0.1;
                    else
                        zero.X = (double)(-genRand.Next(10, 20)) * 0.1;
                }
                else if (genRand.Next(2) == 0) {
                    if (genRand.Next(2) == 0)
                        zero.X = (double)genRand.Next(20, 50) * 0.01;
                    else
                        zero.X = (double)genRand.Next(20, 50) * 0.01;
                }
            }
            else if (vector2D.X < (double)(Main.maxTilesX / 2) && vector2D.X > (double)Main.maxTilesX * 0.25) {
                num3 = -1;
                zero2.Y = 0.0;
                zero2.X = num3;
                zero.Y = 0.0;
                zero.X = num3;
                if (genRand.Next(3) == 0) {
                    if (genRand.Next(2) == 0)
                        zero.Y = -0.2;
                    else
                        zero.Y = 0.2;
                }
            }
            else if (vector2D.X > (double)(Main.maxTilesX / 2) && vector2D.X < (double)Main.maxTilesX * 0.75) {
                num3 = 1;
                zero2.Y = 0.0;
                zero2.X = num3;
                zero.Y = 0.0;
                zero.X = num3;
                if (genRand.Next(3) == 0) {
                    if (genRand.Next(2) == 0)
                        zero.Y = -0.2;
                    else
                        zero.Y = 0.2;
                }
            }
        }

        if (zero2.Y == 0.0) {
            GenVars.DDoorX[GenVars.numDDoors] = (int)vector2D.X;
            GenVars.DDoorY[GenVars.numDDoors] = (int)vector2D.Y;
            GenVars.DDoorPos[GenVars.numDDoors] = 0;
            GenVars.numDDoors++;
        }
        else {
            GenVars.dungeonPlatformX[GenVars.numDungeonPlatforms] = (int)vector2D.X;
            GenVars.dungeonPlatformY[GenVars.numDungeonPlatforms] = (int)vector2D.Y;
            GenVars.numDungeonPlatforms++;
        }

        GenVars.lastDungeonHall = zero2;
        if (Math.Abs(zero.X) > Math.Abs(zero.Y) && genRand.Next(3) != 0)
            num = (int)(num2 * ((double)genRand.Next(110, 150) * 0.01));

        while (num4 > 0) {
            num9++;
            if (zero2.X > 0.0 && vector2D.X > (double)(Main.maxTilesX - 100))
                num4 = 0;
            else if (zero2.X < 0.0 && vector2D.X < 100.0)
                num4 = 0;
            else if (zero2.Y > 0.0 && vector2D.Y > (double)(Main.maxTilesY - 100))
                num4 = 0;
            else if (WorldGen.remixWorldGen && zero2.Y < 0.0 && vector2D.Y < (Main.rockLayer + Main.worldSurface) / 2.0)
                num4 = 0;
            else if (!WorldGen.remixWorldGen && zero2.Y < 0.0 && vector2D.Y < Main.rockLayer + 50.0)
                num4 = 0;

            num4--;
            int num10 = (int)(vector2D.X - num - 4.0 - (double)genRand.Next(6));
            int num11 = (int)(vector2D.X + num + 4.0 + (double)genRand.Next(6));
            int num12 = (int)(vector2D.Y - num - 4.0 - (double)genRand.Next(6));
            int num13 = (int)(vector2D.Y + num + 4.0 + (double)genRand.Next(6));
            if (num10 < 0)
                num10 = 0;

            if (num11 > Main.maxTilesX)
                num11 = Main.maxTilesX;

            if (num12 < 0)
                num12 = 0;

            if (num13 > Main.maxTilesY)
                num13 = Main.maxTilesY;

            for (int m = num10; m < num11; m++) {
                for (int n = num12; n < num13; n++) {
                    if (m < GenVars.dMinX)
                        GenVars.dMinX = m;

                    if (m > GenVars.dMaxX)
                        GenVars.dMaxX = m;

                    if (n > GenVars.dMaxY)
                        GenVars.dMaxY = n;

                    Main.tile[m, n].LiquidAmount = 0;
                    if (!Main.wallDungeon[Main.tile[m, n].WallType]) {
                        Tile tile = Main.tile[m, n];
                        tile.HasTile = true;
                        Main.tile[m, n].TileType = tileType;
                        Main.tile[m, n].Clear(TileDataType.Slope);
                    }
                }
            }

            for (int num14 = num10 + 1; num14 < num11 - 1; num14++) {
                for (int num15 = num12 + 1; num15 < num13 - 1; num15++) {
                    Main.tile[num14, num15].WallType = (ushort)wallType;
                }
            }

            int num16 = 0;
            if (zero.Y == 0.0 && genRand.Next((int)num + 1) == 0)
                num16 = genRand.Next(1, 3);
            else if (zero.X == 0.0 && genRand.Next((int)num - 1) == 0)
                num16 = genRand.Next(1, 3);
            else if (genRand.Next((int)num * 3) == 0)
                num16 = genRand.Next(1, 3);

            num10 = (int)(vector2D.X - num * 0.5 - (double)num16);
            num11 = (int)(vector2D.X + num * 0.5 + (double)num16);
            num12 = (int)(vector2D.Y - num * 0.5 - (double)num16);
            num13 = (int)(vector2D.Y + num * 0.5 + (double)num16);
            if (num10 < 0)
                num10 = 0;

            if (num11 > Main.maxTilesX)
                num11 = Main.maxTilesX;

            if (num12 < 0)
                num12 = 0;

            if (num13 > Main.maxTilesY)
                num13 = Main.maxTilesY;

            for (int num17 = num10; num17 < num11; num17++) {
                for (int num18 = num12; num18 < num13; num18++) {
                    Main.tile[num17, num18].Clear(TileDataType.Slope);
                    if (flag) {
                        if (Main.tile[num17, num18].HasTile || Main.tile[num17, num18].WallType != wallType) {
                            Tile tile = Main.tile[num17, num18];
                            tile.HasTile = true;
                            Main.tile[num17, num18].TileType = GenVars.crackedType;
                        }
                    }
                    else {
                        Tile tile = Main.tile[num17, num18];
                        tile.HasTile = false;
                    }

                    Main.tile[num17, num18].Clear(TileDataType.Slope);
                    Main.tile[num17, num18].WallType = (ushort)wallType;
                }
            }

            vector2D += zero;
            if (flag3 && num9 > genRand.Next(10, 20)) {
                num9 = 0;
                zero.X *= -1.0;
            }
        }

        GenVars.dungeonX = (int)vector2D.X;
        GenVars.dungeonY = (int)vector2D.Y;
        if (zero2.Y == 0.0) {
            GenVars.DDoorX[GenVars.numDDoors] = (int)vector2D.X;
            GenVars.DDoorY[GenVars.numDDoors] = (int)vector2D.Y;
            GenVars.DDoorPos[GenVars.numDDoors] = 0;
            GenVars.numDDoors++;
        }
        else {
            GenVars.dungeonPlatformX[GenVars.numDungeonPlatforms] = (int)vector2D.X;
            GenVars.dungeonPlatformY[GenVars.numDungeonPlatforms] = (int)vector2D.Y;
            GenVars.numDungeonPlatforms++;
        }
    }
}
