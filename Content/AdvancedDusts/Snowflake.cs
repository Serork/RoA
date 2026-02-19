using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Graphics.Renderers;

namespace RoA.Content.AdvancedDusts;

sealed class Snowflake : AdvancedDust<Snowflake> {
    private byte _seed;

    public Vector2 CorePosition;

    protected override void SetDefaults() {
        _seed = (byte)Main.rand.Next(100);

        TimeLeft = 90;

        CorePosition = Vector2.Zero;

        SetFramedTexture(3);

        DrawColor = Lighting.GetColor(Position.ToTileCoordinates());
    }

    public override void Update(ref ParticleRendererSettings settings) {
        DrawColor = Lighting.GetColor(Position.ToTileCoordinates());

        if (CorePosition != Vector2.Zero) {
            if (TimeLeft <= 30) {
                Scale *= Utils.GetLerpValue(0, 30, TimeLeft, true);
            }

            Position += Vector2.One.RotatedBy(Velocity.ToRotation()) * Helper.Wave(-1f, 1f, 5f, _seed) * Velocity.Length();
            Position += Vector2.One.RotatedBy(Velocity.TurnRight().ToRotation()) * Velocity.Length();
            Rotation += -Position.DirectionTo(CorePosition).X.GetDirection() * 0.1f;
            Position += Position.DirectionTo(CorePosition);
            if (Position.Distance(CorePosition) < 5f && TimeLeft > 30) {
                TimeLeft = 30;
            }
            Velocity *= 0.95f;
        }
        else {
            Rotation += Velocity.X * 0.0314f;

            Velocity *= 0.9f;

            if (Velocity.Length() < 1f) {
                AI0 = 1f;
            }
            if (AI0 == 1f) {
                TimeLeft--;
                Velocity.Y += 0.1f;
                Velocity.Y = Math.Min(10f, Velocity.Y);
            }

            float velocityX = Helper.Wave(-1f, 1f, 5f, _seed) * Velocity.Length();
            Position.X += velocityX;

            Rotation += velocityX * 0.025f;
        }

        Position += Velocity;

        if (Scale <= 0.1f || float.IsNaN(Scale) || --TimeLeft <= 0) {
            RestInPool();
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        Draw_Inner(spritebatch, colorModifier: (CorePosition == Vector2.Zero ? Utils.GetLerpValue(0, 30, TimeLeft, true) : 1f) * Utils.GetLerpValue(MaxTimeLeft, MaxTimeLeft - 10, TimeLeft, true));
    }
}
