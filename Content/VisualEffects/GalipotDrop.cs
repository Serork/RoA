using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Core;
using RoA.Core.Data;
using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Content.VisualEffects;

sealed class GalipotDrop : VisualEffect<GalipotDrop> {
    public Projectile Projectile;

    private float _opacity;

    protected override void SetDefaults() {
        TimeLeft = MaxTimeLeft = 2;
        _opacity = 1f;
        //Color = Color.Lerp(new Color(201, 81, 0), new Color(126, 33, 0), 0.5f);
        DrawColor = new Color(255, 190, 44);
    }

    public override void Update(ref ParticleRendererSettings settings) {
        if (!(Projectile == null || !Projectile.active)) {
            TimeLeft = Projectile.timeLeft;
        }
        if (--TimeLeft <= 0 || _opacity <= 0f || Scale <= 0.01f) {
            RestInPool();
            return;
        }
        bool flag = Collision.WetCollision(Position, 0, 0);
        if (!flag) {
            if ((Scale < 1f || Projectile.timeLeft < 100) && _opacity > 0f) {
                _opacity -= 0.065f + Main.rand.NextFloatRange(0.025f);
            }
        }
        else {
            Scale -= 0.05f;
        }
        if (Collision.SolidCollision(Position, 0, 0) || flag) {
            if (!flag) {
                Velocity = Vector2.UnitY;
                Position.Y += 0.05f * Main.rand.NextFloat();
            }
            else {
                Velocity = Vector2.Zero;
            }
            if (Scale < 1f) {
                RestInPool();
            }
        }
        else {
            Velocity *= 0.98f;
            Velocity += new Vector2(Main.windSpeedCurrent * 0.1f, 0.21f * Scale * 0.1f);
            Position += Velocity;
            Scale *= 0.98f;
        }
        //if (_opacity < 1f) {
        //    _opacity += 0.1f;
        //}
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        DrawSelf(spritebatch);
    }

    private void DrawSelf(SpriteBatch spritebatch) {
        spritebatch.End();
        spritebatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        float opacity = _opacity/* * 1.5f*/;
        Vector2 toCorner = new Vector2(0f, Scale).RotatedBy(Rotation);
        Color color2 = new Color(201, 85, 8) * opacity * 1f;
        Color color1 = new Color(126, 33, 0) * opacity * 1f;
        List<Vertex2D> bars = [
            new Vertex2D(Position - Main.screenPosition + Velocity + toCorner, Color.Lerp(color1, color2, Scale).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(0f, 0f, 0f)),
        new Vertex2D(Position - Main.screenPosition + toCorner.RotatedBy(MathHelper.Pi * 0.5f), Color.Lerp(color1, color2, Scale).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)),  new Vector3(0f, 1f, 0f)),
        new Vertex2D(Position - Main.screenPosition + toCorner.RotatedBy(MathHelper.Pi * 1.5f), Color.Lerp(color1, color2, Scale).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(1f, 0f, 0f)),
        new Vertex2D(Position - Main.screenPosition - Velocity * AI0 + toCorner.RotatedBy(MathHelper.Pi), Color.Lerp(color1, color2, Scale).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(1f, 1f, 0f))
        ];
        Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>(ResourceManager.Textures + "Lightball3").Value;
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
        color1 = new Color(255, 241, 44) * opacity * 0.15f;
        color2 = Color.White * opacity * 0.15f;
        bars = [
            new Vertex2D(Position - Main.screenPosition + Velocity + toCorner, Color.Lerp(color1, color2, Scale).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(0f, 0f, 0f)),
        new Vertex2D(Position - Main.screenPosition + toCorner.RotatedBy(MathHelper.Pi * 0.5f), Color.Lerp(color1, color2, Scale).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)),  new Vector3(0f, 1f, 0f)),
        new Vertex2D(Position - Main.screenPosition + toCorner.RotatedBy(MathHelper.Pi * 1.5f), Color.Lerp(color1, color2, Scale).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(1f, 0f, 0f)),
        new Vertex2D(Position - Main.screenPosition - Velocity * AI0 + toCorner.RotatedBy(MathHelper.Pi), Color.Lerp(color1, color2, Scale).MultiplyRGBA(Lighting.GetColor((int)Position.X / 16, (int)Position.Y / 16)), new Vector3(1f, 1f, 0f))
        ];
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);
        Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        spritebatch.EndBlendState();
    }
}
