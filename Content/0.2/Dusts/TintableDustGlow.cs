using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class TintableDustGlow : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.TintableDustLighted;

    public override Color? GetAlpha(Dust dust, Color lightColor) {
        float num = (float)(255 - dust.alpha) / 255f;
        num = (num + 9f) / 10f;
        float num6 = (int)((float)(int)lightColor.R * num);
        float num5 = (int)((float)(int)lightColor.G * num);
        float num4 = (int)((float)(int)lightColor.B * num);
        int num8 = lightColor.A - dust.alpha;
        if (num8 < 0)
            num8 = 0;

        if (num8 > 255)
            num8 = 255;

        return new Color(num6, num5, num4, num8);
    }
}
