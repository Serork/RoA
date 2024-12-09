using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class TectonicDust : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = -1;

    public override bool Update(Dust dust) {
        if (dust.customData is int v && v == 1) {
            dust.scale -= 0.02f;
            //dust.position.Y += 0.02f;
            if (dust.scale < 0.5f) {
                dust.alpha += 20;
                if (dust.alpha >= 255 || dust.scale < 0.01f) {
                    dust.active = false;
                }
            }

            if (Collision.SolidCollision(dust.position, 1, 1)) {
                dust.velocity = Vector2.Zero;
            }
            else {
                dust.BasicDust();
            }
        }
        else {
            dust.BasicDust();
        }

        return false;
    }

    public override bool MidUpdate(Dust dust) {
        return base.MidUpdate(dust);
    }
}