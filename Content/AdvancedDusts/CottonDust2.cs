using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;

using Terraria;
using Terraria.Graphics.Renderers;

namespace RoA.Content.AdvancedDusts;

sealed class CottonDust2 : AdvancedDust<CottonDust2> {
    public Vector2 CorePosition;

    protected override void SetDefaults() {
        SetFramedTexture(3);

        CorePosition = Vector2.Zero;

        TimeLeft = 20;

        DrawColor = Lighting.GetColor(Position.ToTileCoordinates());
    }

    public override void Update(ref ParticleRendererSettings settings) {
        DrawColor = Lighting.GetColor(Position.ToTileCoordinates());

        Velocity = Vector2.Lerp(Velocity, Position.DirectionTo(CorePosition), 0.2f);

        Position += Velocity;

        if (TimeLeft < 10) {
            DrawColor *= 0.9f;
        }

        Rotation += Velocity.X * 0.1f;

        if (Scale <= 0.1f || float.IsNaN(Scale) || --TimeLeft <= 0) {
            RestInPool();
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        Draw_Inner(spritebatch, colorModifier: Utils.GetLerpValue(0, 10, TimeLeft, true));
    }
}
