using System;
using Microsoft.Xna.Framework;

namespace RoA.Core.Utility;

public static class ColorExtensions {
	public static Color Bright(this Color color, float mult) {
		int r = Math.Max(0, Math.Min(255, (int)(color.R * mult)));
		int g = Math.Max(0, Math.Min(255, (int)(color.G * mult)));
		int b = Math.Max(0, Math.Min(255, (int)(color.B * mult)));
		return new Color(r, g, b, color.A);
	}

    public static Color MultiplyAlpha(this Color color, float alpha) => new(color.R, color.G, color.B, (int)(color.A / 255f * MathHelper.Clamp(alpha, 0f, 1f) * 255f));
}
