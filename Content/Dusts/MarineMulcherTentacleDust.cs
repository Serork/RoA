using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class MarineMulcherTentacleDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        float num = (255 - dust.alpha) / 255f;
        num = (num + 3f) / 4f;
        lightColor = Color.White * 0.5f;
        int num6 = (int)(lightColor.R * num);
        int num5 = (int)(lightColor.G * num);
        int num4 = (int)(lightColor.B * num);
        int num8 = lightColor.A - dust.alpha;
        if (num8 < 0)
            num8 = 0;

        if (num8 > 255)
            num8 = 255;

        if (dust.customData is bool) {
            return Color.White with { A = 0 } * 0.25f;
        }

        return new Color(num6, num5, num4, num8);
    }

    public override bool Update(Dust dust) {
        dust.BasicDust();

        if (dust.fadeIn >= 100f) {
            if ((double)dust.scale >= 1.5)
                dust.scale -= 0.01f;
            else
                dust.scale -= 0.05f;

            if ((double)dust.scale <= 0.5)
                dust.scale -= 0.05f;

            if ((double)dust.scale <= 0.25)
                dust.scale -= 0.05f;
        }

        if (dust.customData is bool) {
            dust.velocity.Y -= 0.1f;
            dust.scale *= 0.9f;
        }

        dust.velocity *= 0.94f;
        dust.scale += 0.002f;
        float num93 = dust.scale;
        if (dust.noLight) {
            num93 *= 0.1f;
            dust.scale -= 0.06f;
            if (dust.scale < 1f)
                dust.scale -= 0.06f;

            if (Main.player[Main.myPlayer].wet)
                dust.position += Main.player[Main.myPlayer].velocity * 0.5f;
            else
                dust.position += Main.player[Main.myPlayer].velocity;
        }

        if (num93 > 1f)
            num93 = 1f;

        Lighting.AddLight(dust.position, new Color(252, 144, 144).ToVector3() * 0.75f * num93);

        return false;
    }
}
