using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class LothorPoison : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color result = lightColor * (float)(1f - (float)dust.alpha / 255);
        return result;
    }

    public override void OnSpawn(Dust dust) {
        DustHelper.BasicDust(dust);
    }
}