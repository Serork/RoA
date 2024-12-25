using Humanizer;

using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Tiles.Platforms;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static class NPCExtensions {
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
            if (npc.velocity.Y == 0f)
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
        Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
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

    public static void ApplyFighterAI(this NPC npc, bool backwoods = false, Action<NPC>? movementX = null, bool ignoreBranches = false) {
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

        int num56 = 60;

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
            if (npc.velocity.Y == 0f && ((npc.velocity.X > 0f && npc.direction < 0) || (npc.velocity.X < 0f && npc.direction > 0)))
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

        bool shouldTargetPlayer = NPC.DespawnEncouragement_AIStyle3_Fighters_NotDiscouraged(npc.type, npc.position, npc);

        Player? player = null;
        float dist = float.MaxValue;
        for (int i = 0; i < 255; i++) {
            if (Main.player[i].active && !Main.player[i].dead && !Main.player[i].ghost && Main.player[i].Distance(npc.Center) < dist) {
                dist = Main.player[i].Distance(npc.Center);
                player = Main.player[i];
            }
        }

        bool flag13 = player != null && player.InModBiome<BackwoodsBiome>();
        if (backwoods) {
            shouldTargetPlayer = flag13;
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
        else if (!(npc.ai[2] > 0f) || !NPC.DespawnEncouragement_AIStyle3_Fighters_CanBeBusyWithAction(npc.type)) {
            bool flag12 = backwoods && flag13/*Main.player[npc.target].InModBiome<BackwoodsBiome>()*/;
            if (!flag12 && (double)(npc.position.Y / 16f) < Main.worldSurface/* && npc.type != 624 && npc.type != 631*/) {
                npc.EncourageDespawn(10);
            }

            if (npc.velocity.X == 0f) {
                if (npc.velocity.Y == 0f) {
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
                if (npc.velocity.Y == 0f)
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

        if (npc.velocity.Y == 0f || flag) {
            int num181 = (int)(npc.position.Y + (float)npc.height + 7f) / 16;
            int num182 = (int)(npc.position.Y - 9f) / 16;
            int num183 = (int)npc.position.X / 16;
            int num184 = (int)(npc.position.X + (float)npc.width) / 16;
            int num185 = (int)(npc.position.X + 8f) / 16;
            int num186 = (int)(npc.position.X + (float)npc.width - 8f) / 16;
            bool flag22 = false;
            for (int num187 = num185; num187 <= num186; num187++) {
                if (num187 >= num183 && num187 <= num184 && Main.tile[num187, num181] == null) {
                    flag22 = true;
                    continue;
                }

                if (Main.tile[num187, num182].HasUnactuatedTile && Main.tileSolid[Main.tile[num187, num182].TileType]) {
                    canOpenDoor2 = false;
                    break;
                }

                if (!flag22 && num187 >= num183 && num187 <= num184 && Main.tile[num187, num181].HasUnactuatedTile && Main.tileSolid[Main.tile[num187, num181].TileType])
                    canOpenDoor2 = true;
            }

            if (!canOpenDoor2 && npc.velocity.Y < 0f)
                npc.velocity.Y = 0f;

            if (flag22)
                return;
        }

        if (npc.velocity.Y >= 0f/* && npc.directionY != 1*/) {
            int num188 = 0;
            if (npc.velocity.X < 0f)
                num188 = -1;

            if (npc.velocity.X > 0f)
                num188 = 1;

            Vector2 vector39 = npc.position;
            vector39.X += npc.velocity.X;
            int num189 = (int)((vector39.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 1) * num188)) / 16f);
            int num190 = (int)((vector39.Y + (float)npc.height - 1f) / 16f);
            if (WorldGen.InWorld(num189, num190, 4)) {
                //if (Main.tile[num189, num190] == null)
                //    Main.tile[num189, num190] = new Tile();

                //if (Main.tile[num189, num190 - 1] == null)
                //    Main.tile[num189, num190 - 1] = new Tile();

                //if (Main.tile[num189, num190 - 2] == null)
                //    Main.tile[num189, num190 - 2] = new Tile();

                //if (Main.tile[num189, num190 - 3] == null)
                //    Main.tile[num189, num190 - 3] = new Tile();

                //if (Main.tile[num189, num190 + 1] == null)
                //    Main.tile[num189, num190 + 1] = new Tile();

                //if (Main.tile[num189 - num188, num190 - 3] == null)
                //    Main.tile[num189 - num188, num190 - 3] = new Tile();

                if ((float)(num189 * 16) < vector39.X + (float)npc.width && (float)(num189 * 16 + 16) > vector39.X && ((Main.tile[num189, num190].HasUnactuatedTile && !Main.tile[num189, num190].TopSlope && !Main.tile[num189, num190 - 1].TopSlope && Main.tileSolid[Main.tile[num189, num190].TileType] && !Main.tileSolidTop[Main.tile[num189, num190].TileType]) || (Main.tile[num189, num190 - 1].IsHalfBlock && Main.tile[num189, num190 - 1].HasUnactuatedTile)) && (!Main.tile[num189, num190 - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num189, num190 - 1].TileType] || Main.tileSolidTop[Main.tile[num189, num190 - 1].TileType] || (Main.tile[num189, num190 - 1].IsHalfBlock && (!Main.tile[num189, num190 - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[num189, num190 - 4].TileType] || Main.tileSolidTop[Main.tile[num189, num190 - 4].TileType]))) && (!Main.tile[num189, num190 - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[num189, num190 - 2].TileType] || Main.tileSolidTop[Main.tile[num189, num190 - 2].TileType]) && (!Main.tile[num189, num190 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num189, num190 - 3].TileType] || Main.tileSolidTop[Main.tile[num189, num190 - 3].TileType]) && (!Main.tile[num189 - num188, num190 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num189 - num188, num190 - 3].TileType])) {
                    float num191 = num190 * 16;
                    if (Main.tile[num189, num190].IsHalfBlock)
                        num191 += 8f;

                    if (Main.tile[num189, num190 - 1].IsHalfBlock)
                        num191 -= 8f;

                    if (num191 < vector39.Y + (float)npc.height) {
                        float num192 = vector39.Y + (float)npc.height - num191;
                        float num193 = 16.1f;
                        if (npc.type == 163 || npc.type == 164 || npc.type == 236 || npc.type == 239 || npc.type == 530)
                            num193 += 8f;

                        if (num192 <= num193) {
                            npc.gfxOffY += npc.position.Y + (float)npc.height - num191;
                            npc.position.Y = num191 - (float)npc.height;
                            if (num192 < 9f)
                                npc.stepSpeed = 1f;
                            else
                                npc.stepSpeed = 2f;
                        }
                    }
                }
            }
        }

        if (canOpenDoor2) {
            int num194 = (int)((npc.position.X + (float)(npc.width / 2) + (float)(/*15*/npc.width / 2 * npc.direction)) / 16f);
            int num195 = (int)((npc.position.Y + (float)npc.height - 15f) / 16f);
            //if (npc.type == 109 || npc.type == 163 || npc.type == 164 || npc.type == 199 || npc.type == 236 || npc.type == 239 || npc.type == 257 || npc.type == 258 || npc.type == 290 || npc.type == 391 || npc.type == 425 || npc.type == 427 || npc.type == 426 || npc.type == 580 || npc.type == 508 || npc.type == 415 || npc.type == 530 || npc.type == 532 || npc.type == 582)
                num194 = (int)((npc.position.X + (float)(npc.width / 2) + (float)((npc.width / 2 + 16) * npc.direction)) / 16f);

            //if (Main.tile[num194, num195] == null)
            //    Main.tile[num194, num195] = new Tile();

            //if (Main.tile[num194, num195 - 1] == null)
            //    Main.tile[num194, num195 - 1] = new Tile();

            //if (Main.tile[num194, num195 - 2] == null)
            //    Main.tile[num194, num195 - 2] = new Tile();

            //if (Main.tile[num194, num195 - 3] == null)
            //    Main.tile[num194, num195 - 3] = new Tile();

            //if (Main.tile[num194, num195 + 1] == null)
            //    Main.tile[num194, num195 + 1] = new Tile();

            //if (Main.tile[num194 + direction, num195 - 1] == null)
            //    Main.tile[num194 + direction, num195 - 1] = new Tile();

            //if (Main.tile[num194 + direction, num195 + 1] == null)
            //    Main.tile[num194 + direction, num195 + 1] = new Tile();

            //if (Main.tile[num194 - direction, num195 + 1] == null)
            //    Main.tile[num194 - direction, num195 + 1] = new Tile();

            //Main.tile[num194, num195 + 1].IsHalfBlock;
            if (Main.tile[num194, num195 - 1].HasUnactuatedTile && (Main.tile[num194, num195 - 1].TileType == TileID.ClosedDoor || Main.tile[num194, num195 - 1].TileType == TileID.TallGateClosed) && canOpenDoor) {
                npc.ai[2] += 1f;
                npc.ai[3] = 0f;
                if (npc.ai[2] >= 60f) {
                    bool flag23 = npc.type == 3 || npc.type == 430 || npc.type == 590 || npc.type == 331 || npc.type == 332 || npc.type == 132 || npc.type == 161 || npc.type == 186 || npc.type == 187 || npc.type == 188 || npc.type == 189 || npc.type == 200 || npc.type == 223 || npc.type == 320 || npc.type == 321 || npc.type == 319 || npc.type == 21 || npc.type == 324 || npc.type == 323 || npc.type == 322 || npc.type == 44 || npc.type == 196 || npc.type == 167 || npc.type == 77 || npc.type == 197 || npc.type == 202 || npc.type == 203 || npc.type == 449 || npc.type == 450 || npc.type == 451 || npc.type == 452 || npc.type == 481 || npc.type == 201 || npc.type == 635;
                    bool flag24 = Main.player[npc.target].ZoneGraveyard && Main.rand.Next(60) == 0;
                    if ((!Main.bloodMoon || Main.getGoodWorld) && !flag24 && flag23)
                        npc.ai[1] = 0f;

                    npc.velocity.X = 0.5f * (float)(-npc.direction);
                    int num196 = 5;
                    if (Main.tile[num194, num195 - 1].TileType == 388)
                        num196 = 2;

                    npc.ai[1] += num196;
                    if (npc.type == 27)
                        npc.ai[1] += 1f;

                    if (npc.type == 31 || npc.type == 294 || npc.type == 295 || npc.type == 296)
                        npc.ai[1] += 6f;

                    npc.ai[2] = 0f;
                    bool flag25 = false;
                    if (npc.ai[1] >= 10f) {
                        flag25 = true;
                        npc.ai[1] = 10f;
                    }

                    if (npc.type == 460)
                        flag25 = true;

                    WorldGen.KillTile(num194, num195 - 1, fail: true);
                    if ((Main.netMode != 1 || !flag25) && flag25 && Main.netMode != 1) {
                        if (npc.type == 26) {
                            WorldGen.KillTile(num194, num195 - 1);
                            if (Main.netMode == 2)
                                NetMessage.SendData(17, -1, -1, null, 0, num194, num195 - 1);
                        }
                        else {
                            if (Main.tile[num194, num195 - 1].TileType == 10) {
                                bool flag26 = WorldGen.OpenDoor(num194, num195 - 1, npc.direction);
                                if (!flag26) {
                                    npc.ai[3] = num56;
                                    npc.netUpdate = true;
                                }

                                if (Main.netMode == 2 && flag26)
                                    NetMessage.SendData(19, -1, -1, null, 0, num194, num195 - 1, npc.direction);
                            }

                            if (Main.tile[num194, num195 - 1].TileType == 388) {
                                bool flag27 = WorldGen.ShiftTallGate(num194, num195 - 1, closing: false);
                                if (!flag27) {
                                    npc.ai[3] = num56;
                                    npc.netUpdate = true;
                                }

                                if (Main.netMode == 2 && flag27)
                                    NetMessage.SendData(19, -1, -1, null, 4, num194, num195 - 1);
                            }
                        }
                    }
                }
            }
            else {
                int num197 = npc.spriteDirection;
                if (npc.type == 425)
                    num197 *= -1;

                bool flag15 = ignoreBranches || (!ignoreBranches && Main.player[npc.target].position.Y > npc.Center.Y - npc.height);
                //flag15 = false;
                if ((npc.velocity.X < 0f && num197 == -1) || (npc.velocity.X > 0f && num197 == 1)) {
                    bool flag16 = (Main.tile[num194 + npc.direction, num195 + 1].TileType != ModContent.TileType<TreeBranch>() || !flag15);
                    //flag16 = true;
                    bool flag17 = (Main.tile[num194, num195 + 1].TileType != ModContent.TileType<TreeBranch>() || !flag15);
                    //flag17 = true;
                    bool flag18 = ((!Main.tile[num194, num195 + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num194, num195 + 1].TileType]));
                    bool flag19 = ((!Main.tile[num194 + npc.direction, num195 + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num194 + npc.direction, num195 + 1].TileType]));
                    if (npc.height >= 32 && Main.tile[num194, num195 - 2].HasUnactuatedTile && Main.tileSolid[Main.tile[num194, num195 - 2].TileType]) {
                        if (Main.tile[num194, num195 - 3].HasUnactuatedTile && Main.tileSolid[Main.tile[num194, num195 - 3].TileType]) {
                            if (Main.tile[num194, num195 - 3].TileType != ModContent.TileType<TreeBranch>() || !flag15) {
                                npc.velocity.Y = -8f;
                                npc.netUpdate = true;
                            }
                        }
                        else {
                            if (Main.tile[num194, num195 - 3].TileType != ModContent.TileType<TreeBranch>() || !flag15) {
                                npc.velocity.Y = -7f;
                                npc.netUpdate = true;
                            }
                        }
                    }
                    else if (Main.tile[num194, num195 - 1].HasUnactuatedTile && Main.tileSolid[Main.tile[num194, num195 - 1].TileType]) {
                        if (Main.tile[num194, num195 - 1].TileType != ModContent.TileType<TreeBranch>() || !flag15) {
                            if (npc.type == 624) {
                                npc.velocity.Y = -8f;
                                int num198 = (int)(npc.position.Y + (float)npc.height) / 16;
                                if (WorldGen.SolidTile((int)npc.Center.X / 16, num198 - 8)) {
                                    npc.direction *= -1;
                                    npc.spriteDirection = npc.direction;
                                    npc.velocity.X = 3 * npc.direction;
                                }
                            }
                            else {
                                npc.velocity.Y = -6f;
                            }

                            npc.netUpdate = true;
                        }
                    }
                    else if (npc.position.Y + (float)npc.height - (float)(num195 * 16) > 20f && Main.tile[num194, num195].HasUnactuatedTile && !Main.tile[num194, num195].TopSlope && Main.tileSolid[Main.tile[num194, num195].TileType]) {
                        npc.velocity.Y = -5f;
                        npc.netUpdate = true;
                    }
                    else if (npc.directionY < 0 && npc.type != 67 && (flag18 || flag19)) {
                        if ((flag18 | !flag17) || (flag19 | !flag16)) {
                            npc.velocity.Y = -8f;
                            //npc.velocity.X *= 1.5f;
                            npc.netUpdate = true;
                        }
                    }
                    else if (canOpenDoor) {
                        npc.ai[1] = 0f;
                        npc.ai[2] = 0f;
                    }

                    if (npc.velocity.Y == 0f && flag6 && npc.ai[3] == 1f)
                        npc.velocity.Y = -5f;

                    if (npc.velocity.Y == 0f && Main.player[npc.target].Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) < (float)(Main.player[npc.target].width * 3) && Collision.CanHit(npc, Main.player[npc.target])) {
                        //if (npc.type == 586) {
                        //    int num199 = (int)((npc.Bottom.Y - 16f - Main.player[npc.target].Bottom.Y) / 16f);
                        //    if (num199 < 14 && Collision.CanHit(npc, Main.player[npc.target])) {
                        //        if (num199 < 7)
                        //            npc.velocity.Y = -8.8f;
                        //        else if (num199 < 8)
                        //            npc.velocity.Y = -9.2f;
                        //        else if (num199 < 9)
                        //            npc.velocity.Y = -9.7f;
                        //        else if (num199 < 10)
                        //            npc.velocity.Y = -10.3f;
                        //        else if (num199 < 11)
                        //            npc.velocity.Y = -10.6f;
                        //        else
                        //            npc.velocity.Y = -11f;
                        //    }
                        //}

                        if (npc.velocity.Y == 0f) {
                            int num200 = 6;
                            if (Main.player[npc.target].Bottom.Y > npc.Top.Y - (float)(num200 * 16)) {
                                npc.velocity.Y = -7.9f;
                            }
                            else {
                                int num201 = (int)(npc.Center.X / 16f);
                                int num202 = (int)(npc.Bottom.Y / 16f) - 1;
                                for (int num203 = num202; num203 > num202 - num200; num203--) {
                                    if (Main.tile[num201, num203].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[num201, num203].TileType] && (Main.tile[num201, num203].TileType != ModContent.TileType<TreeBranch>() || !flag15)) {
                                        npc.velocity.Y = -7.9f;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                if ((npc.type == 31 || npc.type == 294 || npc.type == 295 || npc.type == 296 || npc.type == 47 || npc.type == 77 || npc.type == 104 || npc.type == 168 || npc.type == 196 || npc.type == 385 || npc.type == 389 || npc.type == 464 || npc.type == 470 || (npc.type >= 524 && npc.type <= 527)) && npc.velocity.Y == 0f) {
                    int num204 = 100;
                    int num205 = 50;
                    if (npc.type == 586) {
                        num204 = 150;
                        num205 = 150;
                    }

                    if (Math.Abs(npc.position.X + (float)(npc.width / 2) - (Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2))) < (float)num204 && Math.Abs(npc.position.Y + (float)(npc.height / 2) - (Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2))) < (float)num205 && ((npc.direction > 0 && npc.velocity.X >= 1f) || (npc.direction < 0 && npc.velocity.X <= -1f))) {
                        if (npc.type == 586) {
                            npc.velocity.X += npc.direction;
                            npc.velocity.X *= 2f;
                            if (npc.velocity.X > 8f)
                                npc.velocity.X = 8f;

                            if (npc.velocity.X < -8f)
                                npc.velocity.X = -8f;

                            npc.velocity.Y = -4.5f;
                            if (npc.position.Y > Main.player[npc.target].position.Y + 40f)
                                npc.velocity.Y -= 2f;

                            if (npc.position.Y > Main.player[npc.target].position.Y + 80f)
                                npc.velocity.Y -= 2f;

                            if (npc.position.Y > Main.player[npc.target].position.Y + 120f)
                                npc.velocity.Y -= 2f;
                        }
                        else {
                            npc.velocity.X *= 2f;
                            if (npc.velocity.X > 3f)
                                npc.velocity.X = 3f;

                            if (npc.velocity.X < -3f)
                                npc.velocity.X = -3f;

                            npc.velocity.Y = -4f;
                        }

                        npc.netUpdate = true;
                    }
                }

                if (npc.type == 120 && npc.velocity.Y < 0f)
                    npc.velocity.Y *= 1.1f;

                if (npc.type == 287 && npc.velocity.Y == 0f && Math.Abs(npc.position.X + (float)(npc.width / 2) - (Main.player[npc.target].position.X + (float)(Main.player[npc.target].width / 2))) < 150f && Math.Abs(npc.position.Y + (float)(npc.height / 2) - (Main.player[npc.target].position.Y + (float)(Main.player[npc.target].height / 2))) < 50f && ((npc.direction > 0 && npc.velocity.X >= 1f) || (npc.direction < 0 && npc.velocity.X <= -1f))) {
                    npc.velocity.X = 8 * npc.direction;
                    npc.velocity.Y = -4f;
                    npc.netUpdate = true;
                }

                if (npc.type == 287 && npc.velocity.Y < 0f) {
                    npc.velocity.X *= 1.2f;
                    npc.velocity.Y *= 1.1f;
                }

                if (npc.type == 460 && npc.velocity.Y < 0f) {
                    npc.velocity.X *= 1.3f;
                    npc.velocity.Y *= 1.1f;
                }
            }
        }
        else if (canOpenDoor) {
            npc.ai[1] = 0f;
            npc.ai[2] = 0f;
        }
        Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

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
