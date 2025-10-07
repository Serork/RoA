using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Graphics.Renderers;

namespace RoA.Content.VisualEffects;

sealed class SunSigil : VisualEffect<SunSigil> {
    public float AI1;

    public override Texture2D Texture => ResourceManager.DefaultSparkle;

    protected override void SetDefaults() {
        TimeLeft = 50;

        Color result = new Color(DrawColor.ToVector3());
        result.A = 25;
        DrawColor = result;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        Position += new Vector2(0f, AI0).RotatedBy(Velocity.ToRotation()) * 0.5f;
        Position += Velocity;
        if (AI1 != 1f) AI0 -= 1f;
        else AI0 += 1;
        if (AI0 <= -6) AI1 = 1f;
        if (AI0 >= 6) AI1 = 0f;

        Scale *= 0.975f;
        if (Scale <= 0.1f || --TimeLeft <= 0) {
            RestInPool();
        }

        Rotation = Velocity.X * 0.5f;

        Lighting.AddLight(Position, DrawColor.ToVector3() * 0.5f);
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        Main.spriteBatch.DrawWithSnapshot(() => {
            for (int i = 0; i < 1; i++) {
                float num2 = Math.Abs(Velocity.X) + Math.Abs(Velocity.Y);
                num2 *= 10f;
                if (num2 > 10f)
                    num2 = 10f;
                for (int k = 0; (float)k < num2; k++) {
                    Vector2 velocity2 = Velocity;
                    Vector2 vector2 = Position - velocity2 * k;
                    float scale3 = Scale * ((float)k / 10f);
                    Color color2 = DrawColor * Utils.GetLerpValue(0f, 10f, TimeLeft, true);
                    Main.spriteBatch.Draw(Texture, vector2 - Main.screenPosition, null, color2, Rotation, texture.Size() / 2f, scale3, SpriteEffects.None, 0f);
                    Main.spriteBatch.Draw(Texture, vector2 - Main.screenPosition, null, color2, Rotation + MathHelper.PiOver2, texture.Size() / 2f, scale3, SpriteEffects.None, 0f);
                }
            }
        }, blendState: BlendState.Additive);
    }
}
