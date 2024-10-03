using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Utilities;

using System;
using System.IO;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class GrimDefender : ModNPC {
    private const float ATTACKTIME = 145f;

    private Vector2 _tempPosition, _extraVelocity, _extraVelocity2;
    private bool _spearAttack;

    public override void SendExtraAI(BinaryWriter writer) {
        writer.WriteVector2(_tempPosition);
        writer.WriteVector2(_extraVelocity);
        writer.WriteVector2(_extraVelocity2);
        writer.Write(_spearAttack);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _tempPosition = reader.ReadVector2();
        _extraVelocity = reader.ReadVector2();
        _extraVelocity2 = reader.ReadVector2();
        _spearAttack = reader.ReadBoolean();
    }

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 8;
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

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
        position = new (NPC.position.X + 22 / 2, NPC.position.Y + NPC.gfxOffY);
        position.X -= 11;
        position.Y -= 9;
        if (Main.HealthBarDrawSettings == 1) {
            position.Y += 28 + 10f + Main.NPCAddHeight(NPC);
        }
        else if (Main.HealthBarDrawSettings == 2) {
            position.Y -= 24f + Main.NPCAddHeight(NPC) / 2f;
        }

        return true;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Main.EntitySpriteDraw(texture, NPC.position - Main.screenPosition, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2f, NPC.scale, NPC.spriteDirection != -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

        return false;
    }

    public override void FindFrame(int frameHeight) {
        if (NPC.ai[0] == 0f) {
            float num = ATTACKTIME;
            if (NPC.ai[1] <= num * 0.7f) {
                if (++NPC.frameCounter > 3 && NPC.localAI[0] > 0) {
                    NPC.frameCounter = 0.0;
                    NPC.localAI[0]--;
                }
            }
            else if (++NPC.frameCounter > 6 && NPC.localAI[0] < 3) {
                NPC.frameCounter = 0.0;
                NPC.localAI[0]++;
            }
            NPC.frame.Y = frameHeight * ((int)NPC.localAI[0] + (_spearAttack ? 4 : 0));
        }
        else if (NPC.ai[0] >= 1f) {
            NPC.frame.Y = frameHeight * (_spearAttack ? 7 : 3);
        }

        NPC.spriteDirection = NPC.direction;
    }

    public override void AI() {
        NPC.noTileCollide = NPC.noGravity = true;

        bool flag = true;
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
        Vector2 toHead = Main.player[NPC.target].Center - Vector2.UnitY * 18f - NPC.Center;
        float attackCd = ATTACKTIME;
        if (NPC.ai[0] > 1f) {
            NPC.knockBackResist = 0f;
            if (NPC.velocity.X > 0.5f) {
                NPC.spriteDirection = -1;
            }
            if (NPC.velocity.X < -0.5f) {
                NPC.spriteDirection = 1;
            }
            NPC.ai[1]++;
            float num = 60f;
            Vector2 desiredVelocity = new(NPC.ai[2], NPC.ai[3]);

            if (!_spearAttack) {
                bool flag2 = NPC.ai[1] <= num * 0.75f;
                if (flag2) {
                    directedRotation(desiredVelocity);
                }
                //NormalMovement(false);
                float value2 = Math.Abs(NPC.ai[2] * NPC.ai[3]);
                _extraVelocity *= 0.97f;
                NPC.ai[0] = MathHelper.Lerp(NPC.ai[0] - 2f, (float)Math.Sqrt(value2), 0.005f) + 2f;
                NPC.ai[2] *= 0.97f;
                NPC.ai[3] *= 0.97f;
                NPC.velocity = Vector2.SmoothStep(NPC.velocity, desiredVelocity, (NPC.ai[0] - 2f) * 0.5f);
                NPC.velocity += (Utils.MoveTowards(NPC.Center, Main.player[NPC.target].Center, NPC.ai[0]) - NPC.Center) * MathHelper.Clamp(NPC.ai[0] - 2f, 0f, 0.2f) * 0.25f;
                //NPC.velocity *= 0.95f;
            }
            else {
                Vector2 center = new(NPC.position.X + 22 / 2, NPC.position.Y + 28 / 2 + NPC.gfxOffY);
                center.X -= 11;
                center.Y -= 9;

                diff = Main.player[NPC.target].Center + Vector2.UnitY * 20f - center;
                directedRotation(diff);
                diff.Normalize();

                if (NPC.ai[1] <= num * 0.75f) {
                    NPC.ai[2] += 0.1f;
                }
                else {
                    NPC.ai[2] -= 0.1f;
                }
                //NPC.Center = _tempPosition + NPC.velocity;
                ////NPC.velocity = Utils.MoveTowards(NPC.Center, Main.player[NPC.target].Center, NPC.ai[2]) - NPC.Center;

                NormalMovement();
                ApplyExtraVelocity1();

                int width = 60; int height = width;
                NPC.Size = new Vector2(width, height);

                if (NPC.velocity.Length() < 8f) {
                    float progress = NPC.ai[1] / num;
                    float num2 = 0.4f;
                    if (Vector2.Distance(NPC.position, Main.player[NPC.target].position) <= 50f && NPC.localAI[3] != 1f) {
                        NPC.localAI[3] = 1f;
                        NPC.velocity *= 0.5f;
                    }

                    bool flag3 = NPC.localAI[3] == 1f;
                    float speed = 1.4f;
                    if (!flag3) {
                        _extraVelocity2 = diff * speed * progress;
                    }
                    else {
                        if (NPC.localAI[1] < num * 0.2f) {
                            NPC.localAI[1]++;
                            _extraVelocity2 = -diff * speed * 0.4f * (progress - num2);
                        }
                    }
                    NPC.velocity += _extraVelocity2 * 0.75f;
                    _extraVelocity2 = Vector2.Zero;
                }
            }

            if (NPC.ai[1] >= num || NPC.justHit) {
                NPC.ai[0] = 0f;
                NPC.ai[1] = NPC.justHit ? (60f - NPC.ai[1]) : 0f;
                NPC.localAI[1] = 0f;
                NPC.localAI[2] = 0f;
                NPC.localAI[3] = 0f;
            }
        }
        else if (NPC.ai[0] == 1f && NPC.ai[1] >= attackCd) {
            _extraVelocity = _extraVelocity2 = Vector2.Zero;
            flag = false;
            NPC.ai[1] = 0f;
            NPC.ai[0] = 2f;
            if (!_spearAttack) {
                Vector2 desiredVelocity = diff * 10f;
                NPC.ai[2] = desiredVelocity.X;
                NPC.ai[3] = desiredVelocity.Y;
            }
            else {
                NPC.ai[2] = 0f;
            }
            NPC.dontTakeDamage = false;
            NPC.netUpdate = true;
        }
        else {
            int width = 22; int height = 28;
            NPC.Size = new Vector2(width, height);

            NPC.TargetClosest();
            NPC.knockBackResist = 0.9f;

            bool flag2 = NPC.ai[1] <= attackCd * 0.7f;
            NPC.dontTakeDamage = NPC.ai[1] <= attackCd * 0.7f;
            if (NPC.ai[1] < attackCd) {
                if (NPC.Distance(Main.player[NPC.target].Center) <= 400f) {
                    NPC.ai[1]++;
                }

                NormalMovement();

                _tempPosition = NPC.Center;
            }

            ApplyExtraVelocity1();

            diff = !_spearAttack ? toHead : (Main.player[NPC.target].Center - NPC.Center);
            directedRotation(diff);

            if (flag2) {
                _extraVelocity *= 0.97f;
                NPC.velocity *= 0.97f;
            }
            else if (NPC.localAI[2] == 0f) {
                if (Main.rand.NextBool()) {
                    _spearAttack = !_spearAttack;
                }
                if (_spearAttack) {
                    _tempPosition = NPC.Center;
                    //_extraVelocity = Vector2.Zero;
                }
                NPC.localAI[2] = 1f;
            }

            if (NPC.ai[1] >= attackCd) {
                NPC.ai[0] = 1f;
                NPC.localAI[2] = 0f;
            }
        }

        if (flag) {
            NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.X * 0.1f, 0.1f);
        }
    }

    private void ApplyExtraVelocity1() => NPC.velocity += _extraVelocity * 0.075f;

    private void NormalMovement(bool applyExtraVelocity = true) {
        Vector2 playerCenter = Main.player[NPC.target].Center;
        float dist = NPC.Distance(playerCenter);
        float minDist = 150f;
        float inertia = 22f;
        float speed = 5f;
        if (dist <= minDist * 1.25f) {
            Helper.InertiaMoveTowards(ref NPC.velocity, NPC.Center, NPC.Center - (playerCenter - NPC.Center).SafeNormalize(Vector2.Zero) * minDist, inertia * 0.5f, speed * 0.5f, minDist);
        }
        if (dist > minDist) {
            Helper.InertiaMoveTowards(ref NPC.velocity, NPC.Center, playerCenter, inertia * 0.75f, speed * 0.75f, minDist);
        }
        if (applyExtraVelocity) {
            float num269 = Math.Abs(NPC.position.X + (float)(NPC.width / 2) - (Main.player[NPC.target].position.X + (float)(Main.player[NPC.target].width / 2)));
            float num270 = Main.player[NPC.target].position.Y - (float)(NPC.height / 2);
            if (num269 > 50f)
                num270 -= 100f;

            if (NPC.position.Y < num270) {
                _extraVelocity.Y += 0.03f;
                if (_extraVelocity.Y < 0f)
                    _extraVelocity.Y += 0.01f;
            }
            else {
                _extraVelocity.Y -= 0.03f;
                if (_extraVelocity.Y > 0f)
                    _extraVelocity.Y -= 0.01f;
            }

            if (_extraVelocity.Y < -1.35f)
                _extraVelocity.Y = -1.35f;

            if (_extraVelocity.Y > 1.35f)
                _extraVelocity.Y = 1.35f;
        }
    }
}
