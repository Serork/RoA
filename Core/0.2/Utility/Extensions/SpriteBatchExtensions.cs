using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using RoA.Common.Cache;
using RoA.Core.Graphics.Data;

using System;

using Terraria;

namespace RoA.Core.Utility;

static partial class SpriteBatchExtensions {
    public static void Draw(this SpriteBatch spriteBatch, Texture2D textureToDraw, Vector2 position, DrawInfo drawInfo, bool onScreen = true) {
        spriteBatch.Draw(textureToDraw, position - (onScreen ? Main.screenPosition : Vector2.Zero), drawInfo.Clip, drawInfo.Color, drawInfo.Rotation, drawInfo.Origin, drawInfo.Scale, drawInfo.ImageFlip, 0f);
    }

    public static void DrawWithSnapshot(this SpriteBatch spriteBatch, Texture2D textureToDraw, Vector2 position, DrawInfo drawInfo, bool onScreen = true, SpriteSortMode? sortMode = null, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect? effect = null, Matrix? transformationMatrix = null) {
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
        sortMode ??= snapshot.sortMode;
        blendState ??= snapshot.blendState;
        samplerState ??= snapshot.samplerState;
        depthStencilState ??= snapshot.depthStencilState;
        rasterizerState ??= snapshot.rasterizerState;
        effect ??= snapshot.effect;
        transformationMatrix ??= snapshot.transformationMatrix;
        spriteBatch.Begin(new SpriteBatchSnapshot(sortMode.Value, blendState, samplerState, depthStencilState, rasterizerState, effect, transformationMatrix), true);
        Draw(spriteBatch, textureToDraw, position, drawInfo, onScreen);
        spriteBatch.Begin(in snapshot, true);
    }

    public static void DrawWithSnapshot(this SpriteBatch spriteBatch, Action draw, SpriteSortMode? sortMode = null, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect? effect = null, Matrix? transformationMatrix = null) {
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
        sortMode ??= snapshot.sortMode;
        blendState ??= snapshot.blendState;
        samplerState ??= snapshot.samplerState;
        depthStencilState ??= snapshot.depthStencilState;
        rasterizerState ??= snapshot.rasterizerState;
        effect ??= snapshot.effect;
        transformationMatrix ??= snapshot.transformationMatrix;
        spriteBatch.Begin(new SpriteBatchSnapshot(sortMode.Value, blendState, samplerState, depthStencilState, rasterizerState, effect, transformationMatrix), true);
        draw();
        spriteBatch.Begin(in snapshot, true);
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
}
