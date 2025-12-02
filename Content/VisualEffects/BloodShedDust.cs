using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Graphics.Renderers;

namespace RoA.Content.VisualEffects;

sealed class BloodShedDust : VisualEffect<BloodShedDust> {
    private float _opacity;

    protected override void SetDefaults() {
        MaxTimeLeft = TimeLeft = 120;

        _opacity = 1f;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        Velocity *= 0.9f;
        Position += Velocity;
        Scale += 0.007f;
        Rotation += Main.rand.NextFloat(0.01f, 0.1f);
        _opacity -= 0.05f;
        if (_opacity < 0.01f || --TimeLeft <= 0) {
            RestInPool();
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        Color value = DrawColor * _opacity * 1f;
        value.A /= 2;
        float num = Utils.GetLerpValue(0f, 15f, TimeLeft, clamped: true) * Utils.GetLerpValue(60, 45f, TimeLeft, clamped: true);
        Vector2 vector = new Vector2(0.3f, 2f) * num * Scale;
        Vector2 vector2 = new Vector2(0.3f, 1f) * num * Scale;
        Vector2 origin = Texture.Size() / 2f;
        Color color = value * 0.5f;
        SpriteEffects effects = SpriteEffects.None;
        for (double i = -Math.PI; i <= Math.PI; i += Math.PI / 2.0) {
            Vector2 position = Position + origin - Main.screenPosition;
            spritebatch.Draw(Texture, position + vector - vector2, null, color * _opacity, Rotation, origin, Helper.Wave(Scale + 0.05f, Scale + 0.15f, 1f, 0f) * 0.95f, effects, 0f);
        }
    }
}
