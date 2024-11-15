using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Deceleration : ModBuff {
	public override void SetStaticDefaults() {
		// DisplayName.SetDefault("Deceleration");
		// Description.SetDefault("The power of Aqua has corrupted your moving abilities and slowed down your speed");

		Main.debuff[Type] = true;
		Main.pvpBuff[Type] = true;
		Main.buffNoSave[Type] = true;
	}

	public override void Update(Player player, ref int buffIndex) {
		player.GetModPlayer<DecelerationPlayer>().deceleration = true;
		player.velocity = Vector2.Clamp(player.velocity, -Vector2.One * 0.25f, Vector2.One * 0.25f);
	}

	public override void Update(NPC npc, ref int buffIndex) {
		npc.GetGlobalNPC<DecelerationNPC>().deceleration = true;
		if (npc.boss) {
			return;
		}
		npc.velocity = Vector2.Clamp(npc.velocity, -Vector2.One * 0.25f, Vector2.One * 0.25f);
	}
}

sealed class DecelerationPlayer : ModPlayer {
	public bool deceleration;

	public override void ResetEffects() {
		deceleration = false;
	}

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
		if (deceleration) {
            r *= 0.25f;
            g *= 0.6f;
            b *= 1f;
            for (int k = 0; k < 2; k++) {
				if (drawInfo.shadow == 0f && Main.rand.NextBool(1, 3)) {
					int dust = Dust.NewDust(drawInfo.Position, Player.width, Player.height, DustID.DungeonWater, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 1.5f + 0.75f * Main.rand.NextFloat());
					Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity.Y += 1f + 1f * Main.rand.NextFloat();
                    Main.dust[dust].velocity *= 0.25f;
                }
			}
		}
	}
}

sealed class DecelerationNPC : GlobalNPC {
	public override bool InstancePerEntity => true;

	public bool deceleration;

	public override void ResetEffects(NPC npc) {
		deceleration = false;
	}

	public override void DrawEffects(NPC npc, ref Color drawColor) {
		if (deceleration) {
            drawColor = Color.Lerp(drawColor, Color.Blue, 0.25f);
			if (Main.rand.NextBool(1, 3)) {
				for (int k = 0; k < 2; k++)
					if (Main.rand.NextBool(1, 3)) {
						int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.DungeonWater, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 1.5f + 0.75f * Main.rand.NextFloat());
						Main.dust[dust].noGravity = true;
						Main.dust[dust].velocity.Y += 1f + 1f * Main.rand.NextFloat();
						Main.dust[dust].velocity *= 0.25f;
					}
			}
		}
	}
}