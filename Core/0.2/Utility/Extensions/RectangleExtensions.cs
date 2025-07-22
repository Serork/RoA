using Microsoft.Xna.Framework;

using Terraria;

namespace RoA.Core.Utility;

static partial class RectangleExtensions {
    public static Rectangle AdjustX(this Rectangle rectangle, int value) => new(rectangle.X + value, rectangle.Y, rectangle.Width, rectangle.Height);
    public static Rectangle AdjustY(this Rectangle rectangle, int value) => new(rectangle.X, rectangle.Y + value, rectangle.Width, rectangle.Height);
    public static Rectangle AdjustPosition(this Rectangle rectangle, Point value) => rectangle.AdjustX(value.X).AdjustY(value.Y);
    public static Rectangle AdjustPosition(this Rectangle rectangle, Vector2 value) => rectangle.AdjustPosition(value.ToPoint());

    public static Rectangle AdjustWidth(this Rectangle rectangle, int value) => new(rectangle.X, rectangle.Y, rectangle.Width + value, rectangle.Height);
    public static Rectangle AdjustHeight(this Rectangle rectangle, int value) => new(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height + value);
    public static Rectangle AdjustSize(this Rectangle rectangle, Point size) => rectangle.AdjustWidth(size.X).AdjustHeight(size.Y);
}
