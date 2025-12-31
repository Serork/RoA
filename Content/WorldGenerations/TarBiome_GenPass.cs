using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

// TODO: seeds support
sealed class TarBiome_GenPass : ModSystem {
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        tasks.Insert(tasks.FindIndex(task => task.Name == "Settle Liquids") + 1, new PassLegacy("Tar", delegate (GenerationProgress progress, GameConfiguration passConfig) {
            int num916 = 5 * WorldGenHelper.WorldSize;
            double num917 = (double)(Main.maxTilesX - 200) / (double)num916;
            List<Point> list2 = new List<Point>(num916);
            int num918 = 0;
            int num919 = 0;
            while (num919 < num916) {
                double num920 = (double)num919 / (double)num916;
                progress.Set(num920);
                progress.Message = Language.GetOrRegister("Mods.RoA.WorldGen.Tar").Value;
                Point point3 = WorldGen.RandomRectanglePoint((int)(num920 * (double)(Main.maxTilesX - 200)) + 200, (int)GenVars.worldSurface + 200, (int)num917, (Main.maxTilesY - ((int)GenVars.lavaLine + 125)));
                //if (remixWorldGen)
                //    point3 = RandomRectanglePoint((int)(num920 * (double)(Main.maxTilesX - 200)) + 100, (int)GenVars.worldSurface + 100, (int)num917, (int)GenVars.rockLayer - (int)GenVars.worldSurface - 100);

                //Point point3 = new Point(WorldGen.genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance), WorldGen.genRand.Next((int)GenVars.worldSurface + 100, Main.maxTilesY - 500));
                //while ((double)point3.X < (double)Main.maxTilesX * 0.05 && (double)point3.X < (double)Main.maxTilesX * 0.95) {
                //    point3.X = WorldGen.genRand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);
                //}
                point3.X -= 100;

                num918++;
                if (TarBiome.CanPlace(point3, GenVars.structures)) {
                    list2.Add(point3);
                    num919++;
                }
                else if (num918 > 10000) {
                    //num916 = num919;
                    num919++;
                    num918 = 0;
                }
            }

            TarBiome tarBiome = GenVars.configuration.CreateBiome<TarBiome>();
            for (int num921 = 0; num921 < list2.Count; num921++) {
                tarBiome.Place(list2[num921], GenVars.structures);
            }
        }));
    }
}