using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class TarDebuff : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        float num = (255 - dust.alpha) / 255f;
        num = (num + 3f) / 4f;
        int num6 = (int)(lightColor.R * num);
        int num5 = (int)(lightColor.G * num);
        int num4 = (int)(lightColor.B * num);
        int num8 = lightColor.A - dust.alpha;
        if (num8 < 0)
            num8 = 0;

        if (num8 > 255)
            num8 = 255;

        return new Color(num6, num5, num4, num8);
    }

    public override void OnSpawn(Dust dust) {
        dust.velocity.X *= 0.2f;
    }

    public override bool Update(Dust dust) {
        dust.BasicDust(false);

        dust.velocity.X *= 0.97f;
        if (dust.velocity.Y < 0f) {
            dust.velocity.Y = 0f;
        }

        dust.scale *= 0.99f;
        if (dust.scale <= 0.01f) {
            dust.active = false;
        }

        if (!dust.noGravity) {
            dust.velocity.Y += 0.015f;
        }

        if (Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4))
            dust.active = false;

        return false;
    }
}
