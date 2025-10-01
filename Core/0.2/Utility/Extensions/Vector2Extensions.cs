using Microsoft.Xna.Framework;

using Terraria;

namespace RoA.Core.Utility.Extensions;

static class Vector2Extensions {
    public static Vector2 SafeNormalize(this Vector2 vector2) => Utils.SafeNormalize(vector2, Vector2.Zero);

    public static Vector2 AdjustX(this Vector2 vector2, float value) => new(vector2.X, vector2.Y + value);
    public static Vector2 AdjustY(this Vector2 vector2, float value) => new(vector2.X, vector2.Y + value);

    public static bool IsWithinRange(this Vector2 vector2, float value) => vector2.Length() < value;
}
