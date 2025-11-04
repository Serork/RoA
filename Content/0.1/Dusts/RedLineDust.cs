using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class RedLineDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(255 - dust.alpha, 255 - dust.alpha, 255 - dust.alpha, 255 - dust.alpha) * 0.9f;

    public override void OnSpawn(Dust dust) {
        dust.velocity *= 0.3f;
        dust.noGravity = true;
        dust.scale *= 1f;
    }

    public override bool Update(Dust dust) {
        //Lighting.AddLight(dust.position, new Vector3(1f, 0.2f, 0.2f));

        dust.position += dust.velocity;
        dust.scale *= 0.95f;
        dust.velocity.Y *= 0.9f;
        if (dust.scale < 0.1f) {
            dust.active = false;
        }

        return false;
    }
}
