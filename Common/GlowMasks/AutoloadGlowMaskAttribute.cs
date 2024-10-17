using Microsoft.Xna.Framework;

using System;

namespace RoA.Common.GlowMasks;

[AttributeUsage(AttributeTargets.Class)]
sealed class AutoloadGlowMaskAttribute(byte r = 255, byte g = 255, byte b = 255, byte a = 255) : Attribute {
    public readonly Color GlowColor = new(r, g, b, a);
}