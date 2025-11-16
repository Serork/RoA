using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Water : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => new Color(250, 250, 250, 150).MultiplyRGB(lightColor) * (1f - dust.alpha / 255f);

    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        return false;
    }
}
