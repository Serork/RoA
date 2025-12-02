using RoA.Common.Tiles;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Miscellaneous;

sealed partial class Acalypha : CollectableFlower, IGrowLikeTulip {
    //private static bool _hallowOnLeft = true;

    public override void Load() {
        //On_WorldGen.GERunner += On_WorldGen_GERunner;
        On_WorldGen.smCallback_End += On_WorldGen_smCallback_End;
    }

    //private void On_WorldGen_GERunner(On_WorldGen.orig_GERunner orig, int i, int j, double speedX, double speedY, bool good) {
    //    orig(i, j, speedX, speedY, good);
    //    if (good) {
    //        _hallowOnLeft = Math.Sign(speedX) > 0;
    //    }
    //}

    private void On_WorldGen_smCallback_End(On_WorldGen.orig_smCallback_End orig, System.Collections.Generic.List<Terraria.WorldBuilding.GenPass> hardmodeTasks) {
        orig(hardmodeTasks);

        ushort acalyphaTileType = (ushort)ModContent.TileType<Acalypha>();
        bool tryToGrowAcalypha(int k, int l) {
            if (Main.tile[k, l].HasTile && Main.tile[k, l].HasUnactuatedTile && GrowPlantsOverTime.CanGrowAPlant(acalyphaTileType, k, l)) {
                if (WorldGen.genRand.NextChance(0.01)) {
                    GrowPlantsOverTime.GrowAPlant(acalyphaTileType, k, l);
                    if (Main.tile[k, l - 1].TileType == acalyphaTileType) {
                        return true;
                    }
                }
            }
            return false;
        }

        int surfaceAttempts = 200;
        while (true) {
            if (surfaceAttempts > 0) {
                for (int k = 20; k < Main.maxTilesX - 20; k++) {
                    for (int l = WorldGenHelper.SafeFloatingIslandY; l < (int)Main.worldSurface; l++) {
                        if (tryToGrowAcalypha(k, l)) {
                            return;
                        }
                    }
                }
                surfaceAttempts--;
            }
            else {
                for (int k = 20; k < Main.maxTilesX - 20; k++) {
                    for (int l = (int)Main.worldSurface; l < Main.maxTilesY - 20; l++) {
                        if (tryToGrowAcalypha(k, l)) {
                            return;
                        }
                    }
                }
            }
        }
        //if (_hallowOnLeft) {
        //    for (int k = 20; k < Main.maxTilesX - 20; k++) {
        //        if (flag) {
        //            break;
        //        }
        //        for (int l = 20; l < Main.maxTilesY - 20; l++) {
        //            if (tryToGrowAcalypha(k, l)) {
        //                flag = true;
        //                break;
        //            }
        //        }
        //    }
        //}
        //else {
        //    for (int k = Main.maxTilesX - 20; k > 20; k--) {
        //        if (flag) {
        //            break;
        //        }
        //        for (int l = 20; l < Main.maxTilesY - 20; l++) {
        //            if (tryToGrowAcalypha(k, l)) {
        //                flag = true;
        //                break;
        //            }
        //        }
        //    }
        //}
    }

}
