using Microsoft.Xna.Framework;

namespace RoA.Core.Utility;

static class GeometryUtils {
    public static Rectangle CenteredSquare(Vector2 position, int size) => new() { X = (int)position.X - size / 2, Y = (int)position.Y - size / 2, Width = size, Height = size };
    public static Rectangle Square(Vector2 position, int size) => new() { X = (int)position.X, Y = (int)position.Y, Width = size, Height = size };

    public static Rectangle TopRectangle(Vector2 center, Vector2 size) => new((int)(center.X - size.X / 2f), (int)center.Y, (int)size.X, (int)size.Y);
}
