using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
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

		int width = 30; int height = 68;
		NPC.Size = new Vector2(width, height);

		NPC.aiStyle = -1;

		NPC.npcSlots = 2f;
		NPC.value = Item.buyPrice(0, 0, 25, 5);

		NPC.HitSound = SoundID.NPCHit1;
		NPC.DeathSound = SoundID.NPCDeath1;
	}

	//public override void HitEffect(NPC.HitInfo hit) {
	//	if (Main.netMode == NetmodeID.Server) {
	//		return;
	//	}

	//	if (NPC.life <= 0) {
	//		string[] goresNames = new string[] { "Head", "Leg", "Arm", "Axe" };
	//		foreach (string goreName in goresNames) {
	//			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.Find<ModGore>(nameof(RiseofAges) + "/Lumber" + goreName).Type, 1f);
	//		}
	//		for (int i = 0; i < 20; i++) {
	//			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 2.5f * hitDirection, -2.5f, 0, default(Color), 0.7f);
	//		}
	//		for (int i = 0; i < 4; i++) {
	//			Dust.NewDust(NPC.Center, 0, 0, DustID.Bone, 0f, 0.5f);
	//		}
	//	}
	//	else {
	//		for (int i = 0; i < damage / NPC.lifeMax * 50.0; i++) {
	//			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hitDirection, -1f, 0, new Color(), 1f);
	//		}
	//	}
	//}

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
		NPC.spriteDirection = NPC.direction;
		double walkingCounter = 4.0;
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
		int currentFrame = Math.Min((int)CurrentFrame, Main.npcFrameCount[Type] - 1);
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
                NPC.TargetClosest();
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
				if (NPC.velocity.Y <= -6f) {
					NPC.velocity.Y = -6f;
				}
				NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MAXSPEED, MAXSPEED);
				player = Main.player[NPC.target];
				if (NPC.Distance(player.Center) < closeRange && !player.dead) {
                    StateTimer = 0.1f;
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
					NPC.aiStyle = 0;
					AIType = -1;
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