using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.NPCs.Friendly;
using RoA.Content.Tiles.Solid.Backwoods;

using System.IO;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace RoA.Common.BackwoodsSystems;

sealed class HunterSpawnSystem : ModSystem {
    internal static bool HunterWasKilled;

    public static bool ShouldSpawnHunter { get; private set; } = false;
    public static bool ShouldDespawnHunter { get; private set; } = false;
    public static bool ShouldSpawnHunterAttack { get; private set; } = false;

    public override void ClearWorld() {
        HunterWasKilled = false;
        ShouldSpawnHunter = false;
        ShouldDespawnHunter = false;
        ShouldSpawnHunterAttack = false;
    }

    public override void SaveWorldData(TagCompound tag) {
        if (HunterWasKilled) {
            tag[RoA.ModName + nameof(HunterWasKilled)] = true;
        }

        if (ShouldSpawnHunter) {
            tag[RoA.ModName + nameof(ShouldSpawnHunter)] = true;
        }
        if (ShouldDespawnHunter) {
            tag[RoA.ModName + nameof(ShouldDespawnHunter)] = true;
        }
        if (ShouldSpawnHunterAttack) {
            tag[RoA.ModName + nameof(ShouldSpawnHunterAttack)] = true;
        }
    }

    public override void LoadWorldData(TagCompound tag) {
        HunterWasKilled = tag.ContainsKey(RoA.ModName + nameof(HunterWasKilled));

        ShouldSpawnHunter = tag.ContainsKey(RoA.ModName + nameof(ShouldSpawnHunter));
        ShouldDespawnHunter = tag.ContainsKey(RoA.ModName + nameof(ShouldDespawnHunter));
        ShouldSpawnHunterAttack = tag.ContainsKey(RoA.ModName + nameof(ShouldSpawnHunterAttack));
    }

    public override void NetSend(BinaryWriter writer) {
        var flags = new BitsByte();
        flags[0] = HunterWasKilled;

        flags[1] = ShouldSpawnHunter;
        flags[2] = ShouldDespawnHunter;
        flags[3] = ShouldSpawnHunterAttack;
        writer.Write(flags);
    }

    public override void NetReceive(BinaryReader reader) {
        BitsByte flags = reader.ReadByte();
        HunterWasKilled = flags[0];

        ShouldSpawnHunter = flags[1];
        ShouldDespawnHunter = flags[2];
        ShouldSpawnHunterAttack = flags[3];
    }

    public override void PostUpdatePlayers() {
        if (HunterWasKilled) {
            return;
        }

        SpawnHunter();

        SetHunterSpawnVariables();
    }

    private void SetHunterSpawnVariables() {
        bool hunterAlive = NPC.CountNPCS(ModContent.NPCType<Hunter>()) > 0;
        if (ShouldDespawnHunter && !hunterAlive) {
            ShouldDespawnHunter = false;
            ShouldSpawnHunterAttack = true;
        }
        if (Main.dayTime && !ShouldSpawnHunter) {
            if (Main.time < 1 || (Main.IsFastForwardingTime() && Main.time < 61)) {
                if (ShouldSpawnHunterAttack) {
                    ShouldSpawnHunterAttack = false;
                }
                if (!hunterAlive && !ShouldDespawnHunter) {
                    if (Main.rand.NextBool(7) && NPC.downedBoss2) {
                        ShouldSpawnHunter = true;
                        ShouldSpawnHunterAttack = false;
                    }
                }
            }
        }
        if (Main.dayTime) {
            if (Main.time > 42000) {
                if (hunterAlive) {
                    ShouldDespawnHunter = true;
                }
            }
        }
    }

    private int SpawnNPC_TryFindingProperGroundTileType(int spawnTileType, int x, int y) {
        if (!NPC.IsValidSpawningGroundTile(x, y)) {
            for (int i = y + 1; i < y + 30; i++) {
                if (NPC.IsValidSpawningGroundTile(x, i))
                    return Main.tile[x, i].TileType;
            }
        }

        return spawnTileType;
    }

    private void SpawnHunter() {
        if (!ShouldSpawnHunter) {
            return;
        }

        int type = ModContent.NPCType<Hunter>();
        if (NPC.CountNPCS(type) > 0) {
            return;
        }

        bool flag = (double)Main.windSpeedTarget < -0.4 || (double)Main.windSpeedTarget > 0.4;
        bool flag2 = false;
        bool flag3 = false;
        int num = 0;
        bool flag4 = false;
        bool flag5 = false;
        bool flag6 = false;
        bool flag7 = false;
        bool flag8 = false;
        bool flag9 = false;
        bool flag10 = false;
        bool flag11 = false;
        bool flag12 = false;
        bool flag13 = false;
        int num2 = 0;
        int num3 = 0;
        //int num4 = 0;
        for (int k = 0; k < 255; k++) {
            if (!Main.player[k].active || Main.player[k].dead)
                continue;

            flag3 = false;
            if (Main.player[k].isNearNPC(398, NPC.MoonLordFightingDistance))
                continue;

            if (!Main.player[k].InModBiome<BackwoodsBiome>()) {
                continue;
            }

            bool flag15 = false;

            int spawnSpaceX = 3;
            int spawnSpaceY = 3;
            if (Main.player[k].active && !Main.player[k].dead) {
                bool flag16 = Main.player[k].ZoneTowerNebula || Main.player[k].ZoneTowerSolar || Main.player[k].ZoneTowerStardust || Main.player[k].ZoneTowerVortex;
                int spawnRangeX = (int)((double)(NPC.sWidth / 16) * 0.7);
                int spawnRangeY = (int)((double)(NPC.sHeight / 16) * 0.7);
                int safeRangeX = (int)((double)(NPC.sWidth / 16) * 0.52);
                int safeRangeY = (int)((double)(NPC.sHeight / 16) * 0.52);

                if (Main.player[k].inventory[Main.player[k].selectedItem].type == 1254 || Main.player[k].inventory[Main.player[k].selectedItem].type == 1299 || Main.player[k].scope) {
                    float num11 = 1.5f;
                    if (Main.player[k].inventory[Main.player[k].selectedItem].type == 1254 && Main.player[k].scope)
                        num11 = 1.25f;
                    else if (Main.player[k].inventory[Main.player[k].selectedItem].type == 1254)
                        num11 = 1.5f;
                    else if (Main.player[k].inventory[Main.player[k].selectedItem].type == 1299)
                        num11 = 1.5f;
                    else if (Main.player[k].scope)
                        num11 = 2f;

                    spawnRangeX += (int)((double)(NPC.sWidth / 16) * 0.5 / (double)num11);
                    spawnRangeY += (int)((double)(NPC.sHeight / 16) * 0.5 / (double)num11);
                    safeRangeX += (int)((double)(NPC.sWidth / 16) * 0.5 / (double)num11);
                    safeRangeY += (int)((double)(NPC.sHeight / 16) * 0.5 / (double)num11);
                }

                NPCLoader.EditSpawnRange(Main.player[k], ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);

                int num12 = (int)(Main.player[k].position.X / 16f) - spawnRangeX;
                int num13 = (int)(Main.player[k].position.X / 16f) + spawnRangeX;
                int num14 = (int)(Main.player[k].position.Y / 16f) - spawnRangeY;
                int num15 = (int)(Main.player[k].position.Y / 16f) + spawnRangeY;
                int num16 = (int)(Main.player[k].position.X / 16f) - safeRangeX;
                int num17 = (int)(Main.player[k].position.X / 16f) + safeRangeX;
                int num18 = (int)(Main.player[k].position.Y / 16f) - safeRangeY;
                int num19 = (int)(Main.player[k].position.Y / 16f) + safeRangeY;
                if (num12 < 0)
                    num12 = 0;

                if (num13 > Main.maxTilesX)
                    num13 = Main.maxTilesX;

                if (num14 < 0)
                    num14 = 0;

                if (num15 > Main.maxTilesY)
                    num15 = Main.maxTilesY;

                for (int m = 0; m < 50; m++) {
                    int num20 = Main.rand.Next(num12, num13);
                    int num21 = Main.rand.Next(num14, num15);
                    if (!Main.tile[num20, num21].HasUnactuatedTile || !Main.tileSolid[Main.tile[num20, num21].TileType]) {
                        if (!flag16 && Main.wallHouse[Main.tile[num20, num21].WallType])
                            continue;

                        if (!flag6 && (double)num21 < Main.worldSurface * 0.3499999940395355 && !flag12 && ((double)num20 < (double)Main.maxTilesX * 0.45 || (double)num20 > (double)Main.maxTilesX * 0.55 || Main.hardMode)) {
                            num3 = Main.tile[num20, num21].TileType;
                            num = num20;
                            num2 = num21;
                            flag2 = true;
                            // Patch note: flag3 - Sky
                            flag3 = true;
                        }
                        else if (!flag6 && (double)num21 < Main.worldSurface * 0.44999998807907104 && !flag12 && Main.hardMode && Main.rand.Next(10) == 0) {
                            num3 = Main.tile[num20, num21].TileType;
                            num = num20;
                            num2 = num21;
                            flag2 = true;
                            flag3 = true;
                        }
                        else {
                            for (int n = num21; n < Main.maxTilesY && n < num15; n++) {
                                if (Main.tile[num20, n].HasUnactuatedTile && Main.tileSolid[Main.tile[num20, n].TileType]) {
                                    if (num20 < num16 || num20 > num17 || n < num18 || n > num19) {
                                        num3 = Main.tile[num20, n].TileType;
                                        num = num20;
                                        num2 = n;
                                        flag2 = true;
                                    }

                                    break;
                                }
                            }
                        }

                        if (Main.player[k].ZoneShadowCandle)
                            flag5 = false;
                        else if (!flag3 && Main.player[k].afkCounter >= NPC.AFKTimeNeededForNoWorms)
                            flag5 = true;

                        if (flag2) {
                            int num22 = num - spawnSpaceX / 2;
                            int num23 = num + spawnSpaceX / 2;
                            int num24 = num2 - spawnSpaceY;
                            int num25 = num2;
                            if (num22 < 0)
                                flag2 = false;

                            if (num23 > Main.maxTilesX)
                                flag2 = false;

                            if (num24 < 0)
                                flag2 = false;

                            if (num25 > Main.maxTilesY)
                                flag2 = false;

                            if (flag2) {
                                for (int num26 = num22; num26 < num23; num26++) {
                                    for (int num27 = num24; num27 < num25; num27++) {
                                        if (Main.tile[num26, num27].HasUnactuatedTile && Main.tileSolid[Main.tile[num26, num27].TileType]) {
                                            flag2 = false;
                                            break;
                                        }
                                        // Extra patch context.

                                        if (Main.tile[num26, num27].LiquidType == LiquidID.Lava) {
                                            flag2 = false;
                                            break;
                                        }
                                        // Extra patch context.
                                    }
                                }
                            }

                            if (num >= num16 && num <= num17)
                                // Patch note: flag15 - safeRangeX
                                flag15 = true;
                        }
                    }

                    if (flag2 || flag2)
                        break;
                }
            }

            if (flag2) {
                Rectangle rectangle = new Rectangle(num * 16, num2 * 16, 16, 16);
                for (int num28 = 0; num28 < 255; num28++) {
                    if (Main.player[num28].active) {
                        Rectangle rectangle2 = new Rectangle((int)(Main.player[num28].position.X + (float)(Main.player[num28].width / 2) - (float)(NPC.sWidth / 2) - (float)NPC.safeRangeX), (int)(Main.player[num28].position.Y + (float)(Main.player[num28].height / 2) - (float)(NPC.sHeight / 2) - (float)NPC.safeRangeY), NPC.sWidth + NPC.safeRangeX * 2, NPC.sHeight + NPC.safeRangeY * 2);
                        if (rectangle.Intersects(rectangle2))
                            flag2 = false;
                    }
                }
            }

            if (flag2) {
                if (Main.player[k].ZoneDungeon && (!Main.tileDungeon[Main.tile[num, num2].TileType] || Main.tile[num, num2 - 1].WallType == 0))
                    flag2 = false;

                if (Main.tile[num, num2 - 1].LiquidAmount > 0 && Main.tile[num, num2 - 2].LiquidAmount > 0 && Main.tile[num, num2 - 1].LiquidType != LiquidID.Lava) {
                    if (Main.tile[num, num2 - 1].LiquidType == LiquidID.Shimmer)
                        flag2 = false;

                    if (Main.tile[num, num2 - 1].LiquidType == LiquidID.Honey)
                        // Patch note: flag8 - Honey
                        flag8 = true;
                    else
                        // Patch note: flag7 - Water
                        flag7 = true;
                }

                int num29 = (int)Main.player[k].Center.X / 16;
                int num30 = (int)(Main.player[k].Bottom.Y + 8f) / 16;

                //spawnInfo.PlayerFloorX = num29;
                //spawnInfo.PlayerFloorY = num30;

                if (Main.tile[num, num2].TileType == 367) {
                    // Patch note: flag10 - Marble
                    flag10 = true;
                }
                else if (Main.tile[num, num2].TileType == 368) {
                    // Patch note: flag9 - Granite
                    flag9 = true;
                }
                else if (Main.tile[num29, num30].TileType == 367) {
                    flag10 = true;
                }
                else if (Main.tile[num29, num30].TileType == 368) {
                    flag9 = true;
                }
                else {
                    int num31 = Main.rand.Next(20, 31);
                    int num32 = Main.rand.Next(1, 4);
                    if (num - num31 < 0)
                        num31 = num;

                    if (num2 - num31 < 0)
                        num31 = num2;

                    if (num + num31 >= Main.maxTilesX)
                        num31 = Main.maxTilesX - num - 1;

                    if (num2 + num31 >= Main.maxTilesY)
                        num31 = Main.maxTilesY - num2 - 1;

                    for (int num33 = num - num31; num33 <= num + num31; num33 += num32) {
                        int num34 = Main.rand.Next(1, 4);
                        for (int num35 = num2 - num31; num35 <= num2 + num31; num35 += num34) {
                            if (Main.tile[num33, num35].TileType == 367)
                                flag10 = true;

                            if (Main.tile[num33, num35].TileType == 368)
                                flag9 = true;
                        }
                    }

                    num31 = Main.rand.Next(30, 61);
                    num32 = Main.rand.Next(3, 7);
                    if (num29 - num31 < 0)
                        num31 = num29;

                    if (num30 - num31 < 0)
                        num31 = num30;

                    if (num29 + num31 >= Main.maxTilesX)
                        num31 = Main.maxTilesX - num29 - 2;

                    if (num30 + num31 >= Main.maxTilesY)
                        num31 = Main.maxTilesY - num30 - 2;

                    for (int num36 = num29 - num31; num36 <= num29 + num31; num36 += num32) {
                        int num37 = Main.rand.Next(3, 7);
                        for (int num38 = num30 - num31; num38 <= num30 + num31; num38 += num37) {
                            if (Main.tile[num36, num38].TileType == 367)
                                flag10 = true;

                            if (Main.tile[num36, num38].TileType == 368)
                                flag9 = true;
                        }
                    }
                }

                if (flag8)
                    flag2 = false;

                if ((num3 == 477 || num3 == 492) && !Main.bloodMoon && !Main.eclipse && Main.invasionType <= 0 && !Main.pumpkinMoon && !Main.snowMoon && !Main.slimeRain && Main.rand.Next(100) < 10)
                    flag2 = false;
            }

            if (!flag2) {
                continue;
            }

            Tile tile = Main.tile[num, num2];
            int num49 = tile.TileType;
            num49 = SpawnNPC_TryFindingProperGroundTileType(num49, num, num2);

            if (tile.LiquidAmount <= 0 &&
                (num49 == ModContent.TileType<BackwoodsGrass>() || num49 == ModContent.TileType<LivingElderwood>() || num49 == ModContent.TileType<BackwoodsGreenMoss>()) &&
                num2 < BackwoodsVars.FirstTileYAtCenter + 15) {
                //Main.LocalPlayer.position = new Vector2(num, num2).ToWorldCoordinates();
                int newNPC = NPC.NewNPC(new EntitySource_SpawnNPC(), num * 16 + 8, num2 * 16, ModContent.NPCType<Hunter>(), 1);
                ShouldSpawnHunter = false;
            }
        }
    }
}
