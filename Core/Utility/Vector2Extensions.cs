using Microsoft.Xna.Framework;

using Terraria;

namespace RoA.Core.Utility;

static class Vector2Extensions {
    public static Vector2 SafeNormalize(this Vector2 vector2) => Utils.SafeNormalize(vector2, Vector2.Zero);
}
