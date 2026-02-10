using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class NixieTube : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        byte a = (byte)Helper.Wave(0, 65, 5f, 0);
        Color result = (Color.White with { A = a } * 1f).MultiplyRGB(dust.color) * (dust.alpha / 255f);
        result.A = 50;
        return result;
    }

    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        if (dust.customData is Color color) {
            Lighting.AddLight(dust.position, new Vector3(color.R / 255f, color.G / 255f, color.B / 255f));
        }

        if (Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4)) {
            dust.scale *= 0.9f;
            dust.velocity *= 0.25f;
        }

        if (dust.scale < 0.75f) {
            dust.scale *= 0.9f;
        }

        dust.scale -= 0.04f;

        return false;
    }
}
