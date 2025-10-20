using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

sealed class ManaCrystalGore : ModGore {
    public override Color? GetAlpha(Gore gore, Color lightColor) {
        int frameCount = gore.frameCounter;
        Color result;
        switch (frameCount) {
            case 0:
                result = Color.Lerp(new Color(227, 170, 230), new Color(175, 89, 192), 0.5f);
                break;
            case 1:
                result = Color.Lerp(new Color(210, 182, 241), new Color(164, 109, 224), 0.5f);
                break;
            default:
                result = Color.Lerp(new Color(184, 200, 241), new Color(133, 148, 186), 0.5f);
                break;
        }
        result.A = 200;
        result *= 0.8f;

        return result * (1f - gore.alpha / 255f);
    }

    public override bool Update(Gore gore) {
        gore.frame = 0;

        GoreHelper.FadeOutOverTime(gore);

        return base.Update(gore);
    }
}
