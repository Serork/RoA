using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;

namespace RoA.Core.Utility;

public static class SpriteBatchExtensions {
    public static void With(this SpriteBatch spriteBatch,
                            BlendState blendState,
                            Action drawAction,
                            SamplerState samplerState = null) {
        spriteBatch.With(blendState, false, drawAction, samplerState);
    }

    public static void With(this SpriteBatch spriteBatch,
                            BlendState blendState,
                            bool isUI,
                            Action drawAction,
                            SamplerState samplerState = null) {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate,
                          blendState,
                          samplerState ?? Main.DefaultSamplerState,
                          DepthStencilState.None,
                          RasterizerState.CullCounterClockwise,
                          null,
                          isUI ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix);
        drawAction();
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate,
                          BlendState.AlphaBlend, isUI ? SamplerState.AnisotropicClamp : Main.DefaultSamplerState,
                          DepthStencilState.None,
                          RasterizerState.CullCounterClockwise,
                          null,
                          isUI ? Main.UIScaleMatrix : Main.GameViewMatrix.TransformationMatrix);
    }
}
