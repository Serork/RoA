using System;

using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

using Microsoft.Xna.Framework;

using RoA.Content.Tiles.Platforms;
using RoA.Utilities;
using System.Collections.Generic;
using System.Linq;
using RoA.Core.Utility;
using System.IO;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class Fleder : ModNPC {
    private enum State {
        Normal, 
        Sitting,
        Attacking
    }

    private State _state = State.Normal;
    private Vector2 _sittingPosition = Vector2.Zero;

    private bool IsSittingOnBranch => _state == State.Sitting && _sittingPosition != Vector2.Zero;
    public bool IsAttacking => _state == State.Attacking;

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write((int)_state);
        writer.WriteVector2(_sittingPosition);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _state = (State)reader.ReadInt32();
        _sittingPosition = reader.ReadVector2();
    }

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
    }

    public override void SetDefaults() {
        NPC.lifeMax = 80;
        NPC.damage = 22;
        NPC.defense = 3;
        NPC.knockBackResist = 0.5f;

        int width = 50; int height = 30;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.npcSlots = 1f;

        NPC.value = Item.buyPrice(0, 0, 1, 5);

        NPC.HitSound = SoundID.NPCHit27;
        NPC.DeathSound = SoundID.NPCDeath39;

        NPC.noGravity = true;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) => target.AddBuff(BuffID.Bleeding, Main.expertMode ? 135 : 85);

    public override bool? CanFallThroughPlatforms() => true;

    public override void AI() {
        NPC.OffsetTheSameNPC();

        float rotation = NPC.rotation;
        if (NPC.velocity.Y != 0f && Math.Abs(NPC.velocity.X) > 0.05f) {
            rotation = NPC.velocity.X * (MathHelper.PiOver2 * 0.135f);
        }
        rotation = (float)Math.Sin(rotation) * (float)Math.PI * 0.12f;
        NPC.rotation = rotation;

        Rectangle playerRect;
        Rectangle npcRect = new((int)NPC.position.X - 200, (int)NPC.position.Y - 150, NPC.width + 400, NPC.height + 300);
        bool isTriggeredBy(Player player) {
            playerRect = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
            return (npcRect.Intersects(playerRect) || NPC.life < NPC.lifeMax) && !player.dead && player.active;
        }
        void flyAway() {
            float maxSpeed = 4.5f;
            if (NPC.velocity.Y < -maxSpeed) {
                NPC.velocity.Y = -maxSpeed;
            }
            float speedY = 0.075f * 0.6f;
            NPC.velocity.Y -= speedY + speedY / 2f * Main.rand.NextFloat();
            float speedX = NPC.direction * 0.1f * 0.6f;
            NPC.velocity.X += speedX + speedX / 2f * Main.rand.NextFloat();
            if (NPC.velocity.X < -maxSpeed) {
                NPC.velocity.X = -maxSpeed;
            }
            if (NPC.velocity.X > maxSpeed) {
                NPC.velocity.X = maxSpeed;
            }
        }
        if (IsSittingOnBranch) {
            NPC.Center = _sittingPosition;
            NPC.velocity = Vector2.Zero;

            if (Main.netMode != NetmodeID.MultiplayerClient) {
                foreach (Player activePlayer in Main.ActivePlayers) {
                    if (isTriggeredBy(activePlayer)) {
                        NPC.target = activePlayer.whoAmI;
                        _state = State.Attacking;

                        NPC.velocity.Y -= 5f;

                        NPC.netUpdate = true;
                    }
                }
            }

            return;
        }

        Player player = Main.player[NPC.target];
        playerRect = new((int)player.position.X, (int)player.position.Y, player.width, player.height);
        if (!IsAttacking) {
            List<NPC> others = Main.npc.Where(npc => npc.active && npc.type == NPC.type && npc.whoAmI != NPC.whoAmI).ToList();
            float nearestTile = NPC.SearchForNearestTile<TreeBranch>(out Point tile, out Point? treeBranch, (tilePosition) => {
                if (others.Any(npc => npc.As<Fleder>()._state == State.Sitting && npc.WithinRange(tilePosition.ToWorldCoordinates(), 10f))) {
                    return false;
                }

                return true;
            }, 30);
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                foreach (Player activePlayer in Main.ActivePlayers) {
                    if (isTriggeredBy(activePlayer)) {
                        NPC.target = activePlayer.whoAmI;
                        _state = State.Attacking;

                        NPC.netUpdate = true;
                    }
                }
            }
            if (nearestTile >= 25 * 25 && treeBranch != null && NPC.life >= NPC.lifeMax * 0.5f) {
                NPC.noTileCollide = true;

                NPC.localAI[1] = 0f;

                Vector2 destination = treeBranch.Value.ToWorldCoordinates();
                if (Collision.CanHitLine(NPC.Center, 0, 0, destination, 2, 2)) {
                    Helper.InertiaMoveTowards(ref NPC.velocity, NPC.Center, destination);
                }
                else {
                    if (NPC.velocity.LengthSquared() < 1f) {
                        NPC.velocity = new Vector2(1.2f, 0f).RotatedByRandom(MathHelper.TwoPi);
                    }
                    else {
                        NPC.velocity = NPC.velocity.RotatedBy(Main.rand.Next(-1, 2) * 0.2f);
                    }
                }

                if (NPC.WithinRange(destination, 8f) && Math.Abs(NPC.Center.X - destination.X) <= 6f) {
                    NPC.Center = destination;
                    NPC.velocity = Vector2.Zero;
                    NPC.rotation = 0f;

                    _sittingPosition = destination;
                    _state = State.Sitting;
                }

                if (Math.Abs(NPC.velocity.X) > 0.05f) {
                    NPC.direction = NPC.velocity.X.GetDirection();
                }

                return;
            }
            else if (player.dead || !player.active) {
                flyAway();
            }
        }

        NPC.localAI[1] += 1f;
        if (player.dead || !player.active) {
            NPC.TargetClosest();
            if (player.dead || !player.active) {
                _state = State.Normal;
            }
        }

        NPC.noTileCollide = false;

        if (!player.dead) {
            if (NPC.localAI[1] > 100f) {
                _state = State.Attacking;
            }

            NPC.TargetClosest();

            npcRect = new((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height);
            if (npcRect.Intersects(playerRect) || NPC.collideX) {
                float maxSpeedAfterCollideX = 4f;
                float speedAfterCollideX = 2.5f;
                NPC.velocity.X = NPC.oldVelocity.X * -0.5f;
                if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < maxSpeedAfterCollideX) {
                    NPC.velocity.X = speedAfterCollideX;
                }
                if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -maxSpeedAfterCollideX) {
                    NPC.velocity.X = -speedAfterCollideX;
                }
            }
            if (npcRect.Intersects(playerRect) || NPC.collideY) {
                float maxSpeedAfterCollideY = 5f;
                float speedAfterCollideY = 3f;
                NPC.velocity.Y = NPC.oldVelocity.Y * -0.5f;
                if (NPC.velocity.Y > 0f && NPC.velocity.Y < maxSpeedAfterCollideY) {
                    NPC.velocity.Y = speedAfterCollideY;
                }
                if (NPC.velocity.Y < 0f && NPC.velocity.Y > -maxSpeedAfterCollideY) {
                    NPC.velocity.Y = -speedAfterCollideY;
                }
            }

            NPC.TargetClosest(true);
            float speedX = 0.15f;
            float maxSpeedX = 5f;
            if (NPC.direction == -1 && NPC.velocity.X > -maxSpeedX) {
                NPC.velocity.X -= speedX;
                if (NPC.velocity.X > maxSpeedX) {
                    NPC.velocity.X -= 1f;
                }
                else if (NPC.velocity.X > 0f) {
                    NPC.velocity.X -= speedX / 5f;
                }
                if (NPC.velocity.X < -maxSpeedX) {
                    NPC.velocity.X = -maxSpeedX;
                }
            }
            else if (NPC.direction == 1 && NPC.velocity.X < maxSpeedX) {
                NPC.velocity.X += speedX;
                if (NPC.velocity.X < -maxSpeedX) {
                    NPC.velocity.X += speedX;
                }
                else if (NPC.velocity.X < 0f) {
                    NPC.velocity.X += speedX / 5f;
                }
                if (NPC.velocity.X > maxSpeedX) {
                    NPC.velocity.X = maxSpeedX;
                }
            }
            float distX = Math.Abs((float)(NPC.position.X + (double)(NPC.width / 2) - ((player.position.X - player.oldVelocity.Y) + (double)(player.width / 2))));
            float distY = player.position.Y - NPC.height / 2;
            float minX = 50f;
            float upY = 200f;
            if (distX > minX && player.FindBuffIndex(BuffID.Bleeding) == -1) {
                distY -= upY;
            }
            float speedY = 0.15f;
            if (NPC.position.Y < distY) {
                NPC.velocity.Y += speedY;
                if (NPC.velocity.Y < 0f) {
                    NPC.velocity.Y += speedY / 5f;
                }
            }
            else {
                NPC.velocity.Y -= speedY;
                if (NPC.velocity.Y > 0f) {
                    NPC.velocity.Y -= speedY / 5f;
                }
            }
            float maxSpeedY = 5f;
            if (NPC.velocity.Y < -maxSpeedY) {
                NPC.velocity.Y = -maxSpeedY;
            }
            if (NPC.velocity.Y > maxSpeedY) {
                NPC.velocity.Y = maxSpeedY;
            }
        }
        else {
            flyAway();
        }
    }

    public override void FindFrame(int frameHeight) {
        if (_state == State.Sitting) {
            NPC.frame.Y = 2 * frameHeight;

            return;
        }

        NPC.spriteDirection = NPC.direction;
        if (++NPC.frameCounter >= (double)Math.Max(10f - Math.Abs(NPC.velocity.Y) * 2f, 4f)) {
            NPC.frameCounter = 0.0;
            NPC.localAI[0]++;
            if (NPC.localAI[0] >= 4) {
                NPC.localAI[0] = 0;
            }
            int currentFrame = (int)NPC.localAI[0];
            NPC.frame.Y = currentFrame * frameHeight;
        }
    }
}
