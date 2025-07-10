using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Permafrost : LiquidDust {
    protected override Vector3 LightColorRGB => new(0.4f, 0.9f, 0.8f);
}

sealed class Tar : LiquidDust {
    protected override Vector3 LightColorRGB => Vector3.Zero;
}

abstract class LiquidDust : ModDust {
    protected virtual Vector3 LightColorRGB { get; }

    public override Color? GetAlpha(Dust dust, Color lightColor) {
        float num = (float)(255 - dust.alpha) / 255f;
        num = (num + 3f) / 4f;
        int num6 = (int)((float)(int)lightColor.R * num);
        int num5 = (int)((float)(int)lightColor.G * num);
        int num4 = (int)((float)(int)lightColor.B * num);
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
        DustHelper.BasicDust(dust);

        Dust.lavaBubbles++;

        float num100 = dust.scale * 0.3f + 0.4f;
        if (num100 > 1f)
            num100 = 1f;

        float lightR = LightColorRGB.X, lightG = LightColorRGB.Y, lightB = LightColorRGB.Z;
        Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num100 * lightR, num100 * lightG, num100 * lightB);

        if (dust.noGravity) {
            dust.scale += 0.03f;
            if (dust.scale < 1f)
                dust.velocity.Y += 0.075f;

            dust.velocity.X *= 1.08f;
            if (dust.velocity.X > 0f)
                dust.rotation += 0.01f;
            else
                dust.rotation -= 0.01f;

            float num96 = dust.scale * 0.6f;
            if (num96 > 1f)
                num96 = 1f;

            Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f + 1f), num96 * lightR, num96 * lightG, num96 * lightB);
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

                dust.velocity.X += (float)Main.rand.Next(-10, 10) * 0.002f;
                if ((double)dust.velocity.X < -0.25)
                    dust.velocity.X = -0.25f;

                if ((double)dust.velocity.X > 0.25)
                    dust.velocity.X = 0.25f;
            }
        }

        return false;
    }
}
