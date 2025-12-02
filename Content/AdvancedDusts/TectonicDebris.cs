using Microsoft.Xna.Framework;

using RoA.Common.VisualEffects;

using System;

using Terraria;
using Terraria.Graphics.Renderers;

namespace RoA.Content.AdvancedDusts;

sealed class TectonicDebris : AdvancedDust<TectonicDebris> {
    protected override void SetDefaults() {
        MaxTimeLeft = TimeLeft = 120;

        SetFramedTexture(3);
    }

    public override void Update(ref ParticleRendererSettings settings) {
        float length = Velocity.Length();
        Rotation += length * 0.0314f;

        DrawColor = Lighting.GetColor(Position.ToTileCoordinates());

        if (TimeLeft <= 30) {
            Scale = Utils.GetLerpValue(0, 30, TimeLeft, true);
        }
        else {
            Scale = MathHelper.Lerp(Scale, 1f, 0.015f);
        }

        if (AI0 == 0f) {
            Velocity *= 0.95f;
            if (Velocity.Length() < 0.1f) {
                AI0 = 1f;
                Velocity *= 0.5f;
            }
        }
        else {
            Velocity.X *= 0.5f;
            Velocity.Y += 0.1f;
            Velocity.Y = Math.Min(10f, Velocity.Y);
        }

        Position += Velocity;

        if (Scale <= 0.1f || float.IsNaN(Scale) || --TimeLeft <= 0) {
            RestInPool();
        }
    }
}
