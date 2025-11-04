using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class CrimsonInsectDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(255, 255, 255, 0) * 0.9f;

    public override bool Update(Dust dust) {
        if (!dust.noGravity) {
            dust.velocity *= 0.9f;
        }

        float num99 = dust.scale * 0.5f;
        if (num99 > 1f)
            num99 = 1f;

        if (!dust.noLightEmittence)
            Lighting.AddLight((int)(dust.position.X / 16f), (int)(dust.position.Y / 16f), num99 * 1f, num99 * 0.75f, num99 * 0.1f);

        return base.Update(dust);
    }
}