using Microsoft.Xna.Framework;

using RoA.Core.Utility.Extensions;

using System;

using Terraria;

namespace RoA.Core.Utility;

static class MathUtils {
    public static float YoYo(float value) => MathUtils.Clamp01((double)value <= 0.5 ? value * 2f : (float)(1.0 - ((double)value - 0.5) * 2.0));

    public static Vector2 NormalizeWithMaxLength(this Vector2 vector2, float maxSpeed) {
        Vector2 result = vector2;
        if (result.Length() > maxSpeed) {
            result = result.SafeNormalize() * maxSpeed;
        }

        return result;
    }

    public static int GetDirectionTo(this Vector2 vector2, Vector2 destination) => (vector2.X - destination.X).GetDirection();

    public static float SineBumpEasing(float amount, float degree = 1f) => (float)Math.Sin(amount * MathHelper.Pi);

    public static float Clamp01(float value) => value <= 0f ? 0f : value >= 1f ? 1f : value;
    public static double Clamp01(double value) => value <= 0.0 ? 0.0 : value >= 1.0 ? 1.0 : value;

    public static bool Approximately(float a, float b, float tolerance = 1E-06f) => Math.Abs(a - b) < tolerance;
    public static bool Approximately(Vector2 a, Vector2 b, float tolerance = 1E-06f) => Approximately(a.X, b.X, tolerance) && Approximately(a.Y, b.Y, tolerance);

    public static uint PseudoRand(ref uint seed) {
        seed ^= seed << 13;
        seed ^= seed >> 17;
        return seed;
    }

    public static float PseudoRandRange(ref uint seed, float min, float max) => min + (float)((double)(PseudoRand(ref seed) & 1023U) / 1024.0 * ((double)max - (double)min));
    public static float PseudoRandRange(ref uint seed, float max) => PseudoRandRange(ref seed, max > 0f ? 0f : -max, max > 0f ? max : 0f);

    public static float DistanceX(this Vector2 a, Vector2 b) => MathF.Abs(a.X - b.X);
    public static float DistanceY(this Vector2 a, Vector2 b) => MathF.Abs(a.Y - b.Y);
}
