using Microsoft.Xna.Framework;

using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class ManaCrystalShard : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color result = dust.color;
        result.A = 200;
        result *= 0.8f;

        return result * (1f - dust.alpha / 255f);
    }

    public override bool Update(Dust dust) {
        dust.BasicDust();

        return false;
    }

    public override bool PreDraw(Dust dust) {
        dust.QuickDraw(Texture2D.Value);

        return false;
    }
}
