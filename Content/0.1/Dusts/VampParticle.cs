using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class VampParticle : ModDust {
    public override void OnSpawn(Dust dust) {
        dust.noGravity = true;
        dust.noLight = true;
    }

    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(255, 255, 255, 0) * 0.9f * (1f - dust.alpha / 255f);

    public override bool Update(Dust dust) {
        if (dust.alpha > 0 && dust.scale > 0.75f) {
            dust.alpha -= 5;
            if (dust.alpha < 0) {
                dust.alpha = 0;
            }
        }
        else {
            dust.scale -= 0.015f - Math.Min(dust.scale * 0.009f, 0.005f);
            if (dust.scale < 0.01f) {
                dust.active = false;
            }
        }
        if (dust.scale < 0.75f) {
            dust.alpha += 15;
            if (dust.alpha >= 255) {
                dust.active = false;
            }
        }
        dust.velocity = dust.velocity.RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f));
        if (dust.velocity.Length() > 0.1f) {
            dust.velocity *= 0.99f;
        }
        dust.rotation += 0.005f;
        dust.position += dust.velocity;

        return false;
    }
}
