using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static partial class NPCExtensions {
    public static bool FindBuff(this NPC npc, int type, out int index) {
        index = npc.FindBuffIndex(type);
        return index != -1;
    }

    public static bool HasBuff(this NPC npc, int type) => npc.FindBuffIndex(type) != -1;

    public static void ClearBuff(this NPC npc, int buffType) {
        if (buffType == 0) {
            return;
        }

        for (int i = 0; i < NPC.maxBuffs; i++) {
            if (npc.buffType[i] == buffType) {
                npc.DelBuff(i);
            }
        }
    }

    public static T As<T>(this NPC npc) where T : ModNPC => npc.ModNPC as T;

    public static void PseudoGolemAI(this NPC npc, float maxSpeed = 1f, float speed = 0.035f) {
        npc.aiStyle = 3;
        npc.ModNPC.AIType = 243;
        npc.ai[2] = 0f;
        float num87 = maxSpeed * 3f;
        float num88 = speed;
        num87 += (1f - (float)npc.life / (float)npc.lifeMax) * 1.5f * 0.1f;
        num88 += (1f - (float)npc.life / (float)npc.lifeMax) * 0.15f * 0.1f;
        if (npc.velocity.X < 0f - num87 || npc.velocity.X > num87) {
            if (npc.IsGrounded())
                npc.velocity *= 0.7f;
        }
        else if (npc.velocity.X < num87 && npc.direction == 1) {
            npc.velocity.X += num88;
            if (npc.velocity.X > num87)
                npc.velocity.X = num87;
        }
        else if (npc.velocity.X > 0f - num87 && npc.direction == -1) {
            npc.velocity.X -= num88;
            if (npc.velocity.X < 0f - num87)
                npc.velocity.X = 0f - num87;
        }
        if (npc.IsGrounded()) {
            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        }
    }

    public static void ResetAIStyle(this NPC npc) {
        npc.aiStyle = 0;
        npc.ModNPC.AIType = -1;
    }

    public static bool NearestTheSame(this NPC NPC, out NPC npc2, int type = -1) {
        for (int i = 0; i < Main.npc.Length; i++) {
            NPC npc = Main.npc[i];
            if (i != NPC.whoAmI && npc.active && (npc.type == NPC.type || type == npc.type) && Math.Abs(NPC.position.X - npc.position.X) + Math.Abs(NPC.position.Y - npc.position.Y) < NPC.width) {
                npc2 = npc;
                return true;
            }
        }
        npc2 = null;
        return false;
    }

    public static void OffsetNPC(this NPC NPC, NPC npc, float offsetSpeed = 0.05f) {
        if (NPC.position.X < npc.position.X) {
            NPC.velocity.X -= offsetSpeed;
        }
        else {
            NPC.velocity.X += offsetSpeed;
        }
        if (NPC.position.Y < npc.position.Y) {
            NPC.velocity.Y -= offsetSpeed;
        }
        else {
            NPC.velocity.Y += offsetSpeed;
        }
    }

    public static void OffsetTheSameNPC(this NPC npc1, float offsetSpeed = 0.05f) {
        if (npc1.NearestTheSame(out NPC npc2)) {
            npc1.OffsetNPC(npc2, offsetSpeed);
        }
    }

    public static void KillNPC(this NPC npc) {
        npc.active = false;
        npc.life = -1;
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI, 0f, 0f, 0f, 0, 0, 0);
        }
    }

    public static void ApplyAdvancedFlierAI(this NPC npc) {
        npc.noGravity = true;
        if (!Main.player[npc.target].dead) {
            if (npc.collideX) {
                npc.velocity.X = npc.oldVelocity.X * -0.5f;
                if (npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
                    npc.velocity.X = 2f;

                if (npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
                    npc.velocity.X = -2f;
            }

            if (npc.collideY) {
                npc.velocity.Y = npc.oldVelocity.Y * -0.5f;
                if (npc.velocity.Y > 0f && npc.velocity.Y < 1f)
                    npc.velocity.Y = 1f;

                if (npc.velocity.Y < 0f && npc.velocity.Y > -1f)
                    npc.velocity.Y = -1f;
            }

            npc.TargetClosest();
            if (npc.direction == -1 && npc.velocity.X > -3f) {
                npc.velocity.X -= 0.075f;
                if (npc.velocity.X > 2.75f)
                    npc.velocity.X -= 0.075f;
                else if (npc.velocity.X > 0f)
                    npc.velocity.X -= 0.035f;

                if (npc.velocity.X < -2.75f)
                    npc.velocity.X = -2.75f;
            }
            else if (npc.direction == 1 && npc.velocity.X < 3f) {
                npc.velocity.X += 0.075f;
                if (npc.velocity.X < -2.75f)
                    npc.velocity.X += 0.075f;
                else if (npc.velocity.X < 0f)
                    npc.velocity.X += 0.035f;

                if (npc.velocity.X > 2.75f)
                    npc.velocity.X = 2.75f;
            }

            float num269 = Math.Abs(npc.position.X + (float)(npc.width / 2) - (Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2)));
            float num270 = Main.player[npc.target].position.Y - (float)(npc.height / 2);
            if (num269 > 50f)
                num270 -= 100f;

            if (npc.position.Y < num270) {
                npc.velocity.Y += 0.035f;
                if (npc.velocity.Y < 0f)
                    npc.velocity.Y += 0.01f;
            }
            else {
                npc.velocity.Y -= 0.035f;
                if (npc.velocity.Y > 0f)
                    npc.velocity.Y -= 0.01f;
            }

            if (npc.velocity.Y < -2.75f)
                npc.velocity.Y = -2.75f;

            if (npc.velocity.Y > 2.75f)
                npc.velocity.Y = 2.75f;
        }
        else {
            npc.TargetClosest();
        }

        if (npc.wet) {
            if (npc.velocity.Y > 0f)
                npc.velocity.Y *= 0.95f;

            npc.velocity.Y -= 0.25f;
            if (npc.velocity.Y < -3f)
                npc.velocity.Y = -3f;

            npc.TargetClosest();
        }
    }

    public static void LegacyFighterAI(this NPC npc, bool backwoods = false, Action<NPC>? movementX = null, bool isWiderNPC = false, bool shouldOpenDoors = false, bool knocksOnDoors = false) {
        NPC NPC = npc;
        npc.aiStyle = -1;
        int targetDelay = 60;
        int npcTypeForSomeReason = NPC.type;

        NPC.TargetClosest(faceTarget: true);

        movementX(npc);

        bool tileChecks = false;
        if (NPC.IsGrounded()) {
            int num77 = (int)(NPC.position.Y + NPC.height + 7f) / 16;
            int num189 = (int)NPC.position.X / 16;
            int num79 = (int)(NPC.position.X + NPC.width) / 16;
            for (int num80 = num189; num80 <= num79; num80++) {
                if (Main.tile[num80, num77] == null) {
                    return;
                }

                if (Main.tile[num80, num77].HasUnactuatedTile && Main.tileSolid[Main.tile[num80, num77].TileType]) {
                    tileChecks = true;
                    break;
                }
            }
        }
        if (NPC.velocity.Y >= 0f) {
            int direction = Math.Sign(NPC.velocity.X);

            Vector2 position3 = NPC.position;
            position3.X += NPC.velocity.X;
            int num82 = (int)((position3.X + NPC.width / 2 + (NPC.width / 2 + 1) * direction) / 16f);
            int num83 = (int)((position3.Y + NPC.height - 1f) / 16f);
            if (num82 * 16 < position3.X + NPC.width && num82 * 16 + 16 > position3.X && (Main.tile[num82, num83].HasUnactuatedTile && !Main.tile[num82, num83].TopSlope && !Main.tile[num82, num83 - 1].TopSlope && Main.tileSolid[Main.tile[num82, num83].TileType] && !Main.tileSolidTop[Main.tile[num82, num83].TileType] || Main.tile[num82, num83 - 1].IsHalfBlock && Main.tile[num82, num83 - 1].HasUnactuatedTile) && (!Main.tile[num82, num83 - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 1].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 1].TileType] || Main.tile[num82, num83 - 1].IsHalfBlock && (!Main.tile[num82, num83 - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 4].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 4].TileType])) && (!Main.tile[num82, num83 - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 2].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 2].TileType]) && (!Main.tile[num82, num83 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 3].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 3].TileType]) && (!Main.tile[num82 - direction, num83 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82 - direction, num83 - 3].TileType])) {
                float num84 = num83 * 16;
                if (Main.tile[num82, num83].IsHalfBlock) {
                    num84 += 8f;
                }

                if (Main.tile[num82, num83 - 1].IsHalfBlock) {
                    num84 -= 8f;
                }
                if (num84 < position3.Y + NPC.height) {
                    float num85 = position3.Y + NPC.height - num84;
                    float num86 = 16.1f;
                    if (NPC.type == NPCID.BlackRecluse || NPC.type == NPCID.WallCreeper || NPC.type == NPCID.JungleCreeper || NPC.type == NPCID.BloodCrawler || NPC.type == NPCID.DesertScorpionWalk) {
                        num86 += 8f;
                    }

                    if (num85 <= num86) {
                        NPC.gfxOffY += NPC.position.Y + NPC.height - num84;
                        NPC.position.Y = num84 - NPC.height;
                        if (num85 < 9f) {
                            NPC.stepSpeed = 1f;
                        }
                        else {
                            NPC.stepSpeed = 2f;
                        }
                    }
                }
            }
        }
        if (tileChecks && !Main.tile[(int)(NPC.Center.X) / 16, (int)(NPC.Center.Y - 15f) / 16 - 1].HasUnactuatedTile) {
            int tileX = (int)((NPC.position.X + NPC.width / 2 + 15 * NPC.direction) / 16f);
            int tileY = (int)((NPC.position.Y + NPC.height - 15f) / 16f);

            if (isWiderNPC) {
                tileX = (int)((NPC.position.X + NPC.width / 2 + (NPC.width / 2 + 16) * NPC.direction) / 16f);
            }

            if (knocksOnDoors && Main.tile[tileX, tileY - 1].HasUnactuatedTile && (TileLoader.IsClosedDoor(Main.tile[tileX, tileY - 1]) || Main.tile[tileX, tileY - 1].TileType == 388)) {
                NPC.ai[2] += 1f;
                NPC.ai[3] = 0f;
                if (NPC.ai[2] >= 60f) {
                    NPC.velocity.X = 0.5f * -NPC.direction;
                    NPC.ai[2] = 0f;
                    bool opensDoors = shouldOpenDoors;
                    WorldGen.KillTile(tileX, tileY - 1, fail: true);
                    if (opensDoors && Main.netMode != NetmodeID.MultiplayerClient) {
                        if (TileLoader.OpenDoorID(Main.tile[tileX, tileY - 1]) >= 0) {
                            bool openedDoor = WorldGen.OpenDoor(tileX, tileY - 1, NPC.direction);
                            if (!openedDoor) {
                                NPC.ai[3] = targetDelay;
                                NPC.netUpdate = true;
                            }
                            if (Main.netMode == NetmodeID.Server && openedDoor) {
                                NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, tileX, tileY - 1, NPC.direction);
                            }
                        }
                        if (Main.tile[tileX, tileY - 1].TileType == 388) {
                            bool openedTallGate = WorldGen.ShiftTallGate(tileX, tileY - 1, closing: false);
                            if (!openedTallGate) {
                                NPC.ai[3] = targetDelay;
                                NPC.netUpdate = true;
                            }
                            if (Main.netMode == NetmodeID.Server && openedTallGate) {
                                NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 4, tileX, tileY - 1);
                            }
                        }
                    }
                }
            }
            else {
                if (NPC.velocity.X < 0f && NPC.direction == -1 || NPC.velocity.X > 0f && NPC.direction == 1) {
                    void jumpIfPlayerAboveAndClose() {
                        if (NPC.IsGrounded() && Main.expertMode && Main.player[npc.target].Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) < (float)(Main.player[npc.target].width * 3) && Collision.CanHit(npc, Main.player[npc.target])) {
                            if (NPC.IsGrounded()) {
                                int num200 = 6;
                                if (Main.player[npc.target].Bottom.Y > npc.Top.Y - (float)(num200 * 16)) {
                                    npc.velocity.Y = -7.9f;
                                }
                                else {
                                    int num201 = (int)(npc.Center.X / 16f);
                                    int num202 = (int)(npc.Bottom.Y / 16f) - 1;
                                    for (int num203 = num202; num203 > num202 - num200; num203--) {
                                        if (Main.tile[num201, num203].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[num201, num203].TileType]) {
                                            npc.velocity.Y = -7.9f;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    jumpIfPlayerAboveAndClose();

                    bool JumpCheck(int tileX, int tileY) {
                        if (NPC.height >= 32 && Main.tile[tileX, tileY - 2].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 2].TileType]) {
                            if (Main.tile[tileX, tileY - 3].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 3].TileType]) {
                                NPC.velocity.Y = -8f;
                                NPC.netUpdate = true;
                            }
                            else {
                                NPC.velocity.Y = -7f;
                                NPC.netUpdate = true;
                            }
                            return true;
                        }
                        else if (Main.tile[tileX, tileY - 1].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 1].TileType]) {
                            NPC.velocity.Y = -6f;
                            NPC.netUpdate = true;
                            return true;
                        }
                        else if (NPC.position.Y + NPC.height - tileY * 16 > 20f && Main.tile[tileX, tileY].HasUnactuatedTile && !Main.tile[tileX, tileY].TopSlope && Main.tileSolid[Main.tile[tileX, tileY].TileType]) {
                            NPC.velocity.Y = -5f;
                            NPC.netUpdate = true;
                            return true;
                        }
                        else if (NPC.directionY < 0 && (!Main.tile[tileX, tileY + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY + 1].TileType]) && (!Main.tile[tileX + NPC.direction, tileY + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX + NPC.direction, tileY + 1].TileType])) {
                            NPC.velocity.Y = -8f;
                            NPC.velocity.X *= 1.5f;
                            NPC.netUpdate = true;
                            return true;
                        }
                        return false;
                    }
                    if (!JumpCheck(tileX, tileY)) {
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                    }
                }
            }
        }
        else if (knocksOnDoors) {
            NPC.ai[1] = 0f;
            NPC.ai[2] = 0f;
        }
    }

    public static void ApplyFighterAI(this NPC npc, bool backwoods, bool targetPlayer = true, bool targetPlayer2 = false, Action<NPC>? movementX = null, bool isWiderNPC = false, bool shouldOpenDoors = false, bool knocksOnDoors = false, int targetDelay = 60) {
        npc.aiStyle = -1;
        npc.ModNPC.AIType = -1;

        if (Main.player[npc.target].position.Y + (float)Main.player[npc.target].height == npc.position.Y + (float)npc.height)
            npc.directionY = -1;

        bool flag = false;

        bool canOpenDoor2 = false;
        bool flag6 = false;
        if (npc.velocity.X == 0f)
            flag6 = true;

        if (npc.justHit)
            flag6 = false;

        flag6 = false;

        int num56 = targetDelay;

        bool flag7 = false;
        bool canOpenDoor = true;
        //if (npc.type == 343 || npc.type == 47 || npc.type == 67 || npc.type == 109 || npc.type == 110 || npc.type == 111 || npc.type == 120 || npc.type == 163 || npc.type == 164 || npc.type == 239 || npc.type == 168 || npc.type == 199 || npc.type == 206 || npc.type == 214 || npc.type == 215 || npc.type == 216 || npc.type == 217 || npc.type == 218 || npc.type == 219 || npc.type == 220 || npc.type == 226 || npc.type == 243 || npc.type == 251 || npc.type == 257 || npc.type == 258 || npc.type == 290 || npc.type == 291 || npc.type == 292 || npc.type == 293 || npc.type == 305 || npc.type == 306 || npc.type == 307 || npc.type == 308 || npc.type == 309 || npc.type == 348 || npc.type == 349 || npc.type == 350 || npc.type == 351 || npc.type == 379 || (npc.type >= 430 && npc.type <= 436) || npc.type == 591 || npc.type == 380 || npc.type == 381 || npc.type == 382 || npc.type == 383 || npc.type == 386 || npc.type == 391 || (npc.type >= 449 && npc.type <= 452) || npc.type == 466 || npc.type == 464 || npc.type == 166 || npc.type == 469 || npc.type == 468 || npc.type == 471 || npc.type == 470 || npc.type == 480 || npc.type == 481 || npc.type == 482 || npc.type == 411 || npc.type == 424 || npc.type == 409 || (npc.type >= 494 && npc.type <= 506) || npc.type == 425 || npc.type == 427 || npc.type == 426 || npc.type == 428 || npc.type == 580 || npc.type == 508 || npc.type == 415 || npc.type == 419 || npc.type == 520 || (npc.type >= 524 && npc.type <= 527) || npc.type == 528 || npc.type == 529 || npc.type == 530 || npc.type == 532 || npc.type == 582 || npc.type == 624 || npc.type == 631)
        //    flag8 = false;
        //flag8 = false;

        bool flag9 = false;
        int num64 = npc.type;
        //if (num64 == 425 || num64 == 471)
        //    flag9 = true;

        bool flag10 = true;
        switch (npc.type) {
            case 110:
            case 111:
            case 206:
            case 214:
            case 215:
            case 216:
            case 291:
            case 292:
            case 293:
            case 350:
            case 379:
            case 380:
            case 381:
            case 382:
            case 409:
            case 411:
            case 424:
            case 426:
            case 466:
            case 498:
            case 499:
            case 500:
            case 501:
            case 502:
            case 503:
            case 504:
            case 505:
            case 506:
            case 520:
                if (npc.ai[2] > 0f)
                    flag10 = false;
                break;
        }

        if (!flag9 && flag10) {
            if (npc.IsGrounded() && ((npc.velocity.X > 0f && npc.direction < 0) || (npc.velocity.X < 0f && npc.direction > 0)))
                flag7 = true;

            if (npc.position.X == npc.oldPosition.X || npc.ai[3] >= (float)num56 || flag7)
                npc.ai[3] += 1f;
            else if ((double)Math.Abs(npc.velocity.X) > 0.9 && npc.ai[3] > 0f)
                npc.ai[3] -= 1f;

            if (npc.ai[3] > (float)(num56 * 10))
                npc.ai[3] = 0f;

            if (npc.justHit)
                npc.ai[3] = 0f;

            if (npc.ai[3] == (float)num56)
                npc.netUpdate = true;

            if (Main.player[npc.target].Hitbox.Intersects(npc.Hitbox))
                npc.ai[3] = 0f;
        }

        bool shouldTargetPlayer = Terraria.NPC.DespawnEncouragement_AIStyle3_Fighters_NotDiscouraged(npc.type, npc.position, npc);

        bool flag13 = (Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].InModBiome<BackwoodsBiome>() || npc.life < (int)(npc.lifeMax * 0.8f) || targetPlayer2) && targetPlayer;
        if (backwoods) {
            shouldTargetPlayer = flag13;
        }
        else {
            shouldTargetPlayer = targetPlayer;
        }
        //else {
        //    shouldDespawn = false;
        //}
        if (npc.ai[3] < (float)num56 && shouldTargetPlayer) {
            //if (npc.shimmerTransparency < 1f) {
            //    //if ((npc.type == 3 || npc.type == 591 || npc.type == 590 || npc.type == 331 || npc.type == 332 || npc.type == 21 || (npc.type >= 449 && npc.type <= 452) || npc.type == 31 || npc.type == 294 || npc.type == 295 || npc.type == 296 || npc.type == 77 || npc.type == 110 || npc.type == 132 || npc.type == 167 || npc.type == 161 || npc.type == 162 || npc.type == 186 || npc.type == 187 || npc.type == 188 || npc.type == 189 || npc.type == 197 || npc.type == 200 || npc.type == 201 || npc.type == 202 || npc.type == 203 || npc.type == 223 || npc.type == 291 || npc.type == 292 || npc.type == 293 || npc.type == 320 || npc.type == 321 || npc.type == 319 || npc.type == 481 || npc.type == 632 || npc.type == 635) && Main.rand.Next(1000) == 0)
            //    //    SoundEngine.PlaySound(14, (int)npc.position.X, (int)position.Y);

            //    //if ((npc.type == 489 || npc.type == 586) && Main.rand.Next(800) == 0)
            //    //    SoundEngine.PlaySound(14, (int)position.X, (int)position.Y, npc.type);

            //    //if ((npc.type == 78 || npc.type == 79 || npc.type == 80 || npc.type == 630) && Main.rand.Next(500) == 0)
            //    //    SoundEngine.PlaySound(26, (int)position.X, (int)position.Y);

            //    //if (npc.type == 159 && Main.rand.Next(500) == 0)
            //    //    SoundEngine.PlaySound(29, (int)position.X, (int)position.Y, 7);

            //    //if (npc.type == 162 && Main.rand.Next(500) == 0)
            //    //    SoundEngine.PlaySound(29, (int)position.X, (int)position.Y, 6);

            //    //if (npc.type == 181 && Main.rand.Next(500) == 0)
            //    //    SoundEngine.PlaySound(29, (int)position.X, (int)position.Y, 8);

            //    //if (npc.type >= 269 && npc.type <= 280 && Main.rand.Next(1000) == 0)
            //    //    SoundEngine.PlaySound(14, (int)position.X, (int)position.Y);
            //}

            npc.TargetClosest();
            if (npc.directionY > 0 && Main.player[npc.target].Center.Y <= npc.Bottom.Y)
                npc.directionY = -1;
        }
        else if (!(npc.ai[2] > 0f) || !Terraria.NPC.DespawnEncouragement_AIStyle3_Fighters_CanBeBusyWithAction(npc.type)) {
            bool flag12 = backwoods && targetPlayer/*Main.player[npc.target].InModBiome<BackwoodsBiome>()*/;
            if (!flag12 && (double)(npc.position.Y / 16f) < Main.worldSurface/* && npc.type != 624 && npc.type != 631*/) {
                npc.EncourageDespawn(10);
            }

            if (npc.velocity.X == 0f) {
                if (npc.IsGrounded()) {
                    npc.ai[0] += 1f;
                    if (npc.ai[0] >= 2f) {
                        npc.direction *= -1;
                        npc.spriteDirection = npc.direction;
                        npc.ai[0] = 0f;
                    }
                }
            }
            else {
                npc.ai[0] = 0f;
            }

            if (npc.direction == 0)
                npc.direction = 1;
        }

        if (movementX == null) {
            float num87 = 1f;
            float num88 = 0.07f;
            //num87 += (1f - (float)life / (float)lifeMax) * 1.5f;
            //num88 += (1f - (float)life / (float)lifeMax) * 0.15f;
            if (npc.velocity.X < 0f - num87 || npc.velocity.X > num87) {
                if (npc.IsGrounded())
                    npc.velocity *= 0.7f;
            }
            else if (npc.velocity.X < num87 && npc.direction == 1) {
                npc.velocity.X += num88;
                if (npc.velocity.X > num87)
                    npc.velocity.X = num87;
            }
            else if (npc.velocity.X > 0f - num87 && npc.direction == -1) {
                npc.velocity.X -= num88;
                if (npc.velocity.X < 0f - num87)
                    npc.velocity.X = 0f - num87;
            }
        }
        else {
            movementX.Invoke(npc);
        }

        NPC NPC = npc;

        bool tileChecks = false;
        if (NPC.IsGrounded()) {
            int num77 = (int)(NPC.position.Y + NPC.height + 7f) / 16;
            int num189 = (int)NPC.position.X / 16;
            int num79 = (int)(NPC.position.X + NPC.width) / 16;
            for (int num80 = num189; num80 <= num79; num80++) {
                if (Main.tile[num80, num77] == null) {
                    return;
                }

                if (Main.tile[num80, num77].HasUnactuatedTile && Main.tileSolid[Main.tile[num80, num77].TileType]) {
                    tileChecks = true;
                    break;
                }
            }
        }
        if (NPC.velocity.Y >= 0f) {
            int direction = Math.Sign(NPC.velocity.X);

            Vector2 position3 = NPC.position;
            position3.X += NPC.velocity.X;
            int num82 = (int)((position3.X + NPC.width / 2 + (NPC.width / 2 + 1) * direction) / 16f);
            int num83 = (int)((position3.Y + NPC.height - 1f) / 16f);
            if (num82 * 16 < position3.X + NPC.width && num82 * 16 + 16 > position3.X && (Main.tile[num82, num83].HasUnactuatedTile && !Main.tile[num82, num83].TopSlope && !Main.tile[num82, num83 - 1].TopSlope && Main.tileSolid[Main.tile[num82, num83].TileType] && !Main.tileSolidTop[Main.tile[num82, num83].TileType] || Main.tile[num82, num83 - 1].IsHalfBlock && Main.tile[num82, num83 - 1].HasUnactuatedTile) && (!Main.tile[num82, num83 - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 1].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 1].TileType] || Main.tile[num82, num83 - 1].IsHalfBlock && (!Main.tile[num82, num83 - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 4].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 4].TileType])) && (!Main.tile[num82, num83 - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 2].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 2].TileType]) && (!Main.tile[num82, num83 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 3].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 3].TileType]) && (!Main.tile[num82 - direction, num83 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82 - direction, num83 - 3].TileType])) {
                float num84 = num83 * 16;
                if (Main.tile[num82, num83].IsHalfBlock) {
                    num84 += 8f;
                }

                if (Main.tile[num82, num83 - 1].IsHalfBlock) {
                    num84 -= 8f;
                }
                if (num84 < position3.Y + NPC.height) {
                    float num85 = position3.Y + NPC.height - num84;
                    float num86 = 16.1f;
                    if (NPC.type == NPCID.BlackRecluse || NPC.type == NPCID.WallCreeper || NPC.type == NPCID.JungleCreeper || NPC.type == NPCID.BloodCrawler || NPC.type == NPCID.DesertScorpionWalk) {
                        num86 += 8f;
                    }

                    if (num85 <= num86) {
                        NPC.gfxOffY += NPC.position.Y + NPC.height - num84;
                        NPC.position.Y = num84 - NPC.height;
                        if (num85 < 9f) {
                            NPC.stepSpeed = 1f;
                        }
                        else {
                            NPC.stepSpeed = 2f;
                        }
                    }
                }
            }
        }
        if (tileChecks && !Main.tile[(int)(NPC.Center.X) / 16, (int)(NPC.Center.Y - 15f) / 16 - 1].HasUnactuatedTile) {
            int tileX = (int)((NPC.position.X + NPC.width / 2 + 15 * NPC.direction) / 16f);
            int tileY = (int)((NPC.position.Y + NPC.height - 15f) / 16f);

            if (isWiderNPC) {
                tileX = (int)((NPC.position.X + NPC.width / 2 + (NPC.width / 2 + 16) * NPC.direction) / 16f);
            }

            if (knocksOnDoors && Main.tile[tileX, tileY - 1].HasUnactuatedTile && (TileLoader.IsClosedDoor(Main.tile[tileX, tileY - 1]) || Main.tile[tileX, tileY - 1].TileType == 388)) {
                NPC.ai[2] += 1f;
                NPC.ai[3] = 0f;
                if (NPC.ai[2] >= 60f) {
                    NPC.velocity.X = 0.5f * -NPC.direction;
                    NPC.ai[2] = 0f;
                    bool opensDoors = shouldOpenDoors;
                    WorldGen.KillTile(tileX, tileY - 1, fail: true);
                    if (opensDoors && Main.netMode != NetmodeID.MultiplayerClient) {
                        if (TileLoader.OpenDoorID(Main.tile[tileX, tileY - 1]) >= 0) {
                            bool openedDoor = WorldGen.OpenDoor(tileX, tileY - 1, NPC.direction);
                            if (!openedDoor) {
                                NPC.ai[3] = targetDelay;
                                NPC.netUpdate = true;
                            }
                            if (Main.netMode == NetmodeID.Server && openedDoor) {
                                NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 0, tileX, tileY - 1, NPC.direction);
                            }
                        }
                        if (Main.tile[tileX, tileY - 1].TileType == 388) {
                            bool openedTallGate = WorldGen.ShiftTallGate(tileX, tileY - 1, closing: false);
                            if (!openedTallGate) {
                                NPC.ai[3] = targetDelay;
                                NPC.netUpdate = true;
                            }
                            if (Main.netMode == NetmodeID.Server && openedTallGate) {
                                NetMessage.SendData(MessageID.ToggleDoorState, -1, -1, null, 4, tileX, tileY - 1);
                            }
                        }
                    }
                }
            }
            else {
                if (NPC.velocity.X < 0f && NPC.direction == -1 || NPC.velocity.X > 0f && NPC.direction == 1) {
                    void jumpIfPlayerAboveAndClose() {
                        if (NPC.IsGrounded() && Main.expertMode && Main.player[npc.target].Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) < (float)(Main.player[npc.target].width * 3) && Collision.CanHit(npc, Main.player[npc.target])) {
                            if (NPC.IsGrounded()) {
                                int num200 = 6;
                                if (Main.player[npc.target].Bottom.Y > npc.Top.Y - (float)(num200 * 16)) {
                                    npc.velocity.Y = -7.9f;
                                }
                                else {
                                    int num201 = (int)(npc.Center.X / 16f);
                                    int num202 = (int)(npc.Bottom.Y / 16f) - 1;
                                    for (int num203 = num202; num203 > num202 - num200; num203--) {
                                        if (Main.tile[num201, num203].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[num201, num203].TileType]) {
                                            npc.velocity.Y = -7.9f;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    jumpIfPlayerAboveAndClose();

                    bool JumpCheck(int tileX, int tileY) {
                        if (NPC.height >= 32 && Main.tile[tileX, tileY - 2].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 2].TileType]) {
                            if (Main.tile[tileX, tileY - 3].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 3].TileType]) {
                                NPC.velocity.Y = -8f;
                                NPC.netUpdate = true;
                            }
                            else {
                                NPC.velocity.Y = -7f;
                                NPC.netUpdate = true;
                            }
                            return true;
                        }
                        else if (Main.tile[tileX, tileY - 1].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 1].TileType]) {
                            NPC.velocity.Y = -6f;
                            NPC.netUpdate = true;
                            return true;
                        }
                        else if (NPC.position.Y + NPC.height - tileY * 16 > 20f && Main.tile[tileX, tileY].HasUnactuatedTile && !Main.tile[tileX, tileY].TopSlope && Main.tileSolid[Main.tile[tileX, tileY].TileType]) {
                            NPC.velocity.Y = -5f;
                            NPC.netUpdate = true;

                            return true;
                        }
                        else if (NPC.directionY < 0 && (!Main.tile[tileX, tileY + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY + 1].TileType]) && (!Main.tile[tileX + NPC.direction, tileY + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX + NPC.direction, tileY + 1].TileType])) {
                            NPC.velocity.Y = -8f;
                            NPC.velocity.X *= 1.5f;
                            NPC.netUpdate = true;
                            return true;
                        }
                        return false;
                    }
                    if (!JumpCheck(tileX, tileY)) {
                        //NPC.ai[1] = 0f;
                        //NPC.ai[2] = 0f;
                    }
                }
            }
        }
        else if (knocksOnDoors) {
            //NPC.ai[1] = 0f;
            //NPC.ai[2] = 0f;
        }

        if (npc.IsGrounded()) {
            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        }

        //if (Main.netMode != 1 && npc.type == 120 && npc.ai[3] >= (float)num56) {
        //    int targetTileX = (int)Main.player[target].Center.X / 16;
        //    int targetTileY = (int)Main.player[target].Center.Y / 16;
        //    Vector2 chosenTile = Vector2.Zero;
        //    if (AI_AttemptToFindTeleportSpot(ref chosenTile, targetTileX, targetTileY, 20, 9)) {
        //        position.X = chosenTile.X * 16f - (float)(width / 2);
        //        position.Y = chosenTile.Y * 16f - (float)height;
        //        npc.ai[3] = -120f;
        //        netUpdate = true;
        //    }
        //}
    }
}
