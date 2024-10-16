using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

public class BrightstoneDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White;

    public override bool Update(Dust dust) {
        dust.velocity *= 0.98f;

        float num94 = dust.scale * 0.8f;

        if (num94 > 1f)
            num94 = 1f;

        Lighting.AddLight(dust.position, new Color(238, 225, 111).ToVector3() * 0.6f * num94);

        return true;
    }
}