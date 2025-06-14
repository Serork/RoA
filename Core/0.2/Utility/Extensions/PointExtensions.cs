using Microsoft.Xna.Framework;

namespace RoA.Core.Utility.Extensions;

static class PointExtensions {
    public static Point AdjustX(this Point vector2, int value) => new(vector2.X, vector2.Y + value);
    public static Point AdjustY(this Point vector2, int value) => new(vector2.X, vector2.Y + value);
}
