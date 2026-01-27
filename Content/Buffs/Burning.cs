using Microsoft.Xna.Framework;

using RoA.Common.Sets;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Burning : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Hemorrhage");
        //Description.SetDefault("Losing life");
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;

        BuffID.Sets.LongerExpertDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<BurningPlayer>().burningEffect = true;

    public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<BurningNPC>().burningEffect = true;
}

sealed class BurningPlayer : ModPlayer {
    public bool burningEffect;

    public override void ResetEffects() => burningEffect = false;

    public override void UpdateBadLifeRegen() {
        if (Player.dead) {
            return;
        }

        if (burningEffect) {
            if (Player.lifeRegen > 0)
                Player.lifeRegen = 0;

            Player.lifeRegenTime = 0f;
            Player.lifeRegen -= 30;
        }
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
        if (drawInfo.drawPlayer.dead || !drawInfo.drawPlayer.active || drawInfo.shadow != 0f) {
            return;
        }
        if (!burningEffect) {
            return;
        }

        if (Main.rand.NextBool() && drawInfo.shadow == 0f) {
            Player player = drawInfo.drawPlayer;
            Dust dust14 = Dust.NewDustDirect(new Vector2(player.position.X - 2f, player.position.Y - 2f), player.width + 4, player.height + 4, 6, player.velocity.X * 0.4f, player.velocity.Y * 0.4f, 100, default(Color), 2f);
            dust14.noGravity = true;
            dust14.velocity *= 1.8f;
            dust14.velocity.Y -= 0.75f;
            drawInfo.DustCache.Add(dust14.dustIndex);
        }

        r = 1f;
        g *= 0.7f;
        b *= 0.6f;
    }
}

sealed class BurningNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public bool burningEffect;

    public override void ResetEffects(NPC npc) => burningEffect = false;

    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        if (burningEffect) {
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            if (damage < 8) damage = 8;
            npc.lifeRegen -= 30;
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (!npc.active) {
            return;
        }
        if (!burningEffect) {
            return;
        }

        if (Main.rand.NextBool()) {
            Dust dust14 = Dust.NewDustDirect(new Vector2(npc.position.X - 2f, npc.position.Y - 2f), npc.width + 4, npc.height + 4, 6, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default(Color), 2f);
            dust14.noGravity = true;
            dust14.velocity *= 1.8f;
            dust14.velocity.Y -= 0.75f;
        }

        drawColor = Color.Lerp(drawColor, Color.DarkOrange.MultiplyRGB(drawColor), 0.25f);
    }
}
