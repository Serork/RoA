using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

class Fireblossom : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(Color.White.R, Color.White.G, Color.White.B, 0) * (1f - (float)dust.alpha / 255f);

    public override void SetStaticDefaults() => UpdateType = DustID.MeteorHead;

    public override bool PreDraw(Dust dust) {
        if (!dust.noLight)
            Lighting.AddLight(dust.position, 243f / 255f * 0.75f, 138f / 255f * 0.75f, 3f / 255f * 0.75f);

        return base.PreDraw(dust);
    }
}

sealed class Fireblossom2 : Fireblossom {
    public override string Texture => base.Texture[..^1];

    public override void SetStaticDefaults() {
        UpdateType = DustID.MeteorHead;
    }

    public override void OnSpawn(Dust dust) {
        dust.scale *= 0.75f;
    }
}
