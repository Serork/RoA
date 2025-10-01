using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using Terraria;
using Terraria.Graphics.Renderers;

namespace RoA.Content.VisualEffects;

sealed class WardenDust : VisualEffect<WardenDust> {
    protected override void SetDefaults() {
        TimeLeft = 100;

        SetFramedTexture(3, Main.rand.Next(1));
        AI0 = 0f;

        DontEmitLight = true;
    }

    protected override float ScaleDecreaseModifier() => 0.75f;

    public override void Update(ref ParticleRendererSettings settings) {
        if (Velocity.IsWithinRange(1f)) {
            base.Update(ref settings);
        }
        else {
            Velocity *= 0.9f;
            Position += Velocity;
        }
        AI0 += 0.02f;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        int extra = 3;
        Color color2 = Color.White;
        float fadeOutProgress = CustomData == null ? 1f : (float)CustomData!;
        float waveMin = MathHelper.Lerp(0.75f, 1f, 1f - fadeOutProgress), waveMax = MathHelper.Lerp(1.25f, 1f, 1f - fadeOutProgress);
        float wave = Helper.Wave(AI0, waveMin, waveMax, 3f, 0f) * fadeOutProgress;
        float opacity = wave * fadeOutProgress;
        color2 *= opacity;
        spritebatch.Draw(Texture, Position - Main.screenPosition, Frame, Lighting.GetColor(Position.ToTileCoordinates()) * Utils.Remap(fadeOutProgress, 0f, 1f, 0.5f, 1f, true), Rotation, Origin, Scale, SpriteEffects.None, 0f);
        for (int i = 0; i < extra; i++) {
            spritebatch.DrawWithSnapshot(() => {
                spritebatch.Draw(Texture, Position - Main.screenPosition, Frame, color2 * 0.625f * fadeOutProgress, Rotation, Origin, Scale, SpriteEffects.None, 0f);
            }, blendState: BlendState.Additive);
        }
    }
}
