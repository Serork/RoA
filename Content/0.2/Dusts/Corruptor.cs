using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Corruptor : ModDust {
    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        if (dust.customData is int) {
            if (Collision.SolidCollision(dust.position - Vector2.One * 5f, 10, 10) && dust.fadeIn == 0f) {
                dust.scale *= 0.9f;
                dust.velocity *= 0.25f;
            }
        }

        return false;
    }
}
