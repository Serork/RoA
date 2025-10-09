using Microsoft.Xna.Framework;

namespace RoA.Core.Utility.Extensions;
static class ColorExtensions {
    public static Color ModifyRGB(this Color color, float modifier) => (color * modifier) with { A = color.A };
}
