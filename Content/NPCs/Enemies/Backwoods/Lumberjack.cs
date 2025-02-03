using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class Lumberjack : RoANPC {
	private const float MAXSPEED = 1.65f;

    private enum States {
        Spawned,
        Walking,
        Attacking
    }

    public override void SetStaticDefaults() {
		Main.npcFrameCount[Type] = 21;
    }

	public override void SetDefaults() {
		NPC.lifeMax = 200;
		NPC.damage = 44;
		NPC.defense = 8;
		NPC.knockBackResist = 0.1f;

		int width = 28; int height = 44;
		NPC.Size = new Vector2(width, height);

		NPC.aiStyle = -1;

		NPC.npcSlots = 1.25f;
		NPC.value = Item.buyPrice(0, 0, 25, 5);

		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<LumberjackBanner>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Lumberjack")
        ]);
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if (Main.netMode == NetmodeID.Server) {
            return;
        }

        if (NPC.life > 0) {
            for (int num828 = 0; (double)num828 < hit.Damage / (double)NPC.lifeMax * 100.0; num828++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num829 = 0; num829 < 50; num829++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2.5f * (float)hit.HitDirection, -2.5f);
        }

        Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "LumberHead".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "LumberAxe".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "LumberArm".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "LumberLeg".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "LumberLeg".GetGoreType(), Scale: NPC.scale);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }

    public override void OnKill() {
		if (Main.netMode == NetmodeID.MultiplayerClient) {
			return;
		}

		int npc = NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.PantlessSkeleton);
		if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
			NetMessage.SendData(MessageID.SyncNPC, number: npc);
		}
	}

	public override void FindFrame(int frameHeight) {
        double walkingCounter = 4.0;
        int currentFrame = Math.Min((int)CurrentFrame, Main.npcFrameCount[Type] - 1);
        if (NPC.IsABestiaryIconDummy) {
            NPC.frameCounter += 0.75f;
            if (NPC.frameCounter > walkingCounter) {
                int lastWalkingFrame = 7;
                if (++CurrentFrame >= lastWalkingFrame) {
                    CurrentFrame = 0;
                }
                NPC.frameCounter = 0.0;
                ChangeFrame((currentFrame, frameHeight));
            }

			return;
        }
        
		NPC.spriteDirection = NPC.direction;
		double attackCounter = walkingCounter + 50.0;
		switch (State) {
			case (float)States.Walking:
				bool isDead = Main.player[NPC.target].dead;
				if (NPC.velocity.Y > 0f || NPC.velocity.Y <= -0.25f) {
					CurrentFrame = 3;
				}
				else if (NPC.velocity.X == 0f) {
					CurrentFrame = 0;
				}
				else {
					NPC.frameCounter += 1f * Math.Abs(NPC.velocity.X / MAXSPEED);
                    if (NPC.frameCounter > walkingCounter) {
						int lastWalkingFrame = 7;
						if (++CurrentFrame >= lastWalkingFrame + (isDead ? 6 : 0)) {
							CurrentFrame = isDead ? 7 : 0;
						}
						NPC.frameCounter = 0.0;
					}
				}
				break;
			case (float)States.Attacking:
				NPC.frameCounter = 0.0;
				double progress = (double)Helper.EaseInOut3(StateTimer);
				CurrentFrame = 13 + (int)(8.0 * progress);
				break;
		}
		ChangeFrame((currentFrame, frameHeight));
	}

	public override void AI() {
		Player player;
		float closeRange = 65f;
		switch (State) {
			case (float)States.Spawned:
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
				if (NPC.position.ToTileCoordinates().Y < Main.worldSurface) {
					if (NPC.Distance(player.Center) > closeRange && ((NPC.position.X > player.position.X && player.direction == 1) || (NPC.position.X < player.position.X && player.direction == -1))) {
						if (Main.netMode != NetmodeID.MultiplayerClient) {
							NPC.KillNPC();
							return;
						}
					}
				}
                StateTimer = 0.2f;
                ChangeState((int)States.Walking);
				break;
			case (float)States.Walking:
                //NPC.TargetClosest();
                if (Attack) {
					Attack = false;
					NPC.netUpdate = true;
				}
				if (StateTimer > 0f) {
					StateTimer -= TimeSystem.LogicDeltaTime;
					if (NPC.velocity.Y < 0f) {
						NPC.velocity.Y = 0f;
					}
				}
                NPC.aiStyle = 3;
				AIType = 580;
                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
                if (Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) <= 20f && Math.Abs(NPC.velocity.Y) <= NPC.gravity) {
                    NPC.velocity.X *= 0.99f;
                    NPC.velocity.X += (float)NPC.direction * 0.025f;
                }
                if (NPC.velocity.Y <= -6f) {
					NPC.velocity.Y = -6f;
				}
				NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MAXSPEED, MAXSPEED);
				player = Main.player[NPC.target];
				if (NPC.Distance(player.Center) < closeRange && !player.dead) {
                    StateTimer = 0.2f;
                    ChangeState((int)States.Attacking);
				}
				break;
			case (float)States.Attacking:
				player = Main.player[NPC.target];
				bool inRange = NPC.Distance(player.Center) >= closeRange;
				if ((inRange || player.dead) && StateTimer <= 0.2f) {
					StateTimer = 0.1f;
                    ChangeState((int)States.Walking);
					return;
				}
				if (NPC.velocity.Y == 0f) {
                    NPC.ResetAIStyle();
                    NPC.velocity.X *= 0.8f;
                    StateTimer += TimeSystem.LogicDeltaTime / 2f;
					StateTimer *= 1.05f;
					if (StateTimer >= 0.6f) {
						if (!Attack) {
							Attack = true;
							SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
							if (Main.netMode != NetmodeID.MultiplayerClient) {
								Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(NPC.width / 2 * NPC.direction + 10, 0f), Vector2.Zero, ModContent.ProjectileType<LumberjackAxeSlash>(), NPC.damage, 3f, Main.myPlayer);
							}
							NPC.netUpdate = true;
						}
						if (StateTimer >= 1f) {
							StateTimer = 0f;
							Attack = false;
							NPC.netUpdate = true;
						}
					}
				}
				break;
		}
	}

	private class LumberjackAxeSlash : ModProjectile {
		public override string Texture => ResourceManager.EmptyTexture;

		public override bool PreDraw(ref Color lightColor) => false;

        public override void SetDefaults() {
			int width = 120; int height = 60;
			Projectile.Size = new Vector2(width, height);

			Projectile.friendly = false;
			Projectile.hostile = true;

			Projectile.aiStyle = -1;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 10;

			Projectile.tileCollide = false;

			Projectile.alpha = byte.MaxValue;
		}
	}
}