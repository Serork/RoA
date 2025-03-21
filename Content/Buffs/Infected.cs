using Microsoft.Xna.Framework;

using RoA.Common.Sets;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Infected : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Infected");
        //Description.SetDefault("Losing life");
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;

        BuffSets.Debuffs[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<InfectedPlayer>().infectedEffect = true;

    public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<InfectedNPC>().infectedEffect = true;
}

sealed class InfectedPlayer : ModPlayer {
    public bool infectedEffect;

    public override void ResetEffects() => infectedEffect = false;

    public override void UpdateBadLifeRegen() {
        if (Player.dead) {
            return;
        }

        if (infectedEffect) {
            if (Player.lifeRegen > 0) Player.lifeRegen = 0;
            Player.lifeRegenTime = 0;
            Player.lifeRegen -= 8;
        }
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
        if (drawInfo.drawPlayer.dead || !drawInfo.drawPlayer.active || drawInfo.shadow != 0f) {
            return;
        }

        if (infectedEffect) {
            if (Main.rand.Next(4) == 0 && drawInfo.shadow == 0.0) {
                int dust = Dust.NewDust(drawInfo.Position - new Vector2(2f, 2f), Player.width + 4, Player.height + 4, DustID.ScourgeOfTheCorruptor, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 0, default, 1.1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.8f;
                Main.dust[dust].velocity.Y = 0.5f;
            }
        }
    }
}

sealed class InfectedNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public bool infectedEffect;

    public override void ResetEffects(NPC npc) => infectedEffect = false;

    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        if (infectedEffect) {
            if (npc.lifeRegen > 0) npc.lifeRegen = 0;
            npc.lifeRegen -= 16;
            if (damage < 4) damage = 4;
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (!npc.active) {
            return;
        }
        if (infectedEffect) {
            if (Main.rand.Next(4) == 0) {
                int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, DustID.ScourgeOfTheCorruptor, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 0, default, 1.1f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.8f;
                Main.dust[dust].velocity.Y = 0.5f;
            }
        }

        //      if (infectedEffect) {
        //	if (Main.rand.Next(4) == 0) {
        //		int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, DustID.ScourgeOfTheCorruptor, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 0, default, 1.1f);
        //		Main.dust[dust].noGravity = true;
        //		Main.dust[dust].velocity *= 1.8f;
        //		Main.dust[dust].velocity.Y = 0.5f;
        //	}
        //	//Lighting.AddLight(npc.position, 0.3f, 0.3f, 0.01f);
        //}
    }
}