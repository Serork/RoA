using Microsoft.Xna.Framework;

using RoA.Core.Data;

using System;

using Terraria;

namespace RoA.Core.Utility;

static class MathUtils {
    public static float Clamp01(float value) => value <= 0f ? 0f : value >= 1f ? 1f : value;
    public static double Clamp01(double value) => value <= 0.0 ? 0.0 : value >= 1.0 ? 1.0 : value;

    public static bool Approximately(float a, float b, float tolerance = 1E-06f) => Math.Abs(a - b) < tolerance;
    public static bool Approximately(Vector2 a, Vector2 b, float tolerance = 1E-06f) => Approximately(a.X, b.X, tolerance) && Approximately(a.Y, b.Y, tolerance);
}
