using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class BloodShedAxesDebuff : ModBuff {
    public override string Texture => ResourceManager.EmptyTexture;

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<BloodShedAxesDebuffHandler1>().IsEffectActive = true;

    public override void Update(NPC npc, ref int buffIndex) => npc.GetGlobalNPC<BloodShedAxesDebuffHandler2>().IsEffectActive = true;

    private class BloodShedAxesDebuffHandler1 : ModPlayer {
        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
            if (drawInfo.drawPlayer.dead || !drawInfo.drawPlayer.active || drawInfo.shadow != 0f) {
                return;
            }

            if (IsEffectActive) {
                if (Main.rand.NextBool(8)) {
                    int dust = Dust.NewDust(drawInfo.Position - new Vector2(2f, 2f), Player.width + 4, Player.height + 4, DustID.Blood, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 1f);
                    Main.dust[dust].noGravity = false;
                    Main.dust[dust].velocity *= 0.8f;
                    Main.dust[dust].velocity.X *= 0.5f;
                }
            }
        }
    }

    private class BloodShedAxesDebuffHandler2 : GlobalNPC {
        public bool IsEffectActive;

        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc) => IsEffectActive = false;

        public override void DrawEffects(NPC npc, ref Color drawColor) {
            if (!npc.active) {
                return;
            }

            if (IsEffectActive) {
                if (Main.rand.NextBool(8)) {
                    int dust = Dust.NewDust(npc.position - new Vector2(2f, 2f), npc.width + 4, npc.height + 4, DustID.Blood, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f, 100, default, 1f);
                    Main.dust[dust].noGravity = false;
                    Main.dust[dust].velocity *= 0.8f;
                    Main.dust[dust].velocity.X *= 0.5f;
                }
            }
        }
    }
}
