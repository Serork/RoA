using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class BlueRoseDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color shadowColor = lightColor;
        shadowColor = shadowColor.MultiplyAlpha(0.925f);
        shadowColor = shadowColor.MultiplyAlpha(Helper.Wave(0.75f, 1f, 20f, (float)dust.customData));

        return shadowColor;
    }

    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        return false;
    }
}
