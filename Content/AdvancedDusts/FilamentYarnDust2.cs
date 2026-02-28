using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using Terraria;
using Terraria.Graphics.Renderers;

namespace RoA.Content.AdvancedDusts;

sealed class FilamentYarnDust2 : AdvancedDust<FilamentYarnDust2> {
    protected override string TexturePath => ResourceManager.EmptyTexture;

    protected override void SetDefaults() {
        TimeLeft = 40;

        DrawColor = Color.Lerp(Color.Yellow, new Color(196, 182, 70), 0.5f);
        DrawColor.A = 0;

        Scale = Main.rand.NextFloat() * 0.4f + 0.2f;
        Scale *= 1.25f;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        if (CustomData != null && CustomData is Player) {
            Player player9 = (Player)CustomData;
            Position += player9.position - player9.oldPosition;
        }

        Velocity *= 0.95f;
        Scale *= 0.975f;

        if (Velocity.Length() > 0.1f) {
            Rotation = Velocity.ToRotation() - MathHelper.PiOver2;
        }

        Lighting.AddLight(Position, Color.Lerp(new Color(196, 186, 70), new Color(127, 153, 22), 0.5f).ToVector3() * 0.75f * Scale);

        if (--TimeLeft <= 0) {
            RestInPool();

            return;
        }

        Position += Velocity;
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        Texture2D sparkleTexture = ResourceManager.DefaultSparkle;
        Rectangle clip = sparkleTexture.Bounds;
        Vector2 origin = clip.Centered();
        Color color = DrawColor * Utils.GetLerpValue(0, 10, TimeLeft, true) /** Utils.GetLerpValue(MaxTimeLeft, MaxTimeLeft - 10, TimeLeft, true)*/;
        float rotation = Rotation;
        Vector2 scale = new Vector2(Scale) * Utils.GetLerpValue(0, 10, TimeLeft, true);
        DrawInfo drawInfo = new() {
            Clip = clip,
            Origin = origin,
            Color = color,
            Rotation = rotation,
            Scale = scale
        };   
        Vector2 position = Position;
        spritebatch.Draw(sparkleTexture, position, drawInfo);
        spritebatch.Draw(sparkleTexture, position, drawInfo.WithScale(0.5f) with {
            Color = Color.White.MultiplyAlpha(0)
        });
    }
}
