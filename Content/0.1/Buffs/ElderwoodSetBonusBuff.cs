using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class ElderwoodSetBonusBuff : ModBuff {
    public const byte TIME = 150;

    public override void SetStaticDefaults() {

    }

    public override void Update(Player player, ref int buffIndex) {
        int buffTime = player.buffTime[buffIndex];
        if (buffTime > 0) {
            byte maxTime = TIME;
            float value = MathHelper.Clamp((float)buffTime / maxTime, 0f, 1f);
            float value2 = 1f - value * 0.5f;
            player.moveSpeed *= value2;
        }
    }
}
