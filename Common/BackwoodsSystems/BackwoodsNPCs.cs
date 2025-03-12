using RoA.Common.CustomConditions;
using RoA.Common.WorldEvents;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Content.NPCs.Friendly;
using RoA.Content.Tiles.Platforms;
using RoA.Content.Tiles.Solid.Backwoods;
using RoA.Content.Tiles.Walls;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace RoA.Common.BackwoodsSystems;

sealed class BackwoodsNPCs : GlobalNPC {
    public override void ModifyGlobalLoot(GlobalLoot globalLoot) {
        globalLoot.Add(ItemDropRule.ByCondition(new BackwoodsKeyCondition(), ModContent.ItemType<BackwoodsDungeonKey>(), 2500, 1, 1));
    }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns) {
        if (player.InModBiome<BackwoodsBiome>() && !player.ZoneJungle) {
            spawnRate = (int)(spawnRate * 0.2f);
            maxSpawns = (int)(maxSpawns * 1.5f);
        }
    }

    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo) {
        if (spawnInfo.Player.InModBiome<BackwoodsBiome>()) {
            bool surface = spawnInfo.SpawnTileY < BackwoodsVars.FirstTileYAtCenter + 35;
            bool surface2 = spawnInfo.SpawnTileY < BackwoodsVars.FirstTileYAtCenter + 45;
            IEnumerator<KeyValuePair<int, float>> enumerator = pool.GetEnumerator();
            while (enumerator.MoveNext()) {
                pool[enumerator.Current.Key] *= 0.5f;
            }
            //if (surface) {
            //    pool.Clear();
            //}
            //else {
            //    IEnumerator<KeyValuePair<int, float>> enumerator = pool.GetEnumerator();
            //    while (enumerator.MoveNext()) {
            //        pool[enumerator.Current.Key] *= 0.5f;
            //    }
            //}

            Tile tile = WorldGenHelper.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY);
            Tile belowTile = WorldGenHelper.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY + 1);
            bool trulySurface = !belowTile.AnyWall() || belowTile.WallType == ModContent.WallType<LivingBackwoodsLeavesWall>();
            bool flag = false;
            for (int i = spawnInfo.SpawnTileX - 1; i < spawnInfo.SpawnTileX + 1; i++) {
                if (flag) {
                    break;
                }
                for (int j = spawnInfo.SpawnTileY - 1; j < spawnInfo.SpawnTileY + 1; j++) {
                    if (WorldGenHelper.GetTileSafely(i, j).WallType == ModContent.WallType<ElderwoodWall>()) {
                        flag = true;
                        break;
                    }
                }
            }
            pool.Add(ModContent.NPCType<Content.NPCs.Friendly.Hedgehog>(), 0.05f);
            float chance = surface ? 1f : 0.5f;
            if (!surface2 && !NPC.AnyNPCs(ModContent.NPCType<GrimDefender>())) {
                pool.Add(ModContent.NPCType<GrimDefender>(), 0.1f);
            }
            if (BackwoodsVars.BackwoodsTileTypes.Contains((ushort)spawnInfo.SpawnTileType) && !flag) {
                if (tile.TileType != ModContent.TileType<TreeBranch>() && tile.TileType != ModContent.TileType<LivingElderwoodlLeaves>()) {
                    if (!tile.AnyLiquid() && trulySurface && !NPC.AnyNPCs(ModContent.NPCType<CrowdRaven>())) {
                        pool.Add(ModContent.NPCType<CrowdRaven>(), 1f);
                    }
                    if (surface) {
                        if (!Main.IsItDay()) {
                            pool.Add(ModContent.NPCType<Lumberjack>(), 0.25f);
                        }
                        if (trulySurface) {
                            pool.Add(ModContent.NPCType<Hog>(), 0.25f);
                        }
                    }
                    if (NPC.downedBoss2) {
                        pool.Add(ModContent.NPCType<GrimDruid>(), 0.25f);
                    }
                }
                pool.Add(ModContent.NPCType<BabyFleder>(), NPC.downedBoss2 ? 0.35f : 1f);

                if (NPC.downedBoss2) {
                    pool.Add(ModContent.NPCType<Fleder>(), 1f * chance);
                    pool.Add(ModContent.NPCType<FlederSachem>(), 0.2f * (chance - 0.5f));
                    var ent = ModContent.NPCType<EntLegs>();
                    if (!NPC.AnyNPCs(ent)) {
                        pool.Add(ent, 0.05f);
                    }
                    if (BackwoodsFogHandler.IsFogActive && surface) {
                        pool.Add(ModContent.NPCType<Ravencaller>(), 0.2f);
                        pool.Add(ModContent.NPCType<DeerSkullHead>(), 0.2f);
                    }
                    var moonPhase = Main.GetMoonPhase();
                    if (moonPhase == Terraria.Enums.MoonPhase.Full ||
                        moonPhase == Terraria.Enums.MoonPhase.Empty) {
                        var archdruid = ModContent.NPCType<Archdruid>();
                        if (!NPC.AnyNPCs(archdruid)) {
                            pool.Add(archdruid, 0.025f);
                        }
                    }
                }
            }
        }
    }

}
