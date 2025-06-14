using Microsoft.Xna.Framework;

namespace RoA.Core.Utility;

static partial class RectangleExtensions {
    public static Rectangle AdjustX(this Rectangle rectangle, int value) => new(rectangle.X + value, rectangle.Y, rectangle.Width, rectangle.Height);
    public static Rectangle AdjustY(this Rectangle rectangle, int value) => new(rectangle.X, rectangle.Y + value, rectangle.Width, rectangle.Height);
    public static Rectangle AdjustPosition(this Rectangle rectangle, Point value) => new(rectangle.X + value.X, rectangle.Y + value.Y, rectangle.Width, rectangle.Height);

    public static Rectangle AdjustWidth(this Rectangle rectangle, int value) => new(rectangle.X, rectangle.Y, rectangle.Width + value, rectangle.Height);
    public static Rectangle AdjustHeight(this Rectangle rectangle, int value) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height + value);
    public static Rectangle AdjustSize(this Rectangle rectangle, Point size) => new(rectangle.X, rectangle.Y, rectangle.Width + size.X, rectangle.Height + size.Y);
}
