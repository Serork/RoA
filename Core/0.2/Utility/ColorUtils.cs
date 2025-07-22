using Microsoft.Xna.Framework;

using System.Collections.Generic;

using Terraria;

namespace RoA.Core.Utility;

static class ColorUtils {
    public static Color GetLerpColor(ref float lerpColorProgress, ref Color lerpColor, List<Color> from) {
        lerpColorProgress += 0.005f;
        int colorCount = from.Count;
        for (int i = 0; i < colorCount; i++) {
            float part = 1f / colorCount;
            float min = part * i;
            float max = part * (i + 1);
            if (lerpColorProgress >= min && lerpColorProgress <= max) {
                lerpColor = Color.Lerp(from[i], from[i == colorCount - 1 ? 0 : (i + 1)], Utils.Remap(lerpColorProgress, min, max, 0f, 1f, true));
            }
        }
        if (lerpColorProgress > 1f) {
            lerpColorProgress = 0f;
        }
        return lerpColor;
    }
}
