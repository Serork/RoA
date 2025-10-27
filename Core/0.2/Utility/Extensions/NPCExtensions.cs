using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using RoA.Content.NPCs.Enemies.Tar;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Core.Utility;

static partial class NPCExtensions {
    public static bool AnyNPCs(this NPC npc) => NPC.AnyNPCs(npc.type);

    public static void SetDirection(this NPC npc, int direction, bool setSpriteDirectionToo = true) {
        npc.direction = direction;
        if (setSpriteDirectionToo) {
            npc.spriteDirection = npc.direction;
        }
    }

    public static bool CanActivateOnHitEffect(this NPC npc) => !npc.immortal && npc.lifeMax > 5;

    public static void StepUp(this NPC npc) {
        if (npc.IsGrounded()) {
            Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
        }
    }

    public static Player GetTargetPlayer(this NPC npc) => Main.player[npc.target];

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
                if (NPC.IsGrounded() && ((npc.velocity.X > 0f && npc.direction < 0) || (npc.velocity.X < 0f && npc.direction > 0)))
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
                    if (NPC.IsGrounded()) {
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
                    if (NPC.IsGrounded())
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
            if (NPC.IsGrounded()) {
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
            if (npc.IsGrounded()) {
                Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
            }
        }
    }

    public static void ApplyImprovedWalkerAI(this NPC npc) {
        bool flag = npc.velocity.X == 0f && npc.velocity.Y == 0f && !npc.justHit;
        bool flag2 = false;
        bool flag3 = false;
        bool flag4 = false;
        int num = 32;
        int num2 = 15;
        float num3 = 9f;
        bool flag5 = false;
        float num4 = 40f;
        int num5 = 30;
        int num6 = 0;
        bool flag6 = false;
        bool flag7 = true;
        float num7 = 0.9f;
        bool flag8 = false;
        bool flag9 = false;
        bool flag10 = false;
        bool flag11 = false;
        bool flag12 = false;
        bool flag13 = false;
        bool flag14 = false;
        bool flag15 = true;
        int num8 = 70;
        int num9 = num8 / 2;
        float num10 = 11f;
        Vector2 vector = Vector2.Zero;
        int num11 = 1;
        int num12 = 81;
        float num13 = 700f;
        float num14 = 0f;
        float num15 = 0.1f;
        Vector2? vector2 = null;
        float num16 = 0.5f;
        int num17 = 1;
        float num18 = 1f;
        bool flag16 = false;
        float num19 = 30f;
        float num20 = 0f;
        bool flag17 = false;
        bool flag18 = true;
        bool flag19 = false;
        int num21 = 30;
        bool flag20 = false;
        bool flag21 = false;
        bool flag22 = false;
        bool flag23 = false;
        SoundStyle? style = null; // LegacySoundStyle -> SoundStyle
        int num22 = 0;
        bool flag24 = false;
        float maxSpeed = 1f;
        float acceleration = 0.07f;
        float deceleration = 0.8f;
        float num26 = npc.width / 2 + 6;
        bool flag25 = npc.directionY < 0;
        bool flag26 = false;
        int num27 = 1;
        bool flag27 = false;
        float num28 = 5f;
        float num29 = 3f;
        float num30 = 8f;
        float amount = 0.05f;
        float amount2 = 0.04f;
        float amount3 = 0.1f;
        bool flag28 = false;
        float num31 = 0.025f;
        NPCAimedTarget targetData = npc.GetTargetData();
        NPCUtils.NPCTargetingMethod nPCTargetingMethod = NPCUtils.TargetClosestCommon;
        if (NPCID.Sets.BelongsToInvasionOldOnesArmy[npc.type])
            nPCTargetingMethod = NPCUtils.TargetClosestOldOnesInvasion;

        if (targetData.Type == NPCTargetType.NPC && Main.npc[npc.TranslatedTargetIndex].type == 548 && Main.npc[npc.TranslatedTargetIndex].dontTakeDamageFromHostiles) {
            nPCTargetingMethod(npc, faceTarget: true, null);
            targetData = npc.GetTargetData();
        }

        if (NPCID.Sets.FighterUsesDD2PortalAppearEffect[npc.type]) {
            if (!targetData.Invalid)
                flag2 = !Collision.CanHit(npc.Center, 0, 0, targetData.Center, 0, 0) && (npc.direction == Math.Sign(targetData.Center.X - npc.Center.X) || (npc.noGravity && npc.Distance(targetData.Center) > 50f && npc.Center.Y > targetData.Center.Y));

            flag2 &= npc.ai[0] <= 0f;
        }

        if (flag2) {
            if (npc.velocity.Y == 0f || Math.Abs(targetData.Center.Y - npc.Center.Y) > 800f) {
                npc.noGravity = true;
                npc.noTileCollide = true;
            }
        }
        else {
            npc.noGravity = false;
            npc.noTileCollide = false;
        }

        bool flag29 = NPCID.Sets.FighterUsesDD2PortalAppearEffect[npc.type];
        bool flag30 = true;
        if (npc.type == ModContent.NPCType<PerfectMimic>()) {
            flag27 = npc.wet;
            flag30 = false;
            flag16 = true;
            num20 = 150f;
            num19 = 20f;
            //bool dayTime = Main.dayTime;
            flag21 = true;
            flag24 = true;
            flag12 = true;
            flag15 = npc.ai[1] > 40f;

            npc.ai[1] = 10f;

            num8 = 60;
            num9 = 40;
            if (npc.ai[1] > 10f && npc.ai[1] <= 40f && (int)npc.ai[1] % 5 == 0)
                num9 = (int)npc.ai[1] - 1;

            //num12 = 811;
            vector.X -= 4 * npc.direction;
            vector.Y -= 20f;
            //num15 = 0.15f;
            //num16 = 2.5f;
            num13 = 600f;
            num10 = 13f;
            num17 = 1;
            num18 = 0f;
            //num11 = npc.GetAttackDamage_ForProjectiles(40f, 30f);
            maxSpeed = 5f;
            acceleration *= 1f;
            deceleration *= 1f;

            //npc.position += npc.netOffset;
            //if (npc.alpha == 255) {
            //    npc.spriteDirection = npc.direction;
            //    npc.velocity.Y = -6f;
            //    npc.netUpdate = true;
            //    for (int i = 0; i < 35; i++) {
            //        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, 5);
            //        dust.velocity *= 1f;
            //        dust.scale = 1f + Main.rand.NextFloat() * 0.5f;
            //        dust.fadeIn = 1.5f + Main.rand.NextFloat() * 0.5f;
            //        dust.velocity += npc.velocity * 0.5f;
            //    }
            //}

            //npc.alpha -= 15;
            //if (npc.alpha < 0)
            //    npc.alpha = 0;

            //if (npc.alpha != 0) {
            //    for (int j = 0; j < 2; j++) {
            //        Dust dust2 = Dust.NewDustDirect(npc.position, npc.width, npc.height, 5);
            //        dust2.velocity *= 1f;
            //        dust2.scale = 1f + Main.rand.NextFloat() * 0.5f;
            //        dust2.fadeIn = 1.5f + Main.rand.NextFloat() * 0.5f;
            //        dust2.velocity += npc.velocity * 0.3f;
            //    }
            //}

            //if (Main.rand.Next(3) == 0)
            //    Dust.NewDustDirect(npc.position, npc.width, npc.height, 5, 0f, 0f, 50, default(Color), 1.3f).velocity = Vector2.Zero;

            //npc.position -= npc.netOffset;
            if (!(npc.velocity.Y != 0f || !((float)targetData.Hitbox.Bottom < npc.Top.Y) || !(Math.Abs(npc.Center.X - (float)targetData.Hitbox.Center.X) < (float)(npc.width * 3)) || !Collision.CanHit(npc.Hitbox.TopLeft(), npc.Hitbox.Width, npc.Hitbox.Height, targetData.Hitbox.TopLeft(), targetData.Hitbox.Width, targetData.Hitbox.Height))) {
                int num32 = (int)((npc.Bottom.Y - 16f - (float)targetData.Hitbox.Bottom) / 16f);
                if (num32 < 27) {
                    if (num32 < 11)
                        npc.velocity.Y = -11f;
                    else if (num32 < 15)
                        npc.velocity.Y = -13f;
                    else if (num32 < 19)
                        npc.velocity.Y = -14f;
                    else
                        npc.velocity.Y = -15.9f;
                }
            }
        }

        if (flag28) {
            bool flag31 = npc.velocity.Y == 0f;
            for (int num52 = 0; num52 < 200; num52++) {
                if (num52 != npc.whoAmI && Main.npc[num52].active && Main.npc[num52].type == npc.type && Math.Abs(npc.position.X - Main.npc[num52].position.X) + Math.Abs(npc.position.Y - Main.npc[num52].position.Y) < (float)npc.width) {
                    if (npc.position.X < Main.npc[num52].position.X)
                        npc.velocity.X -= num31;
                    else
                        npc.velocity.X += num31;

                    if (npc.position.Y < Main.npc[num52].position.Y)
                        npc.velocity.Y -= num31;
                    else
                        npc.velocity.Y += num31;
                }
            }

            if (flag31)
                npc.velocity.Y = 0f;
        }

        if (flag29) {
            if (npc.localAI[3] == 0f)
                npc.alpha = 255;

            if (npc.localAI[3] == 30f)
                SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy, npc.Center);

            if (npc.localAI[3] < 60f) {
                npc.localAI[3] += 1f;
                npc.alpha -= 5;
                if (npc.alpha < 0)
                    npc.alpha = 0;

                int num53 = (int)npc.localAI[3] / 10;
                float num54 = npc.Size.Length() / 2f;
                num54 /= 20f;
                int maxValue3 = 5;
                if (npc.type == 576 || npc.type == 577)
                    maxValue3 = 1;

                for (int num55 = 0; num55 < num53; num55++) {
                    if (Main.rand.Next(maxValue3) == 0) {
                        Dust dust9 = Dust.NewDustDirect(npc.position, npc.width, npc.height, 27, npc.velocity.X * 1f, 0f, 100);
                        dust9.scale = 0.55f;
                        dust9.fadeIn = 0.7f;
                        dust9.velocity *= 0.1f * num54;
                        dust9.velocity += npc.velocity;
                    }
                }
            }
        }

        if (flag27) {
            npc.noGravity = true;
            npc.TargetClosest(faceTarget: false);
            if (npc.collideX)
                npc.velocity.X = 0f - npc.oldVelocity.X;

            if (npc.velocity.X != 0f)
                npc.direction = Math.Sign(npc.direction);

            if (Collision.CanHit(npc.position, npc.width, npc.height, targetData.Position, targetData.Width, targetData.Height)) {
                Vector2 value = targetData.Center - npc.Center;
                value.Normalize();
                value *= num28;
                npc.velocity = Vector2.Lerp(npc.velocity, value, amount);
                return;
            }

            float num56 = num28;
            if (npc.velocity.Y > 0f)
                num56 = num29;

            if (npc.velocity.Y < 0f)
                num56 = num30;

            Vector2 value2 = new Vector2(npc.direction, -1f);
            value2.Normalize();
            value2 *= num56;
            if (num56 < num28)
                npc.velocity = Vector2.Lerp(npc.velocity, value2, amount2);
            else
                npc.velocity = Vector2.Lerp(npc.velocity, value2, amount3);

            return;
        }

        bool flag32 = false;
        if ((flag12 || flag5) && npc.ai[0] > 0f)
            flag18 = false;

        if (flag12 && npc.ai[1] > 0f)
            flag22 = true;

        if (flag5 && npc.ai[0] > 0f)
            flag22 = true;

        if (flag5) {
            if (npc.ai[0] < 0f) {
                npc.ai[0] += 1f;
                flag = false;
            }

            if (npc.ai[0] == 0f && (npc.velocity.Y == 0f || flag6) && targetData.Type != 0 && (Collision.CanHit(npc.position, npc.width, npc.height, targetData.Position, targetData.Width, targetData.Height) || Collision.CanHitLine(npc.position, npc.width, npc.height, targetData.Position, targetData.Width, targetData.Height)) && (targetData.Center - npc.Center).Length() < num4) {
                npc.ai[0] = num5;
                npc.netUpdate = true;
            }

            if (npc.ai[0] > 0f) {
                npc.spriteDirection = npc.direction * num27;
                if (flag7) {
                    npc.velocity.X *= num7;
                    flag24 = true;
                    flag20 = true;
                    npc.ai[3] = 0f;
                    npc.netUpdate = true;
                }

                npc.ai[0] -= 1f;
                if (npc.ai[0] == 0f) {
                    npc.ai[0] = -num6;
                    npc.netUpdate = true;
                }
            }
        }

        if (flag3 && npc.ai[0] > 0f) {
            if (flag15) {
                nPCTargetingMethod(npc, faceTarget: true, null);
                targetData = npc.GetTargetData();
            }

            if (npc.ai[0] == (float)num9) {
                Vector2 vector4 = npc.Center + vector;
                Vector2 v = targetData.Center - vector4;
                v.Y -= Math.Abs(v.X) * num15;
                Vector2 vector5 = v.SafeNormalize(-Vector2.UnitY) * num10;
                for (int num57 = 0; num57 < num17; num57++) {
                    Vector2 vector6 = vector5;
                    Vector2 vector7 = vector4;
                    if (vector2.HasValue)
                        vector6 += vector2.Value;
                    else
                        vector6 += Utils.RandomVector2(Main.rand, 0f - num16, num16);

                    vector7 += vector5 * num18;
                    if (Main.netMode != 1)
                        Projectile.NewProjectile(npc.GetSource_FromAI(), vector7, vector6, num12, num11, 0f, Main.myPlayer);
                }
            }
        }

        if (flag4 && npc.ai[0] > 0f) {
            if (npc.velocity.Y != 0f && npc.ai[0] < (float)num2)
                npc.ai[0] = num2;

            if (npc.ai[0] == (float)num)
                npc.velocity.Y = 0f - num3;
        }

        if (!flag17 && flag18) {
            if (npc.velocity.Y == 0f && npc.velocity.X * (float)npc.direction < 0f)
                flag19 = true;

            if (npc.position.X == npc.oldPosition.X || npc.ai[3] >= (float)num21 || flag19)
                npc.ai[3] += 1f;
            else if (Math.Abs(npc.velocity.X) > 0.9f && npc.ai[3] > 0f)
                npc.ai[3] -= 1f;

            if (npc.ai[3] > (float)(num21 * 10))
                npc.ai[3] = 0f;

            if (npc.justHit && !flag30)
                npc.ai[3] = 0f;

            if (targetData.Hitbox.Intersects(npc.Hitbox))
                npc.ai[3] = 0f;

            if (npc.ai[3] == (float)num21) {
                npc.netUpdate = true;
                if (flag30) {
                    npc.noGravity = true;
                    npc.noTileCollide = true;
                    npc.position.X += npc.direction * npc.width * 2;
                    int num58 = 20;
                    float num59 = npc.Size.Length() / 2f;
                    num59 /= 20f;
                    int maxValue4 = 5;
                    if (npc.type == 576 || npc.type == 577)
                        maxValue4 = 1;

                    for (int num60 = 0; num60 < num58; num60++) {
                        if (Main.rand.Next(maxValue4) == 0) {
                            Dust dust10 = Dust.NewDustDirect(npc.position, npc.width, npc.height, 27, npc.velocity.X * 1f, 0f, 100);
                            dust10.scale = 0.55f;
                            dust10.fadeIn = 0.7f;
                            dust10.velocity *= 3f * num59;
                            dust10.noGravity = true;
                            dust10.fadeIn = 1.5f;
                            dust10.velocity *= 3f;
                        }
                    }

                    return;
                }
            }
        }

        if (!flag20) {
            if (npc.ai[3] < (float)num21 && flag21) {
                /*
				if (num22 > 0 && Main.rand.Next(num22) == 0)
					SoundEngine.PlaySound(style, npc.Center);
				*/

                if (num22 > 0 && Main.rand.Next(num22) == 0 && style.HasValue)
                    SoundEngine.PlaySound(style.Value, npc.Center);

                bool hasValidTarget = npc.HasValidTarget;
                nPCTargetingMethod(npc, faceTarget: true, null);
                targetData = npc.GetTargetData();
                if (hasValidTarget != npc.HasValidTarget)
                    npc.netUpdate = true;
            }
            else if (!flag22) {
                if (flag23)
                    npc.EncourageDespawn(10);

                if (npc.velocity.X == 0f) {
                    if (npc.velocity.Y == 0f) {
                        npc.ai[2] += 1f;
                        if (npc.ai[2] >= 2f) {
                            npc.direction *= -1;
                            npc.spriteDirection = npc.direction * num27;
                            npc.ai[2] = 0f;
                            npc.netUpdate = true;
                        }
                    }
                }
                else if (npc.ai[2] != 0f) {
                    npc.ai[2] = 0f;
                    npc.netUpdate = true;
                }

                if (npc.direction == 0)
                    npc.direction = 1;
            }
        }

        if (!flag24) {
            if (npc.velocity.X < 0f - maxSpeed || npc.velocity.X > maxSpeed) {
                if (npc.velocity.Y == 0f)
                    npc.velocity *= deceleration;
            }
            else if ((npc.velocity.X < maxSpeed && npc.direction == 1) || (npc.velocity.X > 0f - maxSpeed && npc.direction == -1)) {
                npc.velocity.X = MathHelper.Clamp(npc.velocity.X + acceleration * (float)npc.direction, 0f - maxSpeed, maxSpeed);
            }
        }

        if (flag12) {
            if (npc.confused) {
                npc.ai[0] = 0f;
            }
            else {
                if (npc.ai[1] > 0f)
                    npc.ai[1] -= 1f;

                if (npc.justHit) {
                    npc.ai[1] = num19;
                    npc.ai[0] = 0f;
                }

                if (npc.ai[0] > 0f) {
                    if (flag15) {
                        nPCTargetingMethod(npc, faceTarget: true, null);
                        targetData = npc.GetTargetData();
                    }

                    if (npc.ai[1] == (float)num9) {
                        Vector2 vector8 = npc.Center + vector;
                        Vector2 v2 = targetData.Center - vector8;
                        v2.Y -= Math.Abs(v2.X) * num15;
                        Vector2 vector9 = v2.SafeNormalize(-Vector2.UnitY) * num10;
                        for (int num61 = 0; num61 < num17; num61++) {
                            Vector2 vector10 = vector8;
                            Vector2 vector11 = vector9;
                            if (vector2.HasValue)
                                vector11 += vector2.Value;
                            else
                                vector11 += Utils.RandomVector2(Main.rand, 0f - num16, num16);

                            vector10 += vector11 * num18;
                            if (Main.netMode != 1)
                                Projectile.NewProjectile(npc.GetSource_FromAI(), vector10, vector11, num12, num11, 0f, Main.myPlayer);
                        }

                        if (Math.Abs(vector9.Y) > Math.Abs(vector9.X) * 2f)
                            npc.ai[0] = ((vector9.Y > 0f) ? 1 : 5);
                        else if (Math.Abs(vector9.X) > Math.Abs(vector9.Y) * 2f)
                            npc.ai[0] = 3f;
                        else
                            npc.ai[0] = ((vector9.Y > 0f) ? 2 : 4);

                        if (flag16)
                            npc.direction = ((vector9.X > 0f) ? 1 : (-1));
                    }

                    bool flag33 = true;
                    if ((npc.velocity.Y != 0f && !flag14) || npc.ai[1] <= 0f) {
                        bool flag34 = false;
                        if (num20 != 0f && npc.ai[1] <= 0f)
                            flag34 = true;

                        npc.ai[0] = 0f;
                        npc.ai[1] = (flag34 ? num20 : 0f);
                    }
                    else if (!flag13 || (!flag33 && (!flag14 || npc.velocity.Y == 0f))) {
                        npc.velocity.X *= 0.9f;
                        npc.spriteDirection = npc.direction * num27;
                    }
                }

                if ((npc.ai[0] <= 0f || flag13) && (npc.velocity.Y == 0f || flag14) && npc.ai[1] <= 0f && targetData.Type != 0 && Collision.CanHit(npc.position, npc.width, npc.height, targetData.Position, targetData.Width, targetData.Height)) {
                    Vector2 vector12 = targetData.Center - npc.Center;
                    if (vector12.Length() < num13) {
                        npc.netUpdate = true;
                        npc.velocity.X *= 0.5f;
                        npc.ai[0] = 3f;
                        npc.ai[1] = num8;
                        if (Math.Abs(vector12.Y) > Math.Abs(vector12.X) * 2f)
                            npc.ai[0] = ((vector12.Y > 0f) ? 1 : 5);
                        else if (Math.Abs(vector12.X) > Math.Abs(vector12.Y) * 2f)
                            npc.ai[0] = 3f;
                        else
                            npc.ai[0] = ((vector12.Y > 0f) ? 2 : 4);

                        if (flag16)
                            npc.direction = ((vector12.X > 0f) ? 1 : (-1));
                    }
                }

                if (npc.ai[0] <= 0f || flag13) {
                    bool flag35 = npc.Distance(targetData.Center) < num14;
                    if (flag35 && Collision.CanHitLine(npc.position, npc.width, npc.height, targetData.Position, targetData.Width, targetData.Height))
                        npc.ai[3] = 0f;

                    if (npc.velocity.X < 0f - maxSpeed || npc.velocity.X > maxSpeed || flag35) {
                        if (npc.velocity.Y == 0f)
                            npc.velocity.X *= deceleration;
                    }
                    else if ((npc.velocity.X < maxSpeed && npc.direction == 1) || (npc.velocity.X > 0f - maxSpeed && npc.direction == -1)) {
                        npc.velocity.X = MathHelper.Clamp(npc.velocity.X + acceleration * (float)npc.direction, 0f - maxSpeed, maxSpeed);
                    }
                }
            }
        }

        if (npc.velocity.Y == 0f) {
            int num62 = (int)(npc.Bottom.Y + 7f) / 16;
            int num63 = (int)npc.Left.X / 16;
            int num64 = (int)npc.Right.X / 16;
            int num65;
            for (num65 = num63; num65 <= num64; num65++) {
                num65 = Utils.Clamp(num65, 0, Main.maxTilesX);
                num62 = Utils.Clamp(num62, 0, Main.maxTilesY);
                Tile tile = Main.tile[num65, num62];
                if (tile == null)
                    return;

                if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType]) {
                    flag32 = true;
                    break;
                }
            }
        }

        Point point2 = npc.Center.ToTileCoordinates();
        if (WorldGen.InWorld(point2.X, point2.Y, 5) && !npc.noGravity) {
            npc.GetTileCollisionParameters(out var cPosition, out var cWidth, out var cHeight);
            Vector2 vector13 = npc.position - cPosition;
            Collision.StepUp(ref cPosition, ref npc.velocity, cWidth, cHeight, ref npc.stepSpeed, ref npc.gfxOffY);
            npc.position = cPosition + vector13;
        }

        if (flag32) {
            int num66 = (int)(npc.Center.X + num26 * (float)npc.direction) / 16;
            int num67 = ((int)npc.Bottom.Y - 15) / 16;
            bool flag36 = npc.position.Y + (float)npc.height - (float)(num67 * 16) > 20f;
            Tile tileSafely = Framing.GetTileSafely(num66 + npc.direction, num67 + 1);
            Tile tileSafely2 = Framing.GetTileSafely(num66, num67 + 1);
            Tile tileSafely3 = Framing.GetTileSafely(num66, num67);
            Tile tileSafely4 = Framing.GetTileSafely(num66, num67 - 1);
            Tile tileSafely5 = Framing.GetTileSafely(num66, num67 - 2);
            Tile tileSafely6 = Framing.GetTileSafely(num66, num67 - 3);
            if (flag8 && tileSafely4.HasUnactuatedTile && (TileLoader.IsClosedDoor(tileSafely4) || tileSafely4.TileType == 388)) {
                npc.ai[0] += 1f;
                npc.ai[3] = 0f;
                if (npc.ai[0] >= 60f) {
                    if (flag9)
                        npc.ai[1] = 0f;

                    int num68 = 5;
                    if (Main.tile[num66, num67 - 1].TileType == 388)
                        num68 = 2;

                    npc.velocity.X = 0.5f * (float)(-npc.direction);
                    npc.ai[1] += num68;
                    bool flag37 = false;
                    if (npc.ai[1] >= 10f) {
                        flag37 = true;
                        npc.ai[1] = 10f;
                    }

                    if (flag10)
                        flag37 = true;

                    WorldGen.KillTile(num66, num67 - 1, fail: true);
                    if (Main.netMode != 1 && flag37) {
                        if (flag11) {
                            WorldGen.KillTile(num66, num67 - 1);
                            if (Main.netMode == 2)
                                NetMessage.SendData(17, -1, -1, null, 0, num66, num67 - 1);
                        }
                        else {
                            if (TileLoader.IsClosedDoor(tileSafely4)) {
                                bool flag38 = WorldGen.OpenDoor(num66, num67 - 1, npc.direction);
                                if (!flag38) {
                                    npc.ai[3] = num21;
                                    npc.netUpdate = true;
                                }

                                if (Main.netMode == 2 && flag38)
                                    NetMessage.SendData(19, -1, -1, null, 0, num66, num67 - 1, npc.direction);
                            }

                            if (tileSafely4.TileType == 388) {
                                bool flag39 = WorldGen.ShiftTallGate(num66, num67 - 1, closing: false);
                                if (!flag39) {
                                    npc.ai[3] = num21;
                                    npc.netUpdate = true;
                                }

                                if (Main.netMode == 2 && flag39)
                                    NetMessage.SendData(19, -1, -1, null, 4, num66, num67 - 1, npc.direction);
                            }
                        }
                    }
                }
            }
            else {
                int num69 = npc.spriteDirection * num27;
                if (npc.velocity.X * (float)num69 > 0f) {
                    if (npc.height >= 32 && tileSafely5.HasUnactuatedTile && Main.tileSolid[tileSafely5.TileType]) {
                        npc.netUpdate = true;
                        npc.velocity.Y = -7f;
                        if (tileSafely6.HasUnactuatedTile && Main.tileSolid[tileSafely6.TileType])
                            npc.velocity.Y = -8f;
                    }
                    else if (tileSafely4.HasUnactuatedTile && Main.tileSolid[tileSafely4.TileType]) {
                        npc.velocity.Y = -6f;
                        npc.netUpdate = true;
                    }
                    else if (flag36 && tileSafely3.HasUnactuatedTile && !tileSafely3.TopSlope && Main.tileSolid[tileSafely3.TileType]) {
                        npc.velocity.Y = -5f;
                        npc.netUpdate = true;
                    }
                    else if (flag25 && (!tileSafely2.HasUnactuatedTile || !Main.tileSolid[tileSafely2.TileType]) && (!tileSafely.HasUnactuatedTile || !Main.tileSolid[tileSafely.TileType])) {
                        npc.velocity.X *= 1.5f;
                        npc.velocity.Y = -8f;
                        npc.netUpdate = true;
                    }
                    else if (flag8) {
                        npc.ai[0] = 0f;
                        npc.ai[1] = 0f;
                    }

                    if (npc.velocity.Y == 0f && flag && npc.ai[3] == 1f) {
                        npc.velocity.Y = -5f;
                        npc.netUpdate = true;
                    }
                }

                if (flag26 && npc.velocity.Y == 0f && Math.Abs(targetData.Center.X - npc.Center.X) < 100f && Math.Abs(targetData.Center.Y - npc.Center.Y) < 50f && Math.Abs(npc.velocity.X) >= 1f && npc.velocity.X * (float)npc.direction > 0f) {
                    npc.velocity.X = MathHelper.Clamp(npc.velocity.X * 2f, -3f, 3f);
                    npc.velocity.Y = -4f;
                    npc.netAlways = true;
                }
            }
        }
        else if (flag8) {
            npc.ai[0] = 0f;
            npc.ai[1] = 0f;
        }

        if (!flag2 || !npc.noTileCollide)
            return;

        npc.wet = false;
        if (flag29) {
            if (npc.alpha < 60)
                npc.alpha += 20;

            npc.localAI[3] = 40f;
        }

        bool num70 = npc.velocity.Y == 0f;
        if (Math.Abs(npc.Center.X - targetData.Center.X) > 200f) {
            npc.spriteDirection = (npc.direction = ((targetData.Center.X > npc.Center.X) ? 1 : (-1)));
            npc.velocity.X = MathHelper.Lerp(npc.velocity.X, npc.direction, 0.05f);
        }

        int num71 = 80;
        int num72 = npc.height;
        Vector2 vector14 = new Vector2(npc.Center.X - (float)(num71 / 2), npc.position.Y + (float)npc.height - (float)num72);
        bool flag40 = false;
        if (npc.position.Y + (float)npc.height < targetData.Position.Y + (float)targetData.Height - 16f)
            flag40 = true;

        if (flag40) {
            npc.velocity.Y += 0.5f;
        }
        else if (Collision.SolidCollision(vector14, num71, num72) || targetData.Center.Y - npc.Center.Y < -100f || (targetData.Center.Y - npc.Center.Y < 10f && Math.Abs(targetData.Center.X - npc.Center.X) < 60f)) {
            if (npc.velocity.Y > 0f)
                npc.velocity.Y = 0f;

            if ((double)npc.velocity.Y > -0.2)
                npc.velocity.Y -= 0.025f;
            else
                npc.velocity.Y -= 0.2f;

            if (npc.velocity.Y < -4f)
                npc.velocity.Y = -4f;
        }
        else {
            if (npc.velocity.Y < 0f)
                npc.velocity.Y = 0f;

            if ((double)npc.velocity.Y < 0.1)
                npc.velocity.Y += 0.025f;
            else
                npc.velocity.Y += 0.5f;
        }

        if (npc.velocity.Y > 10f)
            npc.velocity.Y = 10f;

        if (num70)
            npc.velocity.Y = 0f;
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
    
