using Microsoft.Xna.Framework;

using RoA.Common.Sets;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class SolidifyingSap : ModBuff {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Solidifying Sap");
		//Description.SetDefault("Restrains movement");
		Main.buffNoSave[Type] = true;
		Main.debuff[Type] = true;
    }

	public override void Update(Player player, ref int buffIndex) {
		player.velocity.X *= 0.7f;
		player.GetModPlayer<SolidifyingSapPlayer>().solidifyingSap = true;
	}

	public override void Update(NPC npc, ref int buffIndex) {
		npc.GetGlobalNPC<SolidifyingSapNPC>().solidifyingSap = true;
		float value = 0.7f * (1f - MathHelper.Clamp(npc.knockBackResist, 0f, 1f));
		npc.velocity *= value;
	}
}

sealed class SolidifyingSapPlayer : ModPlayer {
	public bool solidifyingSap;

	public override void ResetEffects()
		=> solidifyingSap = false;

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
        if (drawInfo.drawPlayer.dead || !drawInfo.drawPlayer.active || drawInfo.shadow != 0f) {
            return;
        }

        if (solidifyingSap) {
			if (Main.rand.Next(4) == 0 && drawInfo.shadow == 0.0) {
				int _dust = Dust.NewDust(drawInfo.Position - new Vector2(2f, 2f), Player.width, Player.height, DustID.AmberBolt, Player.velocity.X * 0.4f, 3f, 100, default, 0.8f);
				Main.dust[_dust].noGravity = false;
				Main.dust[_dust].velocity *= 1.1f;
				Main.dust[_dust].velocity.Y = Main.dust[_dust].velocity.Y * -0.5f;
			}
			r *= 1f;
			g *= 0.5f;
			b *= 0f;
		}
	}
}

sealed class SolidifyingSapNPC : GlobalNPC {
	public override bool InstancePerEntity => true;

	public bool solidifyingSap;

	public override void ResetEffects(NPC npc)
		=> solidifyingSap = false;

	public override void DrawEffects(NPC npc, ref Color drawColor) {
		if (!npc.active) {
			return;
		}
		if (solidifyingSap) {
            drawColor = Color.Lerp(drawColor, Color.LightGoldenrodYellow, 0.25f);
			if (Main.rand.NextBool(1, 3)) {
				for (int k = 0; k < 2; k++) {
					if (Main.rand.NextBool(1, 2)) {
						var _dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.AmberBolt, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 0.8f);
						Main.dust[_dust].noGravity = true;
						Main.dust[_dust].noLight = true;
						Main.dust[_dust].velocity.Y += 2f;
					}
				}
			}
		}
	}
}