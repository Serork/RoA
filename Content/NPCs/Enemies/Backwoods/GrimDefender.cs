using Microsoft.Xna.Framework;

using RoA.Utilities;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class GrimDefender : ModNPC {
    private Vector2 _tempPosition, _extraVelocity;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;
    }

    public override void SetDefaults() {
        NPC.lifeMax = 80;
        NPC.damage = 22;
        NPC.defense = 3;

        int width = 22; int height = 28;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        //NPC.npcSlots = 0.8f;
        //NPC.value = Item.buyPrice(0, 0, 0, 75);

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
    }


    public override void FindFrame(int frameHeight) {
        if (NPC.ai[0] == 0f) {
            float num = 130f;
            if (NPC.ai[1] <= num * 0.5f) {
                NPC.frame.Y = 0;
            }
            else if (++NPC.frameCounter > 6 && NPC.localAI[0] < 3) {
                NPC.frameCounter = 0.0;
                NPC.localAI[0]++;
            }
            NPC.frame.Y = frameHeight * (int)NPC.localAI[0];
        }
        else if (NPC.ai[0] >= 1f) {
            NPC.frame.Y = frameHeight * 3;
        }

        NPC.spriteDirection = NPC.direction;
    }

    public override void AI() {
        NPC.noTileCollide = NPC.noGravity = true;

        bool flag = true;
        float minDist = 150f;
        float value = (float)Math.Pow(0.99, 2.0);
        Vector2 diff = Main.player[NPC.target].Center - NPC.Center;
        diff.Normalize();
        void directedRotation(Vector2 destination) {
            flag = false;
            float rotation = Helper.VelocityAngle(destination.SafeNormalize(Vector2.Zero)) - MathHelper.PiOver2;
            if (NPC.direction == -1) {
                rotation -= MathHelper.Pi;
            }
            NPC.rotation = Utils.AngleLerp(NPC.rotation, rotation, Math.Abs(rotation) * 0.075f + 0.05f);
        }
        if (NPC.ai[0] > 1f) {
            NPC.knockBackResist = 0f;
            if (NPC.velocity.X > 0.5f) {
                NPC.spriteDirection = -1;
            }
            if (NPC.velocity.X < -0.5f) {
                NPC.spriteDirection = 1;
            }
            NPC.ai[1]++;
            Vector2 desiredVelocity = new(NPC.ai[2], NPC.ai[3]);
            directedRotation(desiredVelocity);
            _extraVelocity *= value;
            NPC.ai[0] = MathHelper.Lerp(NPC.ai[0], (float)Math.Sqrt(Math.Abs(NPC.ai[2] * NPC.ai[3])), 0.01f);
            NPC.ai[2] *= value;
            NPC.ai[3] *= value;
            NPC.velocity = Vector2.SmoothStep(NPC.velocity, desiredVelocity, NPC.ai[0] - 2f);
            NPC.velocity += (Utils.MoveTowards(NPC.Center, Main.player[NPC.target].Center, NPC.ai[0]) - NPC.Center) * MathHelper.Clamp(NPC.ai[0], 0f, 0.2f) * 0.6f;
            if (NPC.ai[1] >= 60f || NPC.justHit) {
                NPC.ai[0] = 0f;
                NPC.ai[1] = NPC.justHit ? (60f - NPC.ai[1]) : 0f;
            }
        }
        else if (NPC.ai[0] == 1f && NPC.ai[1] >= 100f) {
            flag = false;
            NPC.ai[1] = 0f;
            NPC.ai[0] = 2f;
            Vector2 desiredVelocity = diff * 8f;
            NPC.ai[2] = desiredVelocity.X;
            NPC.ai[3] = desiredVelocity.Y;
            NPC.localAI[0] = 0f;
            NPC.frameCounter = 0.0;
        }
        else {
            NPC.TargetClosest();
            Vector2 playerCenter = Main.player[NPC.target].Center;
            float dist = NPC.Distance(playerCenter);
            float inertia = 22f;
            float speed = 5f;
            NPC.knockBackResist = 0.9f;

            float num = 130f;
            if (NPC.ai[1] < num) {
                NPC.ai[1]++;
                if (dist <= minDist * 1.25f) {
                    Helper.InertiaMoveTowards(ref NPC.velocity, NPC.Center, NPC.Center - (playerCenter - NPC.Center).SafeNormalize(Vector2.Zero) * minDist, inertia * 0.5f, speed * 0.5f, minDist);
                }
                if (dist > minDist) {
                    Helper.InertiaMoveTowards(ref NPC.velocity, NPC.Center, playerCenter, inertia * 0.75f, speed * 0.75f, minDist);
                }
                _tempPosition = NPC.Center;
                float num269 = Math.Abs(NPC.position.X + (float)(NPC.width / 2) - (Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2)));
                float num270 = Main.player[NPC.target].position.Y - (float)(NPC.height / 2);
                if (num269 > 50f)
                    num270 -= 100f;

                if (NPC.position.Y < num270) {
                    _extraVelocity.Y += 0.035f;
                    if (_extraVelocity.Y < 0f)
                        _extraVelocity.Y += 0.01f;
                }
                else {
                    _extraVelocity.Y -= 0.035f;
                    if (_extraVelocity.Y > 0f)
                        _extraVelocity.Y -= 0.01f;
                }

                if (_extraVelocity.Y < -1.35f)
                    _extraVelocity.Y = -1.35f;

                if (_extraVelocity.Y > 1.35f)
                    _extraVelocity.Y = 1.35f;
            }

            NPC.velocity += _extraVelocity * 0.1f;

            diff = Main.player[NPC.target].Center - Vector2.UnitY * 30f - NPC.Center;
            directedRotation(diff);

            if (NPC.ai[1] >= num) {
                //_extraVelocity *= value;
                //value = (float)Math.Pow(0.97, 2.0);
                //NPC.velocity *= value;
                Helper.InertiaMoveTowards(ref NPC.velocity, NPC.Center, _tempPosition - (NPC.Center - _tempPosition).SafeNormalize(Vector2.Zero) * minDist, inertia, speed, minDist, true);
                NPC.ai[1]++;
                NPC.ai[0] = 1f;
            }
        }

        if (flag) {
            NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.X * 0.1f, 0.1f);
        }
    }
}
