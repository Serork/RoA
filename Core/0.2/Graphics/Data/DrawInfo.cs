using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility.Extensions;

using Terraria;

namespace RoA.Core.Graphics.Data;

readonly struct DrawInfo() {
    public static DrawInfo Default => new();

    public Vector2 Origin { get; init; } = Vector2.Zero;

    public Vector2 Offset { get; init; } = Vector2.Zero;

    public float Rotation { get; init; } = 0f;
    public Vector2 Scale { get; init; } = Vector2.One;
    public Color Color { get; init; } = Color.White;

    public SpriteEffects ImageFlip { get; init; } = SpriteEffects.None;

    public Rectangle Clip { get; init; } = Rectangle.Empty;

    public DrawInfo WithScale(float scale) => this with { Scale = Scale * scale };
    public DrawInfo WithScaleX(float scale) => this with { Scale = new Vector2(Scale.X * scale, Scale.Y) };
    public DrawInfo WithScaleY(float scale) => this with { Scale = new Vector2(Scale.X, Scale.Y * scale) };

    public DrawInfo WithColor(Color color) => this with { Color = Color.MultiplyRGB(color) };
    public DrawInfo WithColorModifier(float colorModifier) => this with { Color = Color * colorModifier };
    public DrawInfo WithColorRGBModifier(float colorModifier) => this with { Color = Color.ModifyRGB(colorModifier) };

    public DrawInfo WithRotation(float rotation) => this with { Rotation = Rotation + rotation };
}
