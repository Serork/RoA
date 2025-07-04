﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Data;

using System;

using Terraria;

namespace RoA.Core.Utility;

static class SpriteBatchExtensions {
    public static void Draw(this SpriteBatch spriteBatch, Texture2D textureToDraw, Vector2 position, DrawInfo drawInfo, bool onScreen = true) {
        spriteBatch.Draw(textureToDraw, position - (onScreen ? Main.screenPosition : Vector2.Zero), drawInfo.Clip, drawInfo.Color, drawInfo.Rotation, drawInfo.Origin, drawInfo.Scale, drawInfo.ImageFlip, 0f);
    }

    public static void DrawOutlined(this SpriteBatch spriteBatch, Texture2D textureToDraw, Vector2 position, DrawInfo drawInfo, float outlineSize = 2f, bool onScreen = true) {
        for (int i = 0; i < 4; i++) {
            switch (i) {
                case 0:
                    spriteBatch.Draw(textureToDraw, position + -Vector2.UnitX * outlineSize, drawInfo, onScreen);
                    break;
                case 1:
                    spriteBatch.Draw(textureToDraw, position + Vector2.UnitX * outlineSize, drawInfo, onScreen);
                    break;
                case 2:
                    spriteBatch.Draw(textureToDraw, position + -Vector2.UnitY * outlineSize, drawInfo, onScreen);
                    break;
                case 3:
                    spriteBatch.Draw(textureToDraw, position + -Vector2.UnitY * outlineSize, drawInfo, onScreen);
                    break;
            }
        }
    }

    public static void Line(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, SpriteEffects effects = 0) => LineAngle(spriteBatch, start, Utils.AngleTo(start, end), Vector2.Distance(start, end), color, effects);
    public static void Line(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness, SpriteEffects effects = 0) => LineAngle(spriteBatch, start, Utils.AngleTo(start, end), Vector2.Distance(start, end), color, thickness, effects);

    public static void LineAngle(this SpriteBatch spriteBatch, Vector2 start, float angle, float length, Color color, SpriteEffects effects = 0) =>
        spriteBatch.Draw(ResourceManager.Pixel, start - Main.screenPosition, null, color, angle, Vector2.Zero, new Vector2(length, 1f), effects, 0f);

    public static void LineAngle(this SpriteBatch spriteBatch, Vector2 start, float angle, float length, Color color, float thickness, SpriteEffects effects = 0) =>
        spriteBatch.Draw(ResourceManager.Pixel, start - Main.screenPosition, null, color, angle, new Vector2(0.0f, 0.5f), new Vector2(length, thickness), effects, 0f);

    public static void With(this SpriteBatch spriteBatch,
                            BlendState blendState,
                            Action drawAction,
                            Effect effect = null,
                            SamplerState samplerState = null) {
        spriteBatch.With(blendState, false, drawAction, effect, samplerState);
    }

    public static void With(this SpriteBatch spriteBatch,
                            BlendState blendState,
                            bool isUI,
                            Action drawAction,
                            Effect effect = null,
                            SamplerState samplerState = null,
                            RasterizerState rasterizerState = null) {
        bool activeShader = effect != null;
        spriteBatch.End();
        spriteBatch.Begin(activeShader ? default : SpriteSortMode.Immediate,
                          blendState,
                          samplerState ?? Main.DefaultSamplerState,
                          activeShader ? default : DepthStencilState.None,
                          rasterizerState ?? (activeShader ? RasterizerState.CullNone : RasterizerState.CullCounterClockwise),
                          effect,
                          isUI ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix);
        drawAction();
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate,
                          BlendState.AlphaBlend, isUI ? SamplerState.AnisotropicClamp : Main.DefaultSamplerState,
                          DepthStencilState.None,
                          RasterizerState.CullCounterClockwise,
                          effect,
                          isUI ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix);
    }

    public static void BeginBlendState(this SpriteBatch spriteBatch, BlendState state, SamplerState samplerState = null, bool isUI = false, bool isUI2 = false) {
        spriteBatch.End();
        spriteBatch.Begin(isUI2 ? SpriteSortMode.Immediate : isUI ? SpriteSortMode.Deferred : SpriteSortMode.Immediate, state, samplerState ?? Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, isUI ? Main.UIScaleMatrix : Main.GameViewMatrix.ZoomMatrix);
    }

    public static void EndBlendState(this SpriteBatch spriteBatch, bool isUI = false) {
        spriteBatch.End();
        spriteBatch.Begin(isUI ? SpriteSortMode.Deferred : SpriteSortMode.Immediate, BlendState.AlphaBlend, isUI ? SamplerState.AnisotropicClamp : Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, isUI ? Main.UIScaleMatrix : Main.GameViewMatrix.ZoomMatrix);
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
