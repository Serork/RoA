using RoA.Common.CustomConditions;
using RoA.Common.World;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Miscellaneous;
using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Content.Tiles.Platforms;
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
        if (spawnInfo.Invasion || spawnInfo.PlayerInTown) {
            return;
        }

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
            bool trulySurface = !belowTile.AnyWall() || belowTile.WallType == ModContent.WallType<LivingBackwoodsLeavesWall2>();
            bool flag = false;
            for (int i = spawnInfo.SpawnTileX - 1; i < spawnInfo.SpawnTileX + 1; i++) {
                if (flag) {
                    break;
                }
                for (int j = spawnInfo.SpawnTileY - 1; j < spawnInfo.SpawnTileY + 1; j++) {
                    ushort wall = WorldGenHelper.GetTileSafely(i, j).WallType;
                    if (wall == ModContent.WallType<ElderwoodWall3>()) {
                        flag = true;
                        break;
                    }
                }
            }
            pool.Add(ModContent.NPCType<Content.NPCs.Friendly.Hedgehog>(), 0.05f);
            float chance = surface ? 1f : 0.5f;
            ushort grimDefenderType = (ushort)ModContent.NPCType<GrimDefender>();
            if (!surface2 && NPC.CountNPCS(grimDefenderType) < 3) {
                pool.Add(grimDefenderType, 0.1f);
            }
            bool notBranch = tile.TileType != ModContent.TileType<TreeBranch>();
            //bool notLeaves = tile.TileType != ModContent.TileType<LivingElderwoodlLeaves>();
            if (BackwoodsVars.BackwoodsTileTypes.Contains((ushort)spawnInfo.SpawnTileType) && !flag) {
                if (notBranch) {
                    ushort crowdRavenType = (ushort)ModContent.NPCType<CrowdRaven>();
                    if (!tile.AnyLiquid() && trulySurface && !NPC.AnyNPCs(crowdRavenType)) {
                        pool.Add(crowdRavenType, 1f);
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
                    pool.Add(ModContent.NPCType<FlederSachem>(), 0.15f * (chance - 0.5f));
                    if (notBranch) {
                        var ent = ModContent.NPCType<EntLegs>();
                        if (/*!NPC.AnyNPCs(ent) && */surface) {
                            pool.Add(ent, 0.02f);
                        }
                    }
                    if (BackwoodsFogHandler.IsFogActive && surface) {
                        if (notBranch) {
                            pool.Add(ModContent.NPCType<Ravencaller>(), 0.2f);
                        }
                        pool.Add(ModContent.NPCType<DeerSkullHead>(), 0.2f);
                    }
                    if (notBranch) {
                        var moonPhase = Main.GetMoonPhase();
                        if (moonPhase == Terraria.Enums.MoonPhase.Full ||
                            moonPhase == Terraria.Enums.MoonPhase.Empty) {
                            var archdruid = ModContent.NPCType<Archdruid>();
                            if (!NPC.AnyNPCs(archdruid)) {
                                pool.Add(archdruid, 0.01f);
                            }
                        }
                    }
                }
            }
        }
    }

}
