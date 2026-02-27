using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Content.Dusts;
using RoA.Core.Utility;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Content.AdvancedDusts;

sealed class FilamentYarnDust : AdvancedDust<FilamentYarnDust> {
    public Vector2 CorePosition;

    protected override string TexturePath => DustLoader.GetDust(ModContent.DustType<FilamentDust>()).Texture;

    protected override void SetDefaults() {
        SetFramedTexture(3);

        CorePosition = Vector2.Zero;

        TimeLeft = 20;

        Scale *= 0.7f;

        Velocity *= 0.5f;

        DrawColor = new Color(Color.White.R, Color.White.G, Color.White.B, 25);
    }

    public override void Update(ref ParticleRendererSettings settings) {
        if (Scale < 0.75f) {
            Velocity = Vector2.Lerp(Velocity, Position.DirectionTo(CorePosition), 0.025f);
        }

        Position += Velocity.RotatedBy(MathHelper.PiOver2) * 1f * Velocity.X.GetDirection();

        Position += Velocity;

        Rotation += Velocity.X * 0.5f;

        Scale -= 0.025f;

        if (Scale <= 0.1f || float.IsNaN(Scale)) {
            RestInPool();
        }

        float num56 = Scale * 1.4f;
        if (num56 > 0.6f)
            num56 = 0.6f;

        Lighting.AddLight(Position, new Color(215, 230, 10).ToVector3() * 1f * num56);
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        Draw_Inner(spritebatch);
    }
}
