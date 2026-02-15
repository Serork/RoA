using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class FilamentDust : ModDust {
    public override void SetStaticDefaults() {
     
    }

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

        //if (!dust.noLight && !dust.noLightEmittence) {
        //    float num56 = dust.scale * 1.4f;
        //    if (num56 > 0.6f)
        //        num56 = 0.6f;

        //    Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num56, num56 * 0.65f, num56 * 0.4f);
        //}

        return false;
    }
}
