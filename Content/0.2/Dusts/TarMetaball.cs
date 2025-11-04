using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class TarMetaball : ModDust {
    public override bool PreDraw(Dust dust) => false;

    public override void OnSpawn(Dust dust) {
        dust.velocity.Y *= 0.5f;
    }

    public override bool Update(Dust dust) {
        bool flag = false;
        if (dust.customData is float val && val == 2f) {
            flag = true;
        }

        if (!flag) {
            dust.velocity.Y = MathF.Abs(dust.velocity.Y);
        }
        else {
            if (Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4)) {
                dust.scale *= 0.95f;
                dust.velocity *= 0.25f;
            }
        }

        dust.BasicDust();

        if (dust.customData is not float && Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4)) {
            dust.scale *= 0.95f;
            dust.velocity *= 0.25f;
        }

        if (dust.scale < 0.75f) {
            dust.scale *= 0.9f;
        }

        dust.scale -= 0.01f;

        return false;
    }
}
