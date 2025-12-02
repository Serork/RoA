using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Gores;
using RoA.Content.Projectiles.Enemies;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class EntLegs : RoANPC {
    public enum States {
        Walking,
        Shielding,
        Attacking
    }

    private const short WALK = (short)States.Walking;
    private const short SHIELD = (short)States.Shielding;
    private const short ATTACK = (short)States.Attacking;

    private float _attackTimer;
    private bool _shouldAggro;


    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 18;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Hide = true
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifier);
    }

    public override void SetDefaults() {
        NPC.lifeMax = 500;
        NPC.damage = 35;
        NPC.defense = 6;
        NPC.knockBackResist = 0f;

        int width = 28; int height = 48;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.value = Item.buyPrice(0, 0, 15, 0);

        NPC.HitSound = new SoundStyle(ResourceManager.NPCSounds + "EntHit3") { Pitch = 0f, Volume = 0.75f };
        NPC.DeathSound = SoundID.NPCDeath27;

        NPC.dontTakeDamage = true;
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

    public override bool? CanFallThroughPlatforms() => true;

    public override bool PreAI() {
        if (NPC.oldVelocity.Y >= 1f) NPC.localAI[2]++;
        if (NPC.IsGrounded()) {
            if (NPC.localAI[2] > 10) Stomp(true);
            NPC.localAI[2] = 0;
        }

        return base.PreAI();
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_attackTimer);
        writer.Write(_shouldAggro);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _attackTimer = reader.ReadSingle();
        _shouldAggro = reader.ReadBoolean();
    }

    public override void AI() {
        if (NPC.localAI[3] == 0f) {
            NPC.localAI[3] = 1f;

            if (Main.netMode != NetmodeID.MultiplayerClient) {
                int npcSlot = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, ModContent.NPCType<Ent>(), ai0: NPC.whoAmI);
                NPC npc = Main.npc[npcSlot];
                npc.ai[3] = NPC.ai[3];
                npc.life = npc.lifeMax = NPC.lifeMax;
                npc.SpawnedFromStatue = NPC.SpawnedFromStatue;

                NPC.realLife = npc.whoAmI;

                NPC.ai[3] = npcSlot;

                npc.netUpdate = true;

                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcSlot);
            }

            NPC.netUpdate = true;

            return;
        }

        short state = (short)State;
        if (Main.netMode != 1) {
            NPC me = NPC.realLife != 1 ? Main.npc[NPC.realLife] : null;
            if (me != null && me.life < (int)(me.lifeMax * 0.8f)) {
                _shouldAggro = true;
                NPC.netUpdate = true;
            }
        }
        foreach (NPC checkNPC in Main.ActiveNPCs) {
            if (checkNPC.type == ModContent.NPCType<Archdruid>() && checkNPC.life < (int)(checkNPC.lifeMax * 0.8f)) {
                _shouldAggro = true;
            }
        }
        switch (state) {
            case WALK:
                NPC.ApplyFighterAI(true, targetPlayer2: _shouldAggro, movementX: (npc) => {
                    float num87 = 1f * 0.8f;
                    float num88 = 0.07f * 0.8f;
                    //num87 += (1f - (float)life / (float)lifeMax) * 1.5f;
                    //num88 += (1f - (float)life / (float)lifeMax) * 0.15f;
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
                });
                //NPC.PseudoGolemAI(0.3f);

                //if (_attackTimer < 0f && NPC.velocity.Y < 0f) {
                //    NPC.velocity.Y = 0f;
                //}
                if (NPC.HasValidTarget) {
                    if (!Main.player[NPC.target].dead && ++_attackTimer >= 300f && NPC.IsGrounded() && Vector2.Distance(Main.player[NPC.target].position, NPC.position) < 500.0) {
                        if (Collision.CanHit(NPC, Main.player[NPC.target])) {
                            _attackTimer = 0f;
                            ChangeState(SHIELD, keepState: false);
                            SoundEngine.PlaySound(new SoundStyle(ResourceManager.ItemSounds + "Leaves1") { Volume = 0.7f, Pitch = -0.5f }, NPC.position);
                            SoundEngine.PlaySound(SoundID.Item46 with { Volume = 0.5f, Pitch = -0.75f }, NPC.position);
                            NPC.defense += 500;
                        }
                    }
                }
                break;
            case SHIELD:
                if (Math.Abs(NPC.velocity.Y) <= NPC.gravity) {
                    if (NPC.IsGrounded()) {
                        NPC.velocity.X *= 0.8f;
                    }

                    NPC.ResetAIStyle();
                    if (++_attackTimer >= 180f) {
                        bool flag = Collision.CanHit(NPC.Center, 4, 4, Main.player[NPC.target].Center, 4, 4);

                        _attackTimer = 0f;
                        ChangeState(flag ? ATTACK : WALK);

                        NPC.defense -= flag ? 506 : 500;
                    }
                }
                break;
            case ATTACK:
                NPC.ResetAIStyle();

                if (_attackTimer >= 20f && _attackTimer % 10f == 0f) {
                    SoundEngine.PlaySound(SoundID.Item104 with { Volume = 0.7f, Pitch = _attackTimer * 0.01f, PitchVariance = 0.1f }, NPC.position);

                    float dustCount = 14f;
                    int num1 = 0;
                    Vector2 center = NPC.Center - Vector2.UnitY * 32f + Vector2.UnitY * 4f;
                    while (num1 < dustCount) {
                        if (!Main.rand.NextBool(5)) {
                            Vector2 vector = Vector2.UnitX * 0f;
                            vector += -Vector2.UnitY.RotatedBy(num1 * (7f / dustCount)) * new Vector2(3f, 3f);
                            vector = vector.RotatedBy(NPC.velocity.ToRotation());
                            int num3 = Dust.NewDust(center, 0, 0, DustID.MagicMirror, 0f, 0f, 40, default, 1f);
                            Main.dust[num3].noGravity = true;
                            Main.dust[num3].position = new Vector2(center.X + 10f * NPC.direction, center.Y + 10) + vector;
                            Main.dust[num3].velocity = NPC.velocity * 0f + vector.SafeNormalize(Vector2.UnitY) * 0.8f;
                            Main.dust[num3].scale *= Main.rand.NextFloat(1f, 1.75f);
                        }
                        int num4 = num1;
                        num1 = num4 + 1;
                    }

                    if (Main.netMode != NetmodeID.MultiplayerClient) {
                        Vector2 spreadOld = new Vector2(Main.player[NPC.target].position.X - center.X, Main.player[NPC.target].position.Y - center.Y).RotatedByRandom(MathHelper.ToRadians(30));
                        Vector2 spread = Vector2.Normalize(spreadOld);
                        int projectile = Projectile.NewProjectile(NPC.GetSource_FromAI(), center.X + 10f * NPC.direction, center.Y + 10f, spread.X * 4f, spread.Y * 4f, ModContent.ProjectileType<PrimordialLeaf>(),
                            35 / 2, 1f, Main.myPlayer);
                        //NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile);
                    }
                }
                if (++_attackTimer >= 100f) {
                    _attackTimer = -10f;
                    ChangeState(WALK);

                    NPC.defense += 6;
                }
                break;
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = NPC.direction;
        short state = (short)State;
        switch (state) {
            case WALK:
                if (Math.Abs(NPC.velocity.X) < 0.1f) {
                    CurrentFrame = 0;
                }
                else {
                    if (++NPC.frameCounter >= 6.0) {
                        NPC.frameCounter = 0.0;
                        CurrentFrame++;
                        if (NPC.IsGrounded() && (CurrentFrame == 4 || CurrentFrame == 9)) {
                            Stomp();
                        }
                        if (CurrentFrame >= 13 || CurrentFrame < 3) {
                            CurrentFrame = 3;
                        }
                    }
                }
                break;
            case SHIELD:
                CurrentFrame = 2;
                break;
            case ATTACK:
                if (++NPC.frameCounter >= 4.0) {
                    NPC.frameCounter = 0.0;
                    CurrentFrame++;
                    if (CurrentFrame >= 16 || CurrentFrame < 13) {
                        CurrentFrame = 13;
                    }
                }
                break;
        }
        if (NPC.velocity.Y > 0f || NPC.velocity.Y <= -0.25f) {
            CurrentFrame = 1;
        }
        int currentFrame = Math.Min((int)CurrentFrame, Main.npcFrameCount[Type] - 1);
        ChangeFrame((currentFrame, frameHeight));
    }

    private void Stomp(bool empowered = false) {
        string tag = "Ent Stomp";
        PunchCameraModifier punchCameraModifier = new(NPC.Bottom, MathHelper.PiOver2.ToRotationVector2(), 2f + (empowered ? Math.Abs(NPC.oldVelocity.Y) : 0f), 5f + (empowered ? Math.Abs(NPC.oldVelocity.Y) : 0f), 20, 2000f, tag);
        Main.instance.CameraModifiers.Add(punchCameraModifier);
        SoundEngine.PlaySound(SoundID.Item73 with { Volume = empowered ? 0.6f : 0.3f, PitchVariance = 0.1f, Pitch = empowered ? -0.5f : -0.2f }, NPC.Bottom);
        if (!empowered) {
            float num2 = 10f;
            if (Main.netMode != NetmodeID.Server) {
                for (int i = 0; i < Main.rand.Next(1, 4); i++) {
                    Gore.NewGore(null, NPC.Top - Vector2.UnitY * 20f + Main.rand.RandomPointInArea(40f), Utils.RandomVector2(Main.rand, 0f - num2, num2), ModContent.GoreType<BackwoodsLeaf>(), 0.7f + Main.rand.NextFloat() * 0.6f);
                }
            }
        }
    }
}