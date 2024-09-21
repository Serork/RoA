using RoA.Content.Biomes.Backwoods;
using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Content.Tiles.Platforms;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.BackwoodsSystems;

sealed class BackwoodsNPCs : GlobalNPC {
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
        if (player.InModBiome<BackwoodsBiome>()) {
            spawnRate = (int)(spawnRate * 0.2f);
            maxSpawns = (int)(maxSpawns * 1.5f);
        }
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
        if (spawnInfo.Player.InModBiome<BackwoodsBiome>()) {
            pool.Clear();

            Tile tile = WorldGenHelper.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);
            if (BackwoodsVars.BackwoodsTileTypes.Contains((ushort)spawnInfo.SpawnTileType)) {
                if (tile.TileType != ModContent.TileType<TreeBranch>() && tile.TileType != ModContent.TileType<LivingElderwoodlLeaves>()) {
                    if (!Main.IsItDay()) {
                        pool.Add(ModContent.NPCType<Lumberjack>(), 0.25f);
                    }
                    Tile belowTile = WorldGenHelper.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY + 1);
                    if (!belowTile.AnyWall() || belowTile.WallType == ModContent.WallType<LivingBackwoodsLeavesWall>()) {
                        pool.Add(ModContent.NPCType<Hog>(), 0.25f);
                    }
                }
                pool.Add(ModContent.NPCType<BabyFleder>(), NPC.downedBoss2 ? 0.35f : 1f);

                if (NPC.downedBoss2) {
                    pool.Add(ModContent.NPCType<Fleder>(), 1f);
                    pool.Add(ModContent.NPCType<FlederSachem>(), 0.2f);
                    pool.Add(ModContent.NPCType<EntLegs>(), 0.2f);
                }
            }
        }
    }

}
