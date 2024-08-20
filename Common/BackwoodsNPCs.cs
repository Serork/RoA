using RiseofAges.Content.Biomes.Backwoods;

using RoA.Content.NPCs.Enemies.Backwoods;

using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common;

sealed class BackwoodsNPCs : GlobalNPC {
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
        if (player.InModBiome<BackwoodsBiome>()) {
            spawnRate = (int)(spawnRate * 0.3f);
        }
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
        if (!spawnInfo.Player.InModBiome<BackwoodsBiome>()) {
            return;
        }

        pool[0] = 0f;
        pool.Clear();

        pool.Add(ModContent.NPCType<Fleder>(), 1f);
    }
}
