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
        dust.velocity.Y = MathF.Abs(dust.velocity.Y);

        dust.BasicDust();

        if (dust.customData is not float && Collision.SolidCollision(dust.position - Vector2.One * 2, 4, 4)) {
            dust.scale *= 0.9f;
            dust.velocity *= 0.25f;
        }

        if (dust.scale < 0.75f) {
            dust.scale *= 0.9f;
        }

        dust.scale -= 0.01f;

        return false;
    }
}
