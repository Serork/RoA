using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.Generation;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace RoA.Content.WorldGenerations;

sealed class SpawnPointFix : ModSystem {
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight) {
        if (ModLoader.HasMod("Remnants")) {
            return;
        }

        int genIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Spawn Point"));
        tasks.RemoveAt(genIndex);

        tasks.Insert(genIndex, new PassLegacy("Spawn Point", SpawnPointFixer, 0.3068f));
    }

    private void SpawnPointFixer(GenerationProgress progress, GameConfiguration configuration) {
        progress.Set(1.0);
        int num310 = 5;
        bool flag10 = true;
        int num311 = Main.maxTilesX / 2;
        if (Main.tenthAnniversaryWorld && !WorldGen.remixWorldGen) {
            int num312 = GenVars.beachBordersWidth + 15;
            num311 = ((WorldGen.genRand.Next(2) != 0) ? (Main.maxTilesX - num312) : num312);
        }

        while (flag10) {
            int num313 = num311 + WorldGen.genRand.Next(-num310, num310 + 1);
            for (int num314 = 0; num314 < Main.maxTilesY; num314++) {
                if (WorldGen.SolidTile(num313, num314)) {
                    Main.spawnTileX = num313;
                    Main.spawnTileY = num314;
                    break;
                }
            }

            flag10 = false;
            num310++;
            if ((double)Main.spawnTileY > Main.worldSurface)
                flag10 = true;

            if (Main.tile[Main.spawnTileX, Main.spawnTileY - 1].LiquidAmount > 0)
                flag10 = true;
        }

        int num315 = 10;
        while ((double)Main.spawnTileY > Main.worldSurface) {
            int num316 = WorldGen.genRand.Next(num311 - num315, num311 + num315);
            for (int num317 = 0; num317 < Main.maxTilesY; num317++) {
                if (WorldGen.SolidTile(num316, num317)) {
                    Main.spawnTileX = num316;
                    Main.spawnTileY = num317;
                    break;
                }
            }

            num315++;
        }

        if (WorldGen.remixWorldGen) {
            int num318 = Main.maxTilesY - 10;
            while (WorldGen.SolidTile(Main.spawnTileX, num318)) {
                num318--;
            }

            Main.spawnTileY = num318 + 1;
        }
    }
}
