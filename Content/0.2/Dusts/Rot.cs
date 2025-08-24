using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Rot : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor)
        => dust.color.MultiplyRGBA(lightColor * (0.1f + dust.alpha / 250f) * (float)Math.Sin(dust.fadeIn / 120f * MathHelper.Pi));

    public override void OnSpawn(Dust dust) {
        dust.fadeIn = 0;
        dust.noLight = false;
        dust.rotation = Main.rand.NextFloat(6.28f);
        dust.frame = new Rectangle(0, 0, 24, 26);

        dust.color = Color.Brown * Main.rand.NextFloat(0.85f, 1.25f);
    }

    public override bool Update(Dust dust) {
        Helper.ApplyWindPhysics(dust.position, ref dust.velocity);

        dust.position += dust.velocity;
        float velocity = dust.velocity.Y / 40f * (dust.alpha > 7 ? -1 : 1);
        Vector2 center = dust.position + Vector2.One.RotatedBy(dust.rotation) * 18f * dust.scale;
        dust.scale *= 0.999f;
        Vector2 velocityTo = dust.position + Vector2.One.RotatedBy(dust.rotation + velocity) * 18f * dust.scale;
        dust.rotation += velocity;
        dust.position += (center - velocityTo) * 0.3f;
        dust.fadeIn += 3;
        if (dust.fadeIn > 120) {
            dust.active = false;
        }
        return false;
    }
}