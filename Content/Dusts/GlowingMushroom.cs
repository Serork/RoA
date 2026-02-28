using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class GlowingMushroom : ModDust {
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
        dust.velocity *= 0f;
    }

    public override bool Update(Dust dust) {
        dust.BasicDust(applyGravity: false);

        if (dust.customData != null && dust.customData is Player) {
            Player player9 = (Player)dust.customData;
            dust.position += player9.position - player9.oldPosition;
        }

        dust.velocity.X += (float)Main.rand.Next(-10, 11) * 0.01f;
        dust.velocity.Y += (float)Main.rand.Next(-10, 11) * 0.01f;
        if ((double)dust.velocity.X > 0.75)
            dust.velocity.X = 0.75f;

        if ((double)dust.velocity.X < -0.75)
            dust.velocity.X = -0.75f;

        if ((double)dust.velocity.Y > 0.75)
            dust.velocity.Y = 0.75f;

        if ((double)dust.velocity.Y < -0.75)
            dust.velocity.Y = -0.75f;

        dust.scale += 0.007f;
        float num103 = dust.scale * 0.7f;
        if (num103 > 1f)
            num103 = 1f;

        if (!dust.noLightEmittence) {
            Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num103 * 0.4f, num103 * 0.9f, num103);
        }

        return false;
    }
}
