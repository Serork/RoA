using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;

namespace RoA.Core.Utility;

static partial class SpriteBatchExtensions {
    public static void Line(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, SpriteEffects effects = 0, Texture2D? texture = null) => LineAngle(spriteBatch, start, Utils.AngleTo(start, end), Vector2.Distance(start, end), color, effects, texture);
    public static void Line(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness, SpriteEffects effects = 0, Texture2D? texture = null) => LineAngle(spriteBatch, start, Utils.AngleTo(start, end), Vector2.Distance(start, end), color, thickness, effects, texture);

    public static void LineAngle(this SpriteBatch spriteBatch, Vector2 start, float angle, float length, Color color, SpriteEffects effects = 0, Texture2D? texture = null) =>
        spriteBatch.Draw(texture ?? ResourceManager.Pixel, start - Main.screenPosition, null, color, angle, Vector2.Zero, texture != null ? texture.Size() / 2f : new Vector2(length, 1f), effects, 0f);

    public static void LineAngle(this SpriteBatch spriteBatch, Vector2 start, float angle, float length, Color color, float thickness, SpriteEffects effects = 0, Texture2D? texture = null) =>
        spriteBatch.Draw(texture ?? ResourceManager.Pixel, start - Main.screenPosition, null, color, angle, texture != null ? texture.Size() / 2f : new Vector2(0.0f, 0.5f), new Vector2(length, thickness), effects, 0f);

    public static void BeginBlendState(this SpriteBatch spriteBatch, BlendState state, SamplerState samplerState = null, bool isUI = false, bool shader = false) {
        spriteBatch.End();
        spriteBatch.Begin(shader ? SpriteSortMode.Immediate : SpriteSortMode.Deferred, state, samplerState ?? Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, isUI ? Main.UIScaleMatrix : Main.GameViewMatrix.ZoomMatrix);
    }

    public static void EndBlendState(this SpriteBatch spriteBatch, bool isUI = false) {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, isUI ? SamplerState.AnisotropicClamp : Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, isUI ? Main.UIScaleMatrix : Main.GameViewMatrix.ZoomMatrix);
    }

    public static void BeginWorld(this SpriteBatch spriteBatch, bool shader = false, Matrix? overrideMatrix = null, BlendState state = null) {
        var matrix = overrideMatrix ?? Main.Transform;
        if (!shader) {
            spriteBatch.Begin(SpriteSortMode.Deferred, state ?? BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, matrix);
        }
        else {
            spriteBatch.Begin(SpriteSortMode.Immediate, state ?? BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, matrix);
        }
    }
}
