using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

sealed class ElderSnail : ModNPC {
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Collision_MoveSnailOnSlopes")]
    public extern static void NPC_Collision_MoveSnailOnSlopes(NPC self);

    private static byte FRAMECOUNT => 10;
    private static float UPDATEDIRECTIONEVERYNTICKS => 10f;
    private static float UPDATETARGETEVERYNTICKS => 120f;

    private float _targetTimer;
    private float _speedX;

    private bool IsFalling => NPC.ai[2] > 0f;
    private int FacedDirection => (int)-NPC.ai[3];

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(36, 36);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;

        NPC.noGravity = true;
    }

    public override void AI() {
        ApplySnailAI();
    }

    public override bool CheckActive() {
        NPC_Collision_MoveSnailOnSlopes(NPC);

        return base.CheckActive();
    }

    private void ApplySnailAI() {
        Player target = NPC.GetTargetPlayer();
        if (Collision.CanHit(NPC.Center, 0, 0, target.Center, 0, 0)) {
            _targetTimer++;
        }
        if (_targetTimer > UPDATETARGETEVERYNTICKS) {
            _targetTimer = 0f;
            NPC.ai[0] = 0f;
            NPC.localAI[0] = UPDATEDIRECTIONEVERYNTICKS;
        }

        _speedX += 0.75f;
        float max = 30f;
        if (_speedX > max) {
            _speedX = 0f;
        }
        float progress = Ease.CubeInOut(Utils.GetLerpValue(max * 0.25f, max, _speedX, true));
        float speedX = 0.1f + 1.5f * progress,
              speedY = 0.75f + 0.5f * progress;

        if (NPC.ai[0] == 0f) {
            NPC.TargetClosest();
            NPC.directionY = 1;
            NPC.ai[0] = 1f;
            if (NPC.direction > 0)
                NPC.spriteDirection = 1;
        }

        bool flag53 = false;
        if (Main.netMode != 1) {
            if (NPC.ai[2] == 0f && Main.rand.Next(7200) == 0) {
                NPC.ai[2] = 2f;
                NPC.netUpdate = true;
            }

            if (!NPC.collideX && !NPC.collideY) {
                NPC.localAI[3] += 1f;
                if (NPC.localAI[3] > 5f) {
                    NPC.ai[2] = 2f;
                    NPC.netUpdate = true;
                }
            }
            else {
                NPC.localAI[3] = 0f;
            }
        }

        if (IsFalling) {
            NPC.ai[1] = 0f;
            NPC.ai[0] = 1f;
            NPC.directionY = 1;
            if (NPC.velocity.Y > speedX)
                NPC.rotation += (float)NPC.direction * 0.1f;
            else
                NPC.rotation = 0f;

            NPC.spriteDirection = NPC.direction;
            NPC.velocity.X = speedX * (float)NPC.direction;
            NPC.noGravity = false;
            int num1042 = (int)(NPC.Center.X + (float)(NPC.width / 2 * -NPC.direction)) / 16;
            int num1043 = (int)(NPC.position.Y + (float)NPC.height + 8f) / 16;
            if (Main.tile[num1042, num1043] != null && !Main.tile[num1042, num1043].TopSlope && NPC.collideY)
                NPC.ai[2] -= 1f;

            num1043 = (int)(NPC.position.Y + (float)NPC.height - 4f) / 16;
            num1042 = (int)(NPC.Center.X + (float)(NPC.width / 2 * NPC.direction)) / 16;
            if (Main.tile[num1042, num1043] != null && Main.tile[num1042, num1043].BottomSlope)
                NPC.direction *= -1;

            if (NPC.collideX && NPC.velocity.Y == 0f) {
                flag53 = true;
                NPC.ai[2] = 0f;
                NPC.directionY = -1;
                NPC.ai[1] = 1f;
            }

            if (NPC.velocity.Y == 0f) {
                if (NPC.localAI[1] == NPC.position.X) {
                    NPC.localAI[2] += 1f;
                    if (NPC.localAI[2] > 10f) {
                        NPC.direction = 1;
                        NPC.velocity.X = (float)NPC.direction * speedX;
                        NPC.localAI[2] = 0f;
                    }
                }
                else {
                    NPC.localAI[2] = 0f;
                    NPC.localAI[1] = NPC.position.X;
                }
            }
        }

        if (NPC.ai[2] != 0f)
            return;

        NPC.noGravity = true;
        if (NPC.ai[1] == 0f) {
            if (NPC.collideY)
                NPC.ai[0] = 2f;

            if (!NPC.collideY && NPC.ai[0] == 2f) {
                NPC.direction = -NPC.direction;
                NPC.ai[1] = 1f;
                NPC.ai[0] = 1f;
            }

            if (NPC.collideX) {
                NPC.directionY = -NPC.directionY;
                NPC.ai[1] = 1f;
            }
        }
        else {
            if (NPC.collideX)
                NPC.ai[0] = 2f;

            if (!NPC.collideX && NPC.ai[0] == 2f) {
                NPC.directionY = -NPC.directionY;
                NPC.ai[1] = 0f;
                NPC.ai[0] = 1f;
            }

            if (NPC.collideY) {
                NPC.direction = -NPC.direction;
                NPC.ai[1] = 0f;
            }
        }

        if (!flag53) {
            float num1044 = NPC.rotation;
            if (NPC.directionY < 0) {
                if (NPC.direction < 0) {
                    if (NPC.collideX) {
                        NPC.rotation = 1.57f;
                        NPC.spriteDirection = -1;
                    }
                    else if (NPC.collideY) {
                        NPC.rotation = 3.14f;
                        NPC.spriteDirection = 1;
                    }
                }
                else if (NPC.collideY) {
                    NPC.rotation = 3.14f;
                    NPC.spriteDirection = -1;
                }
                else if (NPC.collideX) {
                    NPC.rotation = 4.71f;
                    NPC.spriteDirection = 1;
                }
            }
            else if (NPC.direction < 0) {
                if (NPC.collideY) {
                    NPC.rotation = 0f;
                    NPC.spriteDirection = -1;
                }
                else if (NPC.collideX) {
                    NPC.rotation = 1.57f;
                    NPC.spriteDirection = 1;
                }
            }
            else if (NPC.collideX) {
                NPC.rotation = 4.71f;
                NPC.spriteDirection = -1;
            }
            else if (NPC.collideY) {
                NPC.rotation = 0f;
                NPC.spriteDirection = 1;
            }

            float num1045 = NPC.rotation;
            NPC.rotation = num1044;
            if ((double)NPC.rotation > 6.28)
                NPC.rotation -= 6.28f;

            if (NPC.rotation < 0f)
                NPC.rotation += 6.28f;

            float num1046 = Math.Abs(NPC.rotation - num1045);
            float num1047 = 0.05f;
            if (NPC.rotation > num1045) {
                if ((double)num1046 > 3.14) {
                    NPC.rotation += num1047;
                }
                else {
                    NPC.rotation -= num1047;
                    if (NPC.rotation < num1045)
                        NPC.rotation = num1045;
                }
            }

            if (NPC.rotation < num1045) {
                if ((double)num1046 > 3.14) {
                    NPC.rotation -= num1047;
                }
                else {
                    NPC.rotation += num1047;
                    if (NPC.rotation > num1045)
                        NPC.rotation = num1045;
                }
            }
        }

        if (NPC.localAI[0]++ > UPDATEDIRECTIONEVERYNTICKS) {
            NPC.localAI[0] = 0f;

            NPC.ai[3] = NPC.spriteDirection;
        }

        NPC.velocity.X = speedX * (float)NPC.direction;
        NPC.velocity.Y = speedY * (float)NPC.directionY;
    }

    public override void FindFrame(int frameHeight) {
        
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        SpriteEffects flip = FacedDirection.ToSpriteEffects();
        NPC.QuickDraw(spriteBatch, screenPos, drawColor, effect: flip);

        return false;
    }
}
