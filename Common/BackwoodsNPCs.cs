using RiseofAges.Content.Biomes.Backwoods;

using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Content.Tiles.Platforms;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Core.Utility;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
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

        if (NPC.downedBoss2) {
            List<int> flederValidTiles = [ModContent.TileType<BackwoodsGrass>(), ModContent.TileType<BackwoodsGreenMoss>()];
            Tile tile = WorldGenHelper.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);
            if (flederValidTiles.Contains(tile.TileType)) {
                pool.Add(ModContent.NPCType<Fleder>(), 1f);
            }
        }

        pool.Add(ModContent.NPCType<BabyFleder>(), 1f);
    }
}
