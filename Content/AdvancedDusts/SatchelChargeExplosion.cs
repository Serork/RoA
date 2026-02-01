using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Core.Utility;

using Terraria;
using Terraria.Graphics.Renderers;

namespace RoA.Content.AdvancedDusts;

sealed class SatchelChargeExplosion : AdvancedDust<SatchelChargeExplosion> {
    private Color _baseColor;

    protected override void SetDefaults() {
        TimeLeft = 18;

        SetFramedTexture(6, 7);

        DontEmitLight = true;

        AI0 = Main.rand.NextFloat(10f);

        _baseColor = Color.Lerp(new Color(255, 255, 86), new Color(230, 70, 70), Main.rand.NextFloat() * 0.5f);

        Direction = Main.rand.NextBool().ToSpriteEffects();
    }

    public override void Update(ref ParticleRendererSettings settings) {
        int frame = (int)(6 * (1f - (float)TimeLeft / MaxTimeLeft));
        frame += 1;
        if (AI0-- > 0) {
            frame = 7;
        }
        else {
            if (--TimeLeft <= 0) {
                RestInPool();
            }
        }

        Velocity *= 0.8f;

        float opacity = Utils.GetLerpValue(0, 20, TimeLeft, true) * Utils.GetLerpValue(MaxTimeLeft, MaxTimeLeft - 20, TimeLeft, true);
        DrawColor = _baseColor.MultiplyAlpha(0.375f) * 0.625f * opacity; 

        Frame = Texture.Frame(verticalFrames: 6, frameY: frame);
    }
}
