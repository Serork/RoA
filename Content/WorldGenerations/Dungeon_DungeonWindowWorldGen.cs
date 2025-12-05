using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Decorations;
using RoA.Core;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class DungeonWindowWorldGen : IInitializer {
    void ILoadable.Load(Mod mod) {
        //WorldCommon.ModifyWorldGenTasksEvent += WorldCommon_ModifyWorldGenTasksEvent;

        On_WorldGen.MakeDungeon_Pictures += On_WorldGen_MakeDungeon_Pictures;
    }

    private double On_WorldGen_MakeDungeon_Pictures(On_WorldGen.orig_MakeDungeon_Pictures orig, int[] roomWall, double count) {
        count = 420000.0 / (double)Main.maxTilesX;
        count /= 6;
        UnifiedRandom genRand = WorldGen.genRand;
        for (int i = 0; (double)i < count; i++) {
            int num = genRand.Next(GenVars.dMinX, GenVars.dMaxX);
            int num2 = genRand.Next((int)Main.worldSurface, GenVars.dMaxY);
            while (!Main.wallDungeon[Main.tile[num, num2].WallType] || Main.tile[num, num2].HasTile) {
                num = genRand.Next(GenVars.dMinX, GenVars.dMaxX);
                num2 = genRand.Next((int)Main.worldSurface, GenVars.dMaxY);
            }

            int num3 = num;
            int num4 = num;
            int num5 = num2;
            int num6 = num2;
            int num7 = 0;
            int num8 = 0;
            for (int j = 0; j < 2; j++) {
                num3 = num;
                num4 = num;
                while (!Main.tile[num3, num2].HasTile && Main.wallDungeon[Main.tile[num3, num2].WallType]) {
                    num3--;
                }

                num3++;
                for (; !Main.tile[num4, num2].HasTile && Main.wallDungeon[Main.tile[num4, num2].WallType]; num4++) {
                }

                num4--;
                num = (num3 + num4) / 2;
                num5 = num2;
                num6 = num2;
                while (!Main.tile[num, num5].HasTile && Main.wallDungeon[Main.tile[num, num5].WallType]) {
                    num5--;
                }

                num5++;
                for (; !Main.tile[num, num6].HasTile && Main.wallDungeon[Main.tile[num, num6].WallType]; num6++) {
                }

                num6--;
                num2 = (num5 + num6) / 2;
            }

            num3 = num;
            num4 = num;
            while (!Main.tile[num3, num2].HasTile && !Main.tile[num3, num2 - 1].HasTile && !Main.tile[num3, num2 + 1].HasTile) {
                num3--;
            }

            num3++;
            for (; !Main.tile[num4, num2].HasTile && !Main.tile[num4, num2 - 1].HasTile && !Main.tile[num4, num2 + 1].HasTile; num4++) {
            }

            num4--;
            num5 = num2;
            num6 = num2;
            while (!Main.tile[num, num5].HasTile && !Main.tile[num - 1, num5].HasTile && !Main.tile[num + 1, num5].HasTile) {
                num5--;
            }

            num5++;
            for (; !Main.tile[num, num6].HasTile && !Main.tile[num - 1, num6].HasTile && !Main.tile[num + 1, num6].HasTile; num6++) {
            }

            num6--;
            num = (num3 + num4) / 2;
            num2 = (num5 + num6) / 2;
            num7 = num4 - num3;
            num8 = num6 - num5;
            if (num7 <= 7 || num8 <= 5)
                continue;

            bool[] array = new bool[3] {
                true,
                false,
                false
            };

            if (num7 > num8 * 3 && num7 > 21)
                array[1] = true;

            if (num8 > num7 * 3 && num8 > 21)
                array[2] = true;

            int num9 = genRand.Next(3);
            if (Main.tile[num, num2].WallType == roomWall[0])
                num9 = 0;

            while (!array[num9]) {
                num9 = genRand.Next(3);
            }

            if (WorldGen.nearPicture2(num, num2))
                num9 = -1;

            if (num9 == 0) {
                if (!WorldGen.nearPicture(num, num2) && !nearDungeonWindow(num, num2)) {
                    PlaceDungeonWindow(num, num2);
                    //PlaceTile(num, num2, paintingEntry2.tileType, mute: true, forced: false, -1, paintingEntry2.style);
                }
            }
        }

        return orig(roomWall, count);
    }

    public static bool nearDungeonWindow(int x, int y) {
        for (int i = x - 4; i <= x + 3; i++) {
            for (int j = y - 3; j <= y + 2; j++) {
                if (Main.tile[i, j].HasTile && Main.tile[i, j].TileType == ModContent.TileType<DungeonWindow>())
                    return true;
            }
        }

        return false;
    }

    //private void WorldCommon_ModifyWorldGenTasksEvent(System.Collections.Generic.List<GenPass> tasks, ref double totalWeight) {
    //    int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Dungeon"));

    //    tasks.Insert(genIndex, new PassLegacy("Dungeon Window", DungeonWindowGenerator, 31.6349f));
    //}

    //private static void DungeonWindowGenerator(GenerationProgress progress, GameConfiguration configuration) {

    //}

    private static void PlaceDungeonWindow(int x, int y) {
        ushort dungeonWindowTileType = (ushort)ModContent.TileType<DungeonWindow>();
        UnifiedRandom genRand = WorldGen.genRand;
        void makeMainBase(ref Point16 offset) {
            int sizeX = 3,
                sizeY = 4;
            int halfSizeX = sizeX / 2,
                halfSizeY = sizeY / 2;
            for (int i = 0; i < sizeX; i++) {
                for (int j = 0; j < sizeY; j++) {
                    int placeX = x + i - halfSizeX,
                        placeY = y + j - halfSizeY;
                    WorldGen.PlaceTile(placeX + offset.X, placeY + offset.Y, dungeonWindowTileType);
                }
            }
        }
        Point16 offset = Point16.Zero;
        for (int i = 0; i < 2; i++) {
            makeMainBase(ref offset);
            Vector2 direction = new(genRand.NextFloat(0.8f, 1.9f), genRand.NextFloat(0.8f, 1.9f));
            if (genRand.NextBool()) {
                direction.X *= -1;
            }
            offset += direction.ToPoint16();
        }
    }
}
