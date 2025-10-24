using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static partial class NPCExtensions {
    public static void SetDirection(this NPC npc, int direction, bool setSpriteDirectionToo = true) {
        npc.direction = direction;
        if (setSpriteDirectionToo) {
            npc.spriteDirection = npc.direction;
        }
    }

    public static bool CanActivateOnHitEffect(this NPC npc) => !npc.immortal && npc.lifeMax > 5;

    public static void StepUp(this NPC npc) => Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

    public static Player GetTargetPlayer(this NPC npc) => Main.player[npc.target];

    public static float SpeedX(this NPC npc) => MathF.Abs(npc.velocity.X);

    public static bool HasJustChangedFrame(this NPC npc) => npc.frameCounter == 0.0;

    public static bool SameAs(this NPC npc, NPC checkNPC) => npc.whoAmI == checkNPC.whoAmI;

    public static bool IsModded(this NPC npc) => npc.ModNPC is not null;
    public static bool IsModded(this NPC npc, out ModNPC modNPC) {
        modNPC = npc.ModNPC;
        return modNPC is not null;
    }

    public static float GetRemainingHealthPercentage(this NPC npc) => 1f - npc.life / (float)npc.lifeMax;

    public static byte AnimateFrame(
        this NPC npc,
        byte currentFrame,
        byte startFrame,
        byte endFrame,
        byte animationSpeed,
        ushort frameHeight,
        double additionalCounter = 0.0,
        bool resetAnimation = true) {
        npc.frameCounter += additionalCounter + 1.0;

        if (npc.frameCounter > animationSpeed) {
            bool up = startFrame < endFrame;
            if (resetAnimation || (up && currentFrame < endFrame) || (!up && currentFrame > endFrame)) {
                currentFrame += (byte)up.ToDirectionInt();
            }

            if (((up && currentFrame < startFrame) || (!up && currentFrame > startFrame)) || (resetAnimation && ((up && currentFrame > endFrame) || (!up && currentFrame < endFrame)))) {
                currentFrame = startFrame;
            }

            npc.SetFrame(currentFrame, frameHeight);
            npc.frameCounter = 0.0;
        }

        return currentFrame;
    }

    public static ushort GetCurrentFrame(this NPC npc, ushort frameHeight) => (ushort)(npc.frame.Y / (float)frameHeight);
    public static ushort GetFrameHeight(this NPC npc) => (ushort)(TextureAssets.Npc[npc.type].Height() / (float)Main.npcFrameCount[npc.type]);
    public static void SetFrame(this NPC npc, byte frame, ushort frameHeight) => npc.frame.Y = frame * frameHeight;
    public static void SetFrameCount(this NPC npc, byte frameCount) => Main.npcFrameCount[npc.type] = frameCount;

    public static void UpdateDirectionBasedOnVelocity(this NPC npc, bool updateSpriteDirection = true) {
        npc.direction = npc.velocity.X.GetDirection();
        if (updateSpriteDirection) {
            npc.spriteDirection = npc.direction;
        }
    }
    public static bool IsFacingLeft(this NPC npc) => npc.direction < 0;
    public static bool IsFacingRight(this NPC npc) => npc.direction > 0;
    public static bool IsGrounded(this NPC npc) => npc.velocity.Y == 0f;

    public static void DirectTo(this NPC npc, Vector2 destination, bool updateSpriteDirection = true, bool reverse = false) => DirectTo(npc, destination.GetDirectionTo(npc.Center), updateSpriteDirection, reverse);
    public static void DirectTo(this NPC npc, int direction, bool updateSpriteDirection = true, bool reverse = false) {
        npc.direction = (direction > 0).ToDirectionInt();
        if (reverse) {
            npc.direction *= -1;
        }
        if (updateSpriteDirection) {
            npc.spriteDirection = npc.direction;
        }
    }

    public static class FighterAI {
        public static void ApplyFighterAI(
            NPC npc,
            ref float canBeBusyWithActionTimer,
            ref float encouragementTimer,
            ref float targetClosestTimer,
            bool? shouldTargetPlayer = null,
            Action? xMovement = null,
            Predicate<NPC>? shouldBeBored = null) {
            NPC NPC = npc;
            NPC.aiStyle = NPC.ModNPC.AIType = -1;

            if (Main.player[npc.target].position.Y + (float)Main.player[npc.target].height == npc.position.Y + (float)npc.height)
                npc.directionY = -1;

            bool targetPlayer = true;
            shouldTargetPlayer ??= Terraria.NPC.DespawnEncouragement_AIStyle3_Fighters_NotDiscouraged(npc.type, npc.position, npc);

            //shouldTargetPlayer = npc.life < (int)(npc.lifeMax * 0.8f) || (Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].InModBiome<BackwoodsBiome>() && targetPlayer);

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

            bool flag9 = false;

            bool flag10 = true;

            if (!flag9 && flag10) {
                if (npc.velocity.Y == 0f && ((npc.velocity.X > 0f && npc.direction < 0) || (npc.velocity.X < 0f && npc.direction > 0)))
                    flag7 = true;

                if (shouldBeBored != null && shouldBeBored(npc)) {
                    flag7 = true;
                }

                if (npc.position.X == npc.oldPosition.X || targetClosestTimer >= (float)num56 || flag7)
                    targetClosestTimer += 1f;
                else if ((double)Math.Abs(npc.velocity.X) > 0.9 && targetClosestTimer > 0f)
                    targetClosestTimer -= 1f;

                if (targetClosestTimer > (float)(num56 * 10))
                    targetClosestTimer = 0f;

                if (npc.justHit)
                    targetClosestTimer = 0f;

                if (targetClosestTimer == (float)num56)
                    npc.netUpdate = true;

                if (Main.player[npc.target].Hitbox.Intersects(npc.Hitbox))
                    targetClosestTimer = 0f;
            }

            if (targetClosestTimer < (float)num56 && shouldTargetPlayer.Value) {
                npc.TargetClosest();
                if (npc.directionY > 0 && Main.player[npc.target].Center.Y <= npc.Bottom.Y)
                    npc.directionY = -1;
            }
            else if (!(canBeBusyWithActionTimer > 0f) || !Terraria.NPC.DespawnEncouragement_AIStyle3_Fighters_CanBeBusyWithAction(npc.type)) {
                bool flag12 = targetPlayer/*Main.player[npc.target].InModBiome<BackwoodsBiome>()*/;
                if (!flag12 && (double)(npc.position.Y / 16f) < Main.worldSurface/* && npc.type != 624 && npc.type != 631*/) {
                    npc.EncourageDespawn(10);
                }

                if (npc.velocity.X == 0f) {
                    if (npc.velocity.Y == 0f) {
                        encouragementTimer += 1f;
                        if (encouragementTimer >= 2f) {
                            npc.direction *= -1;
                            npc.spriteDirection = npc.direction;
                            encouragementTimer = 0f;
                        }
                    }
                }
                else {
                    encouragementTimer = 0f;
                }

                if (npc.direction == 0)
                    npc.direction = 1;
            }

            if (xMovement == null) {
                float num87 = 1f * 0.9f;
                float num88 = 0.07f * 0.9f;
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
                xMovement();
            }

            bool tileChecks = false;
            if (NPC.velocity.Y == 0f) {
                int num77 = (int)(NPC.position.Y + NPC.height + 7f) / 16;
                int num189 = (int)NPC.position.X / 16;
                int num79 = (int)(NPC.position.X + NPC.width) / 16;
                for (int num80 = num189; num80 <= num79; num80++) {
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

                        //if (num85 <= num86) {
                        //    NPC.gfxOffY += NPC.position.Y + NPC.height - num84;
                        //    NPC.position.Y = num84 - NPC.height;
                        //    if (num85 < 9f) {
                        //        NPC.stepSpeed = 1f;
                        //    }
                        //    else {
                        //        NPC.stepSpeed = 2f;
                        //    }
                        //}
                    }
                }
            }
            if (tileChecks && !Main.tile[(int)(NPC.Center.X) / 16, (int)(NPC.Center.Y - 15f) / 16 - 1].HasUnactuatedTile) {
                int tileX = (int)((NPC.position.X + NPC.width / 2 + NPC.width * 0.53f * NPC.direction) / 16f);
                int tileY = (int)((NPC.position.Y + NPC.height - 15f) / 16f);

                {
                    if (NPC.velocity.X < 0f && NPC.direction == -1 || NPC.velocity.X > 0f && NPC.direction == 1) {
                        void jumpIfPlayerAboveAndClose() {
                            if (npc.velocity.Y == 0f && Main.expertMode && Main.player[npc.target].Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) < (float)(Main.player[npc.target].width * 3) && Collision.CanHit(npc, Main.player[npc.target])) {
                                if (npc.velocity.Y == 0f) {
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

                        void JumpCheck(int tileX, int tileY) {
                            int num197 = npc.spriteDirection;
                            int num194 = tileX, num195 = tileY;
                            if ((npc.velocity.X < 0f && num197 == -1) || (npc.velocity.X > 0f && num197 == 1)) {
                                if (npc.height >= 32 && Main.tile[num194, num195 - 2].HasUnactuatedTile && Main.tileSolid[Main.tile[num194, num195 - 2].TileType]) {
                                    if (Main.tile[num194, num195 - 3].HasUnactuatedTile && Main.tileSolid[Main.tile[num194, num195 - 3].TileType]) {
                                        npc.velocity.Y = -8f;
                                        npc.netUpdate = true;
                                    }
                                    else {
                                        npc.velocity.Y = -7f;
                                        npc.netUpdate = true;
                                    }
                                }
                                else if (Main.tile[num194, num195 - 1].HasUnactuatedTile && Main.tileSolid[Main.tile[num194, num195 - 1].TileType]) {
                                    npc.velocity.Y = -6f;
                                    npc.netUpdate = true;
                                }
                                else if (npc.position.Y + (float)npc.height - (float)(num195 * 16) > 20f && Main.tile[num194, num195].HasUnactuatedTile && !Main.tile[num194, num195].TopSlope && Main.tileSolid[Main.tile[num194, num195].TileType]) {
                                    npc.velocity.Y = -5f;
                                    npc.netUpdate = true;
                                }
                                else if (npc.directionY < 0 && (!Main.tile[num194, num195 + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num194, num195 + 1].TileType]) && (!Main.tile[num194 + npc.direction, num195 + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num194 + npc.direction, num195 + 1].TileType])) {
                                    npc.velocity.Y = -8f;
                                    npc.velocity.X *= 1.5f;
                                    npc.netUpdate = true;
                                }
                            }
                        }

                        JumpCheck(tileX, tileY);
                    }
                }
            }
            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        }
    }

    public readonly struct NPCHitInfo(ushort lifeMax = 5, ushort damage = 0, ushort defense = 0, float knockBackResist = 1f) {
        public readonly ushort LifeMax = lifeMax;
        public readonly ushort Damage = damage;
        public readonly ushort Defense = defense;
        public readonly float KnockBackResist = knockBackResist;
    }

    public static void SetSizeValues(this NPC npc, int width, int? height = null) {
        npc.width = width;
        npc.height = height ?? width;
    }

    public static void DefaultToEnemy(this NPC npc, in NPCHitInfo hitInfo) {
        npc.friendly = false;

        npc.lifeMax = hitInfo.LifeMax;
        npc.damage = hitInfo.Damage;
        npc.defense = hitInfo.Defense;
        npc.knockBackResist = hitInfo.KnockBackResist;
    }
}
    
