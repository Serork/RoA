using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Hemorrhage : ModBuff {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Hemorrhage");
		//Description.SetDefault("Losing life");
		Main.buffNoSave[Type] = true;
		Main.debuff[Type] = true;
		Main.pvpBuff[Type] = true;
	}

	public override void Update(Player player, ref int buffIndex)  => player.GetModPlayer<HemorrhagePlayer>().hemorrhageEffect = true;
	
	public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<HemorrhageNPC>().hemorrhageEffect = true;
}

sealed class HemorrhagePlayer : ModPlayer {
	public bool hemorrhageEffect;

	public override void ResetEffects() => hemorrhageEffect = false;


	public override void UpdateBadLifeRegen() {
		if (hemorrhageEffect) {
			if (Player.lifeRegen > 0) Player.lifeRegen = 0;
			Player.lifeRegenTime = 0;
			Player.lifeRegen -= 16;
		}
	}

	public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
		if (hemorrhageEffect) {
			if (Main.rand.Next(4) == 0 && drawInfo.shadow == 0.0) {
				int dust = Dust.NewDust(drawInfo.Position - new Vector2(2f, 2f), Player.width + 4, Player.height + 4, 5, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 0, new Color(85, 0, 15), 1.1f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 1.8f;
				Main.dust[dust].velocity.Y = 0.5f;
			}
			r *= 85 / 255;
			g *= 0;
			b *= 15 / 255f;
		}
	}
}

sealed class HemorrhageNPC : GlobalNPC {
	public override bool InstancePerEntity => true;

	public bool hemorrhageEffect;

	public override void ResetEffects(NPC npc) => hemorrhageEffect = false;


	public override void UpdateLifeRegen(NPC npc, ref int damage) {
		if (hemorrhageEffect) {
			if (npc.lifeRegen > 0) npc.lifeRegen = 0;
			npc.lifeRegen -= 16;
			if (damage < 4) damage = 4;
		}
	}

	public override void DrawEffects(NPC npc, ref Color drawColor) {
		if (hemorrhageEffect) {
			if (Main.rand.Next(4) == 0) {
				int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, 5, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 0, new Color(85, 0, 15), 1.1f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 1.8f;
				Main.dust[dust].velocity.Y = 0.5f;
			}
			Lighting.AddLight(npc.position, 85 / 255f, 0f, 15 / 255f);
		}
	}
}