using Microsoft.Xna.Framework;

using RoA.Content.Projectiles.Enemies;
using RoA.Core;
using RoA.Utilities;

using System;

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

	public override string Texture => ResourceManager.EmptyTexture;

    public override void SetStaticDefaults() {
		Main.npcFrameCount[Type] = 18;
	}

	public override void SetDefaults() {
		NPC.lifeMax = 500;
		NPC.damage = 36;
		NPC.defense = 6;
		NPC.knockBackResist = 0f;

		int width = 35; int height = 40;
		NPC.Size = new Vector2(width, height);

		NPC.aiStyle = -1;

		NPC.HitSound = SoundID.NPCHit52;
		NPC.DeathSound = SoundID.NPCDeath27;

		NPC.dontTakeDamage = true;
	}

	public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) => false;

	public override void AI() {
		if (NPC.localAI[3] == 0f) {
			NPC.localAI[3] = 1f;

			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int npc = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.position.X, (int)NPC.position.Y, ModContent.NPCType<Ent>(), ai0: NPC.whoAmI);
				Main.npc[npc].life = Main.npc[npc].lifeMax = NPC.lifeMax;
				NPC.realLife = Main.npc[npc].whoAmI;
				if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
					NetMessage.SendData(MessageID.SyncNPC, number: npc);
				}

				NPC.ai[3] = npc;
			}

			NPC.netUpdate = true;

			return;
		}

		short state = (short)State;
		switch (state) {
			case WALK:
				NPC.direction = Main.player[NPC.target].Center.DirectionFrom(NPC.Center).X.GetDirection();
                NPC.aiStyle = 3;
                AIType = 580;
                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
                if (Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) <= 20f && Math.Abs(NPC.velocity.Y) <= NPC.gravity) {
                    NPC.velocity.X *= 0.99f;
                    NPC.velocity.X += (float)NPC.direction * 0.025f;
                }
				float maxSpeed = 1.25f;
				NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);
				if (++NPC.ai[2] >= 300f) {
					NPC.ai[2] = 0f;
                    ChangeState(SHIELD, keepState: false);

					NPC.defense += 500;
				}
				break;
			case SHIELD:
				if (Math.Abs(NPC.velocity.Y) <= NPC.gravity) {
					NPC.velocity.X *= 0.8f;

					NPC.aiStyle = 0;
					AIType = -1;
					if (++NPC.ai[2] >= 180f) {
						bool flag = Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height);

                        NPC.ai[2] = 0f;
                        ChangeState(flag ? ATTACK : WALK);

						NPC.defense -= flag ? 506 : 500;
					}
				}
				break;
			case ATTACK:
				if (NPC.ai[2] >= 20f && NPC.ai[2] % 10f == 0f) {
					SoundEngine.PlaySound(SoundID.Item104, NPC.position);

					float dustCount = 14f;
					int num1 = 0;
					Vector2 center = NPC.Center - Vector2.UnitY * 32f;
                    while (num1 < dustCount) {
						Vector2 vector = Vector2.UnitX * 0f;
						vector += -Vector2.UnitY.RotatedBy(num1 * (7f / dustCount)) * new Vector2(3f, 3f);
						vector = vector.RotatedBy(NPC.velocity.ToRotation());
						int num3 = Dust.NewDust(center, 0, 0, DustID.MagicMirror, 0f, 0f, 40, default, 1f);
						Main.dust[num3].noGravity = true;
						Main.dust[num3].position = new Vector2(center.X + 20 * NPC.direction, center.Y + 8) + vector;
						Main.dust[num3].velocity = NPC.velocity * 0f + vector.SafeNormalize(Vector2.UnitY) * 0.8f;
						int num4 = num1;
						num1 = num4 + 1;
					}

					if (Main.netMode != NetmodeID.MultiplayerClient) {
						Vector2 spreadOld = new Vector2(Main.player[NPC.target].position.X - center.X, Main.player[NPC.target].position.Y - center.Y).RotatedByRandom(MathHelper.ToRadians(30));
						Vector2 spread = Vector2.Normalize(spreadOld);
						int projectile = Projectile.NewProjectile(NPC.GetSource_FromAI(), center.X + 14f * NPC.direction, center.Y + 8f, spread.X * 4f, spread.Y * 4f, ModContent.ProjectileType<PrimordialLeaf>(), NPC.damage / 2, 0.1f, Main.myPlayer);
						NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile);
					}
				}
				if (++NPC.ai[2] >= 100f) {
                    NPC.ai[2] = 0f;
                    ChangeState(WALK, keepState: false);

                    NPC.defense += 6;
				}
				break;
		}
	}

	public override void FindFrame(int frameHeight) {
		NPC.spriteDirection = -NPC.direction;
		short state = (short)State;
		switch (state) {
			case WALK:
				if (NPC.velocity.X == 0f) {
					CurrentFrame = 0;
				}
				else {
					if (++NPC.frameCounter >= 6.0) {
						NPC.frameCounter = 0.0;
						CurrentFrame++;
						if ((CurrentFrame == 4 || CurrentFrame == 9) || (NPC.oldVelocity.Y > 3f && NPC.velocity.Y == 0f)) {
							string tag = "Ent Stomp";
							PunchCameraModifier punchCameraModifier = new(NPC.Bottom, MathHelper.PiOver2.ToRotationVector2(), 4f, 5f, 20, 1000f, tag);
							Main.instance.CameraModifiers.Add(punchCameraModifier);
							SoundEngine.PlaySound(SoundID.Item73, NPC.Bottom);
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
}