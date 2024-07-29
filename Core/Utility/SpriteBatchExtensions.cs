using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;

namespace RoA.Core.Utility;

static class SpriteBatchExtensions {
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
                            SamplerState samplerState = null) {
        bool activeShader = effect != null;
        spriteBatch.End();
        spriteBatch.Begin(activeShader ? default : SpriteSortMode.Immediate,
                          blendState,
                          samplerState ?? Main.DefaultSamplerState,
                          activeShader ? default : DepthStencilState.None,
                          activeShader ? RasterizerState.CullNone : RasterizerState.CullCounterClockwise,
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
        spriteBatch.Begin(isUI2 ? SpriteSortMode.Immediate : isUI ? SpriteSortMode.Deferred : SpriteSortMode.Immediate, state, samplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, isUI ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix);
    }

    public static void EndBlendState(this SpriteBatch spriteBatch, bool isUI = false) {
        spriteBatch.End();
        spriteBatch.Begin(isUI ? SpriteSortMode.Deferred : SpriteSortMode.Immediate, BlendState.AlphaBlend, isUI ? SamplerState.AnisotropicClamp : Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, isUI ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix);
    }

    public static void BeginWorld(this SpriteBatch spriteBatch, bool shader = false, Matrix? overrideMatrix = null) {
        var matrix = overrideMatrix ?? Main.Transform;
        if (!shader) {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, matrix);
        }
        else {
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default, Main.Rasterizer, null, matrix);
        }
    }
}
