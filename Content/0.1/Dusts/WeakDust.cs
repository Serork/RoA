using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class WeakDust : ModDust {
    public override void OnSpawn(Dust dust) {
        dust.noLight = false;
        dust.velocity /= 3.5f;
        dust.scale *= 0.8f;
    }

    public override bool Update(Dust dust) {
        Lighting.AddLight(dust.position, 243f / 255f, 138f / 255f, 3f / 255f);
        dust.scale -= 0.03f;
        if (dust.scale < 0.25)
            dust.active = false;
        return false;
    }

    public override bool MidUpdate(Dust dust) {
        if (!dust.noGravity) dust.velocity.Y += 0.05f;
        if (!dust.noLight) {
            float strength = dust.scale * 1.4f;
            if (strength > 1f) strength = 1f;
            Lighting.AddLight(dust.position, 0.1f * strength, 0.1f * strength, 0f * strength);
        }
        return false;
    }
    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(Color.White.R, Color.White.G, Color.White.B, 120) * 0.9f * (1f - (float)dust.alpha / 255f);
}