using Microsoft.Xna.Framework;

using RoA.Common;

using System;

using Terraria;

namespace RoA.Utilities;

static class Helper {
    public static float Wave(float minimum, float maximum, float speed = 1f, float offset = 0f) => Wave((float)TimeSystem.TimeForVisualEffects, minimum, maximum, speed, offset);
    public static float Wave(float step, float minimum, float maximum, float speed = 1f, float offset = 0f) => minimum + ((float)Math.Sin(step * (double)speed + (double)offset) + 1f) / 2f * (maximum - minimum);

    public static void AddClamp(ref int value, int add, int min, int max) => AddClamp(ref value, add, min, max);
    public static void AddClamp(ref float value, float add, float min = 0f, float max = 1f) => value = Math.Clamp(value + add, min, max);

    public static Vector2 VelocityToPoint(Vector2 a, Vector2 b, float speed) {
        Vector2 vector2 = b - a;
        Vector2 velocity = vector2 * (speed / vector2.Length());
        return !velocity.HasNaNs() ? velocity : Vector2.Zero;
    }

    public static float VelocityAngle(Vector2 velocity) => (float)Math.Atan2(velocity.Y, velocity.X) + (float)Math.PI / 2f;

    public static void SmoothClamp(ref float value, float min, float max, float lerpValue) {
        if (value < min) {
            value = MathHelper.Lerp(value, min, lerpValue);
        }
        if (value > max) {
            value = MathHelper.Lerp(value, max, lerpValue);
        }
    }

    // terraria overhaul
    public static float Damp(float source, float destination, float smoothing, float dt) => MathHelper.Lerp(source, destination, 1f - MathF.Pow(smoothing, dt));

    public static float SmoothAngleLerp(this float curAngle, float targetAngle, float amount) {
        float angle;
        if (targetAngle < curAngle) {
            float num = targetAngle + (float)Math.PI * 2f;
            angle = ((num - curAngle > curAngle - targetAngle) ? MathHelper.SmoothStep(curAngle, targetAngle, amount) : MathHelper.SmoothStep(curAngle, num, amount));
        }
        else {
            if (!(targetAngle > curAngle))
                return curAngle;

            float num = targetAngle - (float)Math.PI * 2f;
            angle = ((targetAngle - curAngle > curAngle - num) ? MathHelper.SmoothStep(curAngle, num, amount) : MathHelper.SmoothStep(curAngle, targetAngle, amount));
        }

        return MathHelper.WrapAngle(angle);
    }
}
