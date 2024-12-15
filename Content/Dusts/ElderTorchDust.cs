using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class ElderTorchDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(lightColor.R, lightColor.G, lightColor.B, 25);

    public override void OnSpawn(Dust dust) {
        dust.velocity.Y = (float)Main.rand.Next(-10, 6) * 0.1f;
        dust.velocity.X *= 0.3f;
        dust.scale *= 0.7f;
    }

    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        if (!dust.noGravity)
            dust.velocity.Y += 0.05f;

        if (!dust.noLight && !dust.noLightEmittence) {
            float num56 = dust.scale * 1.4f;
            if (num56 > 1f)
                num56 = 1f;

            Lighting.AddLight(dust.position, 0.25f * num56, 0.65f * num56, 0.85f * num56);
        }

        return false;
    }
}