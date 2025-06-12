﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
}
