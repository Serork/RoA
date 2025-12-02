using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable enable
namespace RoA.Common.Cache;

/// <summary>Contains the data for a <see cref="SpriteBatch.Begin(SpriteSortMode, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect, Matrix)"/> call.</summary>
struct SpriteBatchSnapshot {
    private static readonly Matrix identityMatrix = Matrix.Identity;
    public SpriteSortMode sortMode;
    public BlendState blendState;
    public SamplerState samplerState;
    public DepthStencilState depthStencilState;
    public RasterizerState rasterizerState;
    public Effect? effect;
    public Matrix transformationMatrix;

    public SpriteBatchSnapshot(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState? blendState = null, SamplerState? samplerState = null, DepthStencilState? depthStencilState = null, RasterizerState? rasterizerState = null, Effect? effect = null, Matrix? transformationMatrix = null) {
        this.sortMode = sortMode;
        this.blendState = blendState ?? BlendState.AlphaBlend;
        this.samplerState = samplerState ?? SamplerState.LinearClamp;
        this.depthStencilState = depthStencilState ?? DepthStencilState.None;
        this.rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
        this.effect = effect;
        this.transformationMatrix = transformationMatrix ?? identityMatrix;
    }

    /// <summary>Calls <seealso cref="SpriteBatch.Begin(SpriteSortMode, BlendState, SamplerState, DepthStencilState, RasterizerState, Effect, Matrix)"/> with the data on this <seealso cref="SpriteBatchSnapshot"/> instance.</summary>
    /// <param name="spriteBatch">The spritebatch to begin.</param>
    public readonly void Begin(SpriteBatch spriteBatch) {
        spriteBatch.Begin(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformationMatrix);
    }

    public static SpriteBatchSnapshot Capture(SpriteBatch spriteBatch) {
        SpriteSortMode sortMode = (SpriteSortMode)SpriteBatchSnapshotCache.SortModeField.GetValue(spriteBatch);
        BlendState blendState = (BlendState)SpriteBatchSnapshotCache.BlendStateField.GetValue(spriteBatch);
        SamplerState samplerState = (SamplerState)SpriteBatchSnapshotCache.SamplerStateField.GetValue(spriteBatch);
        DepthStencilState depthStencilState = (DepthStencilState)SpriteBatchSnapshotCache.DepthStencilStateField.GetValue(spriteBatch);
        RasterizerState rasterizerState = (RasterizerState)SpriteBatchSnapshotCache.RasterizerStateField.GetValue(spriteBatch);
        Effect effect = (Effect)SpriteBatchSnapshotCache.EffectField.GetValue(spriteBatch);
        Matrix transformMatrix = (Matrix)SpriteBatchSnapshotCache.TransformMatrixField.GetValue(spriteBatch);

        return new SpriteBatchSnapshot(sortMode, blendState, samplerState, depthStencilState, rasterizerState, effect, transformMatrix);
    }

    //void Revalidate()
    //{
    //    blendState ??= BlendState.AlphaBlend;
    //    samplerState ??= SamplerState.LinearClamp;
    //    depthStencilState ??= DepthStencilState.None;
    //    rasterizerState ??= RasterizerState.CullCounterClockwise;
    //}
}
