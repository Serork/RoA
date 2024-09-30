using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class SummonedRaven : ModNPC {
	public enum States {
		Spawn,
		Attacking
	}

	private const short SPAWN = (short)States.Spawn;
	private const short ATTACKING = (short)States.Attacking;

	private ref float State => ref NPC.ai[2];
    private ref float Acceleration => ref NPC.ai[3];

	public override void SetStaticDefaults() {
		//base.SetStaticDefaults();

		// DisplayName.SetDefault("Summoned Raven");
		Main.npcFrameCount[Type] = 5;

		//NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
		//	CustomTexturePath = nameof(RiseofAges) + "/Assets/Textures/Bestiary/SummonedRaven",
		//	Position = new Vector2(0f, -16f),
		//	Velocity = 0f,
		//	Frame = 1
		//};
		//NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
	}

	public override void SetDefaults() {
		NPC.CloneDefaults(NPCID.Raven);

		NPC.aiStyle = -1;

		NPC.alpha = 255;

        NPC.noTileCollide = false;

        //SpawnModBiomes = new int[] {
        //	ModContent.GetInstance<BackwoodsBiome>().Type
        //};
    }

	public override bool? CanFallThroughPlatforms() => true;

    public override void AI() {
		if (NPC.NearestTheSame(out NPC npc)) {
            NPC.OffsetNPC(npc, 0.25f);
		}

		if (Acceleration < 1.5f) {
			Acceleration += 0.0615f;
			Acceleration *= 1.05f;
		}

		short state = (short)State;
		if (state == SPAWN) {
			NPC.noGravity = true;

			if (NPC.ai[0] == 0f && NPC.ai[1] == 0f) {
				NPC.alpha = 0;

				State = ATTACKING;
			}

			NPC.velocity = new Vector2(NPC.ai[0], NPC.ai[1]) * Acceleration;

			NPC.rotation = NPC.velocity.ToRotation() + MathHelper.ToRadians(90f);

			if (Main.netMode != NetmodeID.Server && Main.rand.NextBool()) {
				int dust = Dust.NewDust(NPC.position, 16, 16, 108, NPC.velocity.X * 0.01f, NPC.velocity.Y * 0.01f, NPC.alpha, new Color(30, 30, 55), Main.rand.NextFloat(1.1f, 1.3f));
				Main.dust[dust].noLight = true;
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity.X *= 0.4f;
				Main.dust[dust].velocity.Y *= 0.4f;
			}

			if (NPC.alpha > 0) {
				NPC.alpha -= 15 - Main.rand.Next(1, 8);

				if (Main.netMode != NetmodeID.Server && NPC.alpha % 20 == 0) {
					if (Main.rand.NextBool()) {
						for (int i = 0; i < 16; i++) {
							int dust = Dust.NewDust(NPC.position, 20, 20, 108, (float)Math.Cos(MathHelper.Pi / 6 * i), (float)Math.Sin(MathHelper.Pi / 6 * i), 140, new Color(30, 30, 55), 1.2f);
							Main.dust[dust].noGravity = true;
						}
					}
				}
				return;
			}
			State = ATTACKING;
			NPC.netUpdate = true;
			return;
		}

		NPC.alpha = 0;

		NPC.ApplyAdvancedFlierAI();
		AnimationType = 301;
	}


	public override void FindFrame(int frameHeight) {
		short state = (short)State;
		if (state != SPAWN) {
			return;
		}
		NPC.spriteDirection = NPC.direction;
		NPC.frame.Y = 0;
	}

	//public override void HitEffect(NPC.HitInfo hit) {
	//	if (Main.netMode == NetmodeID.Server) {
	//		return;
	//	}
	//	if (NPC.life <= 0) {
	//		Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), ModContent.Find<ModGore>(nameof(RiseofAges) + "/RavenGore1").Type);
	//		for (int i = 0; i < 2; i++) {
	//			Gore.NewGore(NPC.GetSource_Death(), NPC.position, new Vector2(Main.rand.Next(-6, 7), Main.rand.Next(-6, 7)), ModContent.Find<ModGore>(nameof(RiseofAges) + "/RavenGore2").Type);
	//		}
	//		for (int i = 0; i < 25; i++) {
	//			Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 2.5f * hitDirection, -2.5f, 0, default, 1.1f);
	//		}
	//	}
	//	int life = 0;
	//	while (life < damage / NPC.lifeMax * 15) {
	//		Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hitDirection, -1f, 0, default, 0.75f);
	//		life++;
	//	}
	//}

	public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
		Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
		spriteBatch.Draw(texture, NPC.position - screenPos + new Vector2(NPC.width, NPC.height) / 2, NPC.frame, drawColor * (1f - NPC.alpha / 255f), NPC.rotation, new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) / 2, NPC.scale, NPC.velocity.X > 0f? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
		return false;
	}
}