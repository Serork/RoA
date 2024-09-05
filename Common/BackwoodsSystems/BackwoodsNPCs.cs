using RoA.Content.Biomes.Backwoods;
using RoA.Content.NPCs.Enemies.Backwoods;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.BackwoodsSystems;

sealed class BackwoodsNPCs : GlobalNPC {
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
        if (player.InModBiome<BackwoodsBiome>()) {
            spawnRate = (int)(spawnRate * 0.3f);
        }
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
        if (spawnInfo.Player.InModBiome<BackwoodsBiome>()) {
            pool.Clear();

            if (BackwoodsVars.BackwoodsTileTypes.Contains((ushort)spawnInfo.SpawnTileType)) {
                if (NPC.downedBoss2) {
                    pool.Add(ModContent.NPCType<Fleder>(), 1f);
                    pool.Add(ModContent.NPCType<FlederSachem>(), 0.2f);
                }
                if (!Main.dayTime) {
                    pool.Add(ModContent.NPCType<Lumberjack>(), 0.2f);
                }
                if (spawnInfo.SpawnTileY < BackwoodsVars.FirstTileYAtCenter) {
                    pool.Add(ModContent.NPCType<Hog>(), 0.2f);
                }

                pool.Add(ModContent.NPCType<BabyFleder>(), NPC.downedBoss2 ? 0.35f : 1f);
            }
        }
    }

}
