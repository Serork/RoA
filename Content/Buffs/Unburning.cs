using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Unburning : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<UnburningPlayer>().fireRegen = true;
}

sealed class UnburningPlayer : ModPlayer {
    public bool fireRegen;

    public override void ResetEffects() => fireRegen = false;

    public override void UpdateLifeRegen() {
        if (fireRegen) {
            Player.lifeRegenTime += 4;
            Player.lifeRegen += 4;
        }
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright) {
        if (fireRegen && !drawInfo.drawPlayer.dead) {
            if (Main.rand.NextBool(4) && drawInfo.shadow == 0.0) {
                int dust = Dust.NewDust(drawInfo.Position - new Vector2(2f, 2f), Player.width + 4, Player.height + 4, DustID.Torch, Player.velocity.X * 0.4f, Player.velocity.Y * 0.4f, 100, default, 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 1.8f;
                Main.dust[dust].velocity.Y -= 0.5f;
            }
            Lighting.AddLight(Player.Center, new Vector3(0.2f, 0.1f, 0.1f));
        }
    }
}
