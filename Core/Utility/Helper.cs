using RoA.Common.Common;

using System;

namespace RoA.Utilities;

static class Helper {
    public static float Wave(float minimum, float maximum, float speed = 1f, float offset = 0f) => Wave((float)TimeSystem.TimeForVisualEffects, minimum, maximum, speed, offset);
    public static float Wave(float step, float minimum, float maximum, float speed = 1f, float offset = 0f) => minimum + ((float)Math.Sin(step * (double)speed + (double)offset) + 1f) / 2f * (maximum - minimum);

    public static void AddClamp(ref int value, int add, int min, int max) => AddClamp(ref value, add, min, max);
    public static void AddClamp(ref float value, float add, float min = 0f, float max = 1f) => value = Math.Clamp(value + add, min, max);
}
