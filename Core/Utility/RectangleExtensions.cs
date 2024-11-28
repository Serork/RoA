using Microsoft.Xna.Framework;

using Terraria;

namespace RoA.Core.Utility;

static class RectangleExtensions {
    public static Vector2 Centered(this Rectangle rectangle) => rectangle.Size() / 2f;

    public static Vector2 BottomCenter(this Rectangle rectangle) => new(rectangle.Width / 2f, rectangle.Height);
}
