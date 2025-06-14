using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RoA.Core.Utility;

static class GeometryUtils {
    public static Rectangle CenteredSquare(Vector2 position, int size) => new() { X = (int)position.X - size / 2, Y = (int)position.Y - size / 2, Width = size, Height = size };
}
