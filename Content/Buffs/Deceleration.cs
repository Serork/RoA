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
        //player.velocity *= 0.995f;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        npc.GetGlobalNPC<DecelerationNPC>().deceleration = true;
        //float value = MathHelper.Clamp(npc.knockBackResist, 0f, 1f);
        //float value2 = MathHelper.Lerp(value, 1f, (1f - value) * 0.995f);
        //npc.velocity *= value2;
    }
}

sealed class DecelerationPlayer : ModPlayer {
    public bool deceleration;

    public override void ResetEffects() {
        deceleration = false;
    }

    public override void UpdateBadLifeRegen() {
        if (Player.dead) {
            return;
        }

        if (deceleration) {
            if (Player.lifeRegen > 0) Player.lifeRegen = 0;
            Player.lifeRegenTime = 0;
            Player.lifeRegen -= 8;
        }
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
        if (drawInfo.drawPlayer.dead || !drawInfo.drawPlayer.active || drawInfo.shadow != 0f) {
            return;
        }
        if (deceleration) {
            r *= 0.8f;
            g *= 0.8f;
            b *= 1f;
            for (int k = 0; k < 2; k++) {
                if (drawInfo.shadow == 0f && Main.rand.NextBool(1, 3)) {
                    int dust = Dust.NewDust(drawInfo.Position, Player.width, Player.height, DustID.DungeonWater, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 1.5f + 0.75f * Main.rand.NextFloat());
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity.Y += 1f + 1f * Main.rand.NextFloat();
                    Main.dust[dust].velocity *= 0.25f;
                    drawInfo.DustCache.Add(Main.dust[dust].dustIndex);
                }
            }
        }
    }
}

sealed class DecelerationNPC : GlobalNPC {
    public override bool InstancePerEntity => true;

    public bool deceleration;

    public override void UpdateLifeRegen(NPC npc, ref int damage) {
        if (deceleration) {
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;

            npc.lifeRegen -= 8;
        }
    }

    public override void ResetEffects(NPC npc) {
        deceleration = false;
    }

    public override void DrawEffects(NPC npc, ref Color drawColor) {
        if (!npc.active) {
            return;
        }
        if (deceleration) {
            drawColor = Color.Lerp(drawColor, Color.Blue.MultiplyRGB(drawColor), 0.25f);
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