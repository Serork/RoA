using Microsoft.Xna.Framework;

using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;

namespace RoA.Core.Utility;

static class MathUtils {
    public static Vector2 ClosestTo(this List<Vector2> list, Vector2 to) {
        Vector2 vector2 = list[0];
        float num1 = Vector2.DistanceSquared(list[0], to);
        for (int index = 1; index < list.Count; ++index) {
            float num2 = Vector2.DistanceSquared(list[index], to);
            if ((double)num2 < (double)num1) {
                num1 = num2;
                vector2 = list[index];
            }
        }
        return vector2;
    }

    public static Vector2 ClosestTo(this Vector2[] list, Vector2 to) {
        Vector2 vector2 = list[0];
        float num1 = Vector2.DistanceSquared(list[0], to);
        for (int index = 1; index < list.Length; ++index) {
            float num2 = Vector2.DistanceSquared(list[index], to);
            if ((double)num2 < (double)num1) {
                num1 = num2;
                vector2 = list[index];
            }
        }
        return vector2;
    }

    public static Vector2 ClosestTo(this Vector2[] list, Vector2 to, out int index) {
        index = 0;
        Vector2 vector2 = list[0];
        float num1 = Vector2.DistanceSquared(list[0], to);
        for (int index1 = 1; index1 < list.Length; ++index1) {
            float num2 = Vector2.DistanceSquared(list[index1], to);
            if ((double)num2 < (double)num1) {
                index = index1;
                num1 = num2;
                vector2 = list[index1];
            }
        }
        return vector2;
    }

    public static Vector2 TurnRight(this Vector2 vector2) => new Vector2(-vector2.Y, vector2.X);
    public static Vector2 TurnLeft(this Vector2 vector2) => vector2.TurnRight().TurnRight().TurnRight();

    public static float ClampedDistanceProgress(Vector2 a, Vector2 b, float startOffset = 0f, float maxDistance = 0f) {
        Vector2 center = a,
                playerCenter = b;
        float distance = playerCenter.Distance(center);
        float distance2 = (playerCenter + b.DirectionTo(center) * startOffset).Distance(center);
        float distanceProgress = Clamp01(distance2 / maxDistance);
        if (distance < startOffset) {
            distanceProgress = 0f;
        }
        return 1f - distanceProgress;
    }

    public static int GetPercentageFromModifier(float value) => (int)(MathF.Round((value - 1f) * 100f));

    public static ushort SecondsToFrames(float seconds) => (ushort)MathF.Round(seconds * 60f);

    public static ushort MinutesToFrames(float minutes) => (ushort)MathF.Round(minutes * 3600f);

    public static float Sin01(float x) => MathF.Sin(x) * 0.5f + 0.5f;

    public static float Cos01(float x) => MathF.Cos(x) * 0.5f + 0.5f;

    public static float Repeat(float value, float length) {
        float quotient = MathF.Floor(value / length);

        return value - quotient * length;
    }

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
