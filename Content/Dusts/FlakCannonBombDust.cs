using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class FlakCannonBombDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(Color.White.R, Color.White.G, Color.White.B, 25);

    public override void OnSpawn(Dust dust) {
        dust.velocity.Y = (float)Main.rand.Next(-10, 6) * 0.1f;
        dust.velocity.X *= 0.3f;
        dust.scale *= 0.7f;
    }

    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        if (dust.customData != null && dust.customData is int) {
            if ((int)dust.customData == 0) {
                if (Collision.SolidCollision(dust.position - Vector2.One * 5f, 10, 10) && dust.fadeIn == 0f) {
                    dust.scale *= 0.9f;
                    dust.velocity *= 0.25f;
                }
            }
            else if ((int)dust.customData == 1) {
                dust.scale *= 0.98f;
                dust.velocity.Y *= 0.98f;
                if (Collision.SolidCollision(dust.position - Vector2.One * 5f, 10, 10) && dust.fadeIn == 0f) {
                    dust.scale *= 0.9f;
                    dust.velocity *= 0.25f;
                }
            }
        }

        if (!dust.noLight && !dust.noLightEmittence) {
            Lighting.AddLight(dust.position, new Color(143, 255, 133).ToVector3() * 0.5f * dust.scale);
        }

        return false;
    }

    public override bool PreDraw(Dust dust) => true;
}
