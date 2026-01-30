using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class LightCompressorDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color result = dust.color;
        result.A = 125;
        //result *= MathHelper.Min(1, dust.fadeIn / 20f);
        return result;
    }

    public override void OnSpawn(Dust dust) {
        dust.fadeIn = 0;
        dust.noLight = false;
    }

    public override bool Update(Dust dust) {
        Lighting.AddLight(dust.position, (Color.Lerp(Color.SkyBlue, Color.Blue, 0.05f) with { A = 0 }).ToVector3() * 0.75f * dust.scale);

        dust.ApplyDustScale();

        dust.rotation = dust.velocity.ToRotation() + 1.57f;
        dust.position += dust.velocity;

        dust.velocity *= 0.98f;

        dust.fadeIn += 2;

        dust.scale *= 0.95f;

        if (dust.fadeIn > 60) {
            dust.active = false;
        }

        return false;
    }
}
