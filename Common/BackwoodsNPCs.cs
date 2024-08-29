using RoA.Content.Biomes.Backwoods;
using RoA.Content.NPCs.Enemies.Backwoods;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common;

sealed class BackwoodsNPCs : GlobalNPC {
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
        if (player.InModBiome(BackwoodsBiome.Instance)) {
            spawnRate = (int)(spawnRate * 0.3f);
        }
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
        if (spawnInfo.Player.InModBiome(BackwoodsBiome.Instance)) {
            pool.Clear();

            if (BackwoodsVars.BackwoodsTileTypes.Contains((ushort)spawnInfo.SpawnTileType)) {
                if (NPC.downedBoss2) {
                    pool.Add(ModContent.NPCType<Fleder>(), 1f);
                    pool.Add(ModContent.NPCType<FlederSachem>(), 0.2f);
                }

                pool.Add(ModContent.NPCType<BabyFleder>(), NPC.downedBoss2 ? 0.35f : 1f);
            }
        }
    }

}
