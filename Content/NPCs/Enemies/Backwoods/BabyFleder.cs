using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using RoA.Common;
using RoA.Core.Utility;
using RoA.Utilities;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class BabyFleder : ModNPC {
    private enum State {
        Normal,
        Sitting
    }

    private State _state = State.Sitting;

    public bool HasParent => ParentIndex > -1;

    private bool IsSitting => _state == State.Sitting;

    public int ParentIndex {
        get => (int)NPC.ai[2] - 1;
        set => NPC.ai[2] = value + 1;
    }

    private ref float StateTimer => ref NPC.ai[0];
    private ref float AITimer => ref NPC.ai[3];

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write((int)_state);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _state = (State)reader.ReadInt32();
    }

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
    }

    public override void SetDefaults() {
        NPC.lifeMax = 80;
        NPC.damage = 22;
        NPC.defense = 3;
        NPC.knockBackResist = 0.9f;

        int width = 25; int height = 25;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.npcSlots = 0.75f;
        NPC.value = Item.buyPrice(0, 0, 0, 75);

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
        if (NPC.downedBoss2) {
            target.AddBuff(BuffID.Bleeding, Main.expertMode ? 60 : 30);
        }
    }

    public override bool? CanFallThroughPlatforms() => true;

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = NPC.direction;
        if (IsSitting && Math.Abs(NPC.velocity.Y) < 0.1f) {
            NPC.frame.Y = 2 * frameHeight;
            return;
        }
        if (++NPC.frameCounter >= (double)Math.Max(10f - Math.Abs(NPC.velocity.Y) * 3.5f, 6f)) {
            NPC.frameCounter = 0;
            NPC.localAI[0]++;
            if (NPC.localAI[0] >= 4) {
                NPC.localAI[0] = 0;
            }
        }
        int currentFrame = (int)NPC.localAI[0];
        NPC.frame.Y = currentFrame * frameHeight;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        spriteBatch.Draw(texture, NPC.position - screenPos + new Vector2(0f, IsSitting ? 3f : 0f) + new Vector2(NPC.width, NPC.height) / 2, NPC.frame, drawColor, NPC.rotation, new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) / 2, NPC.scale, NPC.spriteDirection != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        
        return false;
    }

    public override void AI() {
        if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
            NPC.TargetClosest();
        }
        int target = NPC.target;
        int direction = NPC.direction;
        Player player = Main.player[target];
        NPC parent = HasParent ? Main.npc[ParentIndex] : new NPC();
        bool hasParent = HasParent && parent.active;
        Vector2 position = hasParent ? parent.position : player.position;
        Vector2 center = hasParent ? parent.Center : player.Center;
        if (NPC.velocity.X >= 5f) {
            NPC.velocity.X = 5f;
        }
        else if (NPC.velocity.X <= -5f) {
            NPC.velocity.X = -5f;
        }
        if (hasParent && parent.As<Fleder>().IsAttacking) {
            ResetParentState();
        }
        void move(bool flag) {
            if (!HasParent) {
                if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
                    AITimer = 0f;
                }
                AITimer += 1f;
                if (AITimer >= 15f && Main.rand.NextChance(AITimer / 200f)) {
                    NPC.noGravity = false;
                    NPC.velocity.Y += 0.15f;
                    NPC.velocity.Y = Math.Min(3f, NPC.velocity.Y);
                    if (Main.tile[(int)NPC.Bottom.X / 16, (int)NPC.Bottom.Y / 16 + 1].HasTile || NPC.collideY) {
                        AITimer = 0f;
                        _state = State.Sitting;

                        NPC.netUpdate = true;
                    }
                }
                NPC.netUpdate = true;
            }
            hasParent = flag;

            float accelerationX = 0.05f;
            float accelerationY = 0.02f;
            float maxAcceleration = 2f;
            float maxSpeed = 2f;
            float distance = 40f;
            float maxDistance = 350f;
            float distanceBetween = Math.Abs(NPC.position.X + NPC.width / 2 - (position.X + player.width / 2));
            float currentMovement = center.Y - NPC.height / 2;
            bool isMovingRight = (position.X >= NPC.position.X + 5f && NPC.velocity.X <= -0.5f) || (position.X <= NPC.position.X - 5f && NPC.velocity.X >= 0.5f);
            bool flag2 = ParentIndex != -1f && HasParent && hasParent;
            NPC.noTileCollide = false;
            if (flag2) {
                NPC.SlightlyMoveTo(center, 5f, 40f);
                if (NPC.target == 255 || player.dead || Collision.CanHit(NPC.Center, 1, 1, center, 1, 1)) {
                    StateTimer -= 1f;
                    NPC.TargetClosest(false);
                    NPC.netUpdate = true;
                }
                else if (StateTimer > 0f) {
                    StateTimer = 90f;
                    NPC.TargetClosest(false);

                    NPC.netUpdate = true;
                }
                if (distanceBetween > maxDistance) {
                    NPC.SlightlyMoveTo(position, 25f, 20f);
                    NPC.noTileCollide = true;
                }
                if (StateTimer <= 0f) {
                    currentMovement = NPC.Center.Y + NPC.directionY * 1000;
                    if (NPC.velocity.X < 0f) {
                        NPC.direction = -1;
                    }
                    else if (NPC.velocity.X > 0f || NPC.direction == 0) {
                        NPC.direction = 1;
                    }
                    if (NPC.position.Y < currentMovement) {
                        NPC.velocity.Y += accelerationY;
                        if (NPC.velocity.Y < 0f) {
                            NPC.velocity.Y += accelerationY;
                        }
                    }
                    else {
                        NPC.velocity.Y -= accelerationY;
                        if (NPC.velocity.Y > 0f) {
                            NPC.velocity.Y -= accelerationY;
                        }
                    }
                }
            }
            currentMovement -= maxDistance / 2f;
            if (NPC.direction == -1 && NPC.velocity.X > 0f - maxAcceleration) {
                NPC.velocity.X -= accelerationX;
                if (NPC.velocity.X > maxAcceleration) {
                    NPC.velocity.X -= accelerationX;
                }
                else if (NPC.velocity.X > 0f) {
                    NPC.velocity.X -= accelerationX / 2f;
                }
                if (NPC.velocity.X < 0f - maxAcceleration) {
                    NPC.velocity.X = 0f - maxAcceleration;
                }
            }
            else if (NPC.direction == 1 && NPC.velocity.X < maxAcceleration) {
                NPC.velocity.X += accelerationX;
                if (NPC.velocity.X < 0f - maxAcceleration) {
                    NPC.velocity.X += accelerationX;
                }
                else if (NPC.velocity.X < 0f) {
                    NPC.velocity.X += accelerationX / 2f;
                }
                if (NPC.velocity.X > maxAcceleration) {
                    NPC.velocity.X = maxAcceleration;
                }
            }
            if (flag2 && !(distanceBetween > maxDistance || distanceBetween < distance)) {
                Vector2 position2 = Vector2.Normalize(center - NPC.Center - NPC.velocity) * 5f;
                float speed2 = 0.1f;
                float maxSpeed2 = 1.5f;
                if (NPC.velocity.X < position2.X) {
                    if (NPC.velocity.X > maxSpeed2) {
                        NPC.velocity.X = maxSpeed2;
                    }
                    NPC.velocity.X += speed2;
                    if (NPC.velocity.X < 0f && position2.X > 0f) {
                        NPC.velocity.X += speed2;
                    }
                }
                else if (NPC.velocity.X > position2.X) {
                    if (NPC.velocity.X < -maxSpeed2) {
                        NPC.velocity.X = -maxSpeed2;
                    }
                    NPC.velocity.X -= speed2;
                    if (NPC.velocity.X > 0f && position2.X < 0f) {
                        NPC.velocity.X -= speed2;
                    }
                }
                else if (NPC.velocity.X > 0f) {
                    NPC.velocity.X -= speed2;
                }
                else if (NPC.velocity.X < 0f) {
                    NPC.velocity.X += speed2;
                }
                else {
                    NPC.velocity.X = 0f;

                }
                speed2 += 0.025f;
                if (NPC.velocity.Y < position2.Y) {
                    if (NPC.velocity.Y > maxSpeed2) {
                        NPC.velocity.Y = maxSpeed2;
                    }
                    NPC.velocity.Y += speed2;
                    if (NPC.velocity.Y < 0f && position2.Y > 0f) {
                        NPC.velocity.Y += speed2;
                    }
                }
                else if (NPC.velocity.Y > position2.Y) {
                    if (NPC.velocity.Y < -maxSpeed2) {
                        NPC.velocity.Y = -maxSpeed2;
                    }
                    NPC.velocity.Y -= speed2;
                    if (NPC.velocity.Y > 0f && position2.Y < 0f) {
                        NPC.velocity.Y -= speed2;
                    }
                }
                else if (NPC.velocity.Y > 0f) {
                    NPC.velocity.Y -= speed2;
                }
                else if (NPC.velocity.Y < 0f) {
                    NPC.velocity.Y += speed2;
                }
                else {
                    NPC.velocity.Y = 0f;
                }
            }
            if (NPC.position.Y < currentMovement) {
                NPC.velocity.Y += accelerationY;
                if (NPC.velocity.Y < 0f) {
                    NPC.velocity.Y += accelerationY;
                }
            }
            else {
                NPC.velocity.Y -= accelerationY;
                if (NPC.velocity.Y > 0f) {
                    NPC.velocity.Y -= accelerationY;
                }
            }
            if (NPC.velocity.Y < 0f - maxSpeed) {
                NPC.velocity.Y = 0f - maxSpeed;
            }
            if (NPC.velocity.Y > maxSpeed) {
                NPC.velocity.Y = maxSpeed;
            }
        }
        if (Math.Abs(NPC.velocity.X) > 0.025f) {
            NPC.rotation = NPC.velocity.X * 0.1f;
        }
        else {
            NPC.rotation = 0f;
        }
        NPC.OffsetTheSameNPC(0.2f);
        NPC.noGravity = true;
        bool flag4 = NPC.Distance(player.Center) < 150f;
        if (!hasParent) {
            ResetParentState();
            if (IsSitting) {
                NPC.velocity *= 0.9f;
                if (player.dead) {
                    NPC.noGravity = false;
                }
                else {
                    Rectangle playerRect = new((int)player.position.X, (int)player.position.Y, player.width, player.height);
                    Rectangle npcRect = new((int)NPC.position.X - 200, (int)NPC.position.Y - 150, NPC.width + 400, NPC.height + 300);
                    if (flag4 || ((npcRect.Intersects(playerRect) && Collision.CanHit(NPC.Center, 1, 1, center, 1, 1)) || NPC.life < NPC.lifeMax)) {
                        _state = State.Normal;
                        NPC.velocity.Y -= 1f;
                        AITimer = 0f;

                        NPC.netUpdate = true;
                    }
                }
            }
            else {
                bool flag3 = NPC.direction == 1 && NPC.velocity.X > 0f && NPC.position.X < player.position.X | NPC.direction != 1 && NPC.velocity.X < 0f && NPC.position.X > player.position.X;
                if (Collision.CanHit(NPC.Center, 1, 1, center, 1, 1) || flag3 || flag4) {
                    NPC.BasicFlier(0.4f, 0.2f, 4f, 2f);
                    NPC.velocity.X *= 0.9f;
                    if (NPC.velocity.LengthSquared() > 1f) {
                        NPC.velocity.Y *= 0.9f;
                    }
                    NPC.TargetClosest();
                }
                else {
                    hasParent = false;
                    move(hasParent);
                }
            }
        }
        else {
            if (_state != State.Normal) {
                _state = State.Normal;

                NPC.netUpdate = true;
            }
            move(true);
        }

        if (IsSitting) {
            int x = (int)(NPC.Center.X / 16f);
            int y = (int)(NPC.Center.Y / 16f);
            Rectangle tileRect2 = new(x * 16, y * 16, 16, 16);
            if (Main.tile[x, y].HasTile && NPC.frame.Intersects(tileRect2)) {
                NPC.position.Y -= 0.2f;
            }
            NPC.noGravity = false;
            NPC.velocity.Y += 0.15f;
            NPC.velocity.Y = Math.Min(3f, NPC.velocity.Y);
        }
        else {
            NPC.noGravity = true;
        }

        NPC.netUpdate = true;
    }

    private void ResetParentState() {
        if (HasParent) {
            ParentIndex = -1;

            NPC.netUpdate = true;
        }
    }
}