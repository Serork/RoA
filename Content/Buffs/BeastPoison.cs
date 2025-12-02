using Microsoft.Xna.Framework;

using RoA.Common.Sets;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class BeastPoison : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Beast's Poison");
        //Description.SetDefault("Iosing life, movement speed decreased");

        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;

        BuffSets.Debuffs[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.GetModPlayer<BeastPoisonPlayer>().beastPoison = true;
        //player.moveSpeed *= 0.25f;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        npc.GetGlobalNPC<BeastPoisonNPC>().beastPoison = true;
    }
}

sealed class BeastPoisonNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public bool beastPoison;

    public override void ResetEffects(NPC npc) => beastPoison = false;

    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        if (beastPoison) {
            if (npc.lifeRegen > 0) {
                npc.lifeRegen = 0;
            }
            npc.lifeRegen -= 10;
            if (damage < 2) {
                damage = 2;
            }
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (!npc.active) {
            return;
        }

        if (beastPoison) {
            drawColor = Color.Lerp(drawColor, Color.YellowGreen.MultiplyRGB(drawColor), 0.25f);
            if (Main.rand.Next(15) >= 10) {
                int rotDust = Dust.NewDust(npc.position - new Vector2(1f, 1f), npc.width + 1, npc.height + 1, 5, npc.velocity.X * 0.6f, npc.velocity.Y * 0.4f, 125, Color.YellowGreen, 1.1f);
                Main.dust[rotDust].noGravity = true;
            }
        }
    }

    public override void PostAI(NPC npc) {
        //if (beastPoison) {
        //    npc.velocity *= 0.8f;
        //}
    }
}

sealed class BeastPoisonPlayer : ModPlayer {
    public bool beastPoison;

    public override void ResetEffects() => beastPoison = false;

    public override void UpdateBadLifeRegen() {
        if (Player.dead) {
            return;
        }

        if (beastPoison) {
            if (Player.lifeRegen > 0) Player.lifeRegen = 0;
            Player.lifeRegenTime = 0;
            Player.lifeRegen -= 10;
        }
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
        if (drawInfo.drawPlayer.dead || !drawInfo.drawPlayer.active || drawInfo.shadow != 0f) {
            return;
        }
        if (beastPoison) {
            r *= 0.6f;
            g *= 0.9f;
            b *= 0.6f;

            if (Main.rand.Next(15) >= 10) {
                int rotDust = Dust.NewDust(Player.position - new Vector2(1f, 1f), Player.width + 1, Player.height + 1, 5, Player.velocity.X * 0.6f, Player.velocity.Y * 0.4f, 125, Color.YellowGreen, 1.1f);
                Main.dust[rotDust].noGravity = true;
                drawInfo.DustCache.Add(Main.dust[rotDust].dustIndex);
            }
        }
    }
}