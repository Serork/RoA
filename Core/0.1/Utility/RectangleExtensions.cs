using Microsoft.Xna.Framework;

using Terraria;

namespace RoA.Core.Utility;

static partial class RectangleExtensions {
    public static Vector2 Centered(this Rectangle rectangle) => rectangle.Size() / 2f;

    public static Vector2 BottomCenter(this Rectangle rectangle) => new(rectangle.Width / 2f, rectangle.Height);
    public static Vector2 TopCenter(this Rectangle rectangle) => new(rectangle.Width / 2f, 0);
    public static Vector2 LeftCenter(this Rectangle rectangle) => new(rectangle.Width / 2f, rectangle.Height / 2f);
}
