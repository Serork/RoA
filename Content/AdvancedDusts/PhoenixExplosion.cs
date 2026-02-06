using Microsoft.Xna.Framework;

using RoA.Common.VisualEffects;
using RoA.Core.Utility;

using Terraria;
using Terraria.Graphics.Renderers;

namespace RoA.Content.AdvancedDusts;

sealed class PhoenixExplosion : AdvancedDust<PhoenixExplosion> {
    private Color _baseColor;

    protected override void SetDefaults() {
        TimeLeft = 21;

        SetFramedTexture(7, 0);

        _baseColor = Color.Lerp(new Color(249, 75, 7), new Color(255, 231, 66), Main.rand.NextFloat() * 0.25f);

        Scale = MathHelper.Lerp(1.125f, 1.25f, 0.5f);

        Rotation = MathHelper.TwoPi * Main.rand.NextFloat();
    }

    public override void Update(ref ParticleRendererSettings settings) {
        int frame = (int)(7 * (1f - (float)TimeLeft / MaxTimeLeft));

        if (--TimeLeft <= 0) {
            RestInPool();
        }

        Scale *= MathHelper.Lerp(0.925f, 0.95f, 0.5f);

        Frame = Texture.Frame(verticalFrames: 7, frameY: frame);

        float opacity = Utils.GetLerpValue(0, 4, TimeLeft, true) * Utils.GetLerpValue(MaxTimeLeft, MaxTimeLeft - 4, TimeLeft, true);
        DrawColor = _baseColor.MultiplyAlpha(0.25f) * 0.75f * 1f;
    }
}
