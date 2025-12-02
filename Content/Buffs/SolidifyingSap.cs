using Microsoft.Xna.Framework;

using RoA.Content.Dusts;

using Terraria;
using Terraria.DataStructures;
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
        player.velocity.X *= 0.85f;
        player.GetModPlayer<SolidifyingSapPlayer>().solidifyingSap = true;
    }

    public override void Update(NPC npc, ref int buffIndex) {
        npc.GetGlobalNPC<SolidifyingSapNPC>().solidifyingSap = true;
        if (npc.boss) return;
        float value = MathHelper.Clamp(npc.knockBackResist, 0f, 1f);
        float value2 = MathHelper.Lerp(value, 1f, (1f - value) * 0.999f);
        npc.velocity.X *= value2;
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
                int dust = Dust.NewDust(drawInfo.Position - new Vector2(2f, 2f), Player.width, Player.height, ModContent.DustType<Galipot>(), Player.velocity.X * 0.4f, 3f, 50, default, 0.8f);
                Main.dust[dust].noGravity = false;
                Main.dust[dust].velocity *= 1.1f;
                Main.dust[dust].velocity.Y = Main.dust[dust].velocity.Y * -0.5f;
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
            drawColor = Color.Lerp(drawColor, Color.LightGoldenrodYellow.MultiplyRGB(drawColor), 0.25f);
            if (Main.rand.NextBool(1, 3)) {
                for (int k = 0; k < 2; k++) {
                    if (Main.rand.NextBool(1, 2)) {
                        var dust = Dust.NewDust(npc.position, npc.width, npc.height, ModContent.DustType<Galipot>(), npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 50, default, 0.8f);
                        Main.dust[dust].noGravity = true;
                        Main.dust[dust].noLight = true;
                        Main.dust[dust].velocity.Y += 2f;
                    }
                }
            }
        }
    }
}