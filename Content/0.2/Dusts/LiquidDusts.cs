using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Tar : LiquidDust {
}

abstract class LiquidDust : ModDust {
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
        dust.velocity *= 0.1f;
        dust.velocity.Y = -0.5f;
    }

    public override bool Update(Dust dust) {
        dust.BasicDust();

        Dust.lavaBubbles++;

        float num100 = dust.scale * 0.3f + 0.4f;
        if (num100 > 1f)
            num100 = 1f;

        if (dust.noGravity) {
            dust.scale += 0.03f;
            if (dust.scale < 1f)
                dust.velocity.Y += 0.075f;

            dust.velocity.X *= 1.08f;
            if (dust.velocity.X > 0f)
                dust.rotation += 0.01f;
            else
                dust.rotation -= 0.01f;
        }
        else {
            if (!Collision.WetCollision(new Vector2(dust.position.X, dust.position.Y - 8f), 4, 4)) {
                dust.scale = 0f;
            }
            else {
                dust.alpha += Main.rand.Next(2);
                if (dust.alpha > 255)
                    dust.scale = 0f;

                dust.velocity.Y = -0.5f;
                if (dust.type == 34) {
                    dust.scale += 0.005f;
                }
                else {
                    dust.alpha++;
                    dust.scale -= 0.01f;
                    dust.velocity.Y = -0.2f;
                }

                dust.velocity.X += Main.rand.Next(-10, 10) * 0.002f;
                if (dust.velocity.X < -0.25)
                    dust.velocity.X = -0.25f;

                if (dust.velocity.X > 0.25)
                    dust.velocity.X = 0.25f;
            }
        }

        return false;
    }
}
