using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.VisualEffects;
using RoA.Common.WorldEvents;
using RoA.Core.Utility;

using Terraria;
using Terraria.Graphics.Renderers;

namespace RoA.Content.VisualEffects;

sealed class Fog2 : VisualEffect<Fog2> {
    private float _brightness;

    public float FadeIn { get; private set; }
    public float FadeIn2 { get; private set; }

    public int Alpha { get; private set; }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        Color lightColor = Lighting.GetColor((int)(Position.X / 16f), (int)(Position.Y / 16f));
        float brightness = (lightColor.R / 255f + lightColor.G / 255f + lightColor.B / 255f) / 3f;
        float brightness2 = MathHelper.Clamp((brightness - 0.6f) * 5f, 0f, 1f);
        _brightness = MathHelper.Lerp(_brightness, 1f - brightness2, 0.15f);
        spritebatch.Draw(Texture, Position - Main.screenPosition, Frame, lightColor * MathUtils.Clamp01(_brightness * (1f - Alpha / 255f)), Rotation, Origin, Scale, SpriteEffects.None, 0f);
    }

    protected override void SetDefaults() {
        _brightness = 0f;

        Frame = new Rectangle(0, 12 * Main.rand.Next(4), 42, 12);
        Alpha = 255;
        FadeIn = FadeIn2 = Main.rand.NextFloat(10f, 20f);

        DontEmitLight = true;

        if (!Helper.OnSurface(Position, ref Velocity)) {
            Velocity.X *= 0.15f;
        }

        SetFramedTexture(2);
    }

    public override void Update(ref ParticleRendererSettings settings) {
        TimeLeft = 2;

        FadeIn -= TimeSystem.LogicDeltaTime;
        if (!BackwoodsFogHandler.IsFogActive) {
            FadeIn -= TimeSystem.LogicDeltaTime;
        }
        if (FadeIn <= 0 && Alpha >= 255) {
            RestInPool();
            return;
        }

        bool flag = false;
        Point point = (Position + new Vector2(15f, 0f)).ToTileCoordinates();
        Tile tile = Main.tile[point.X, point.Y];
        Tile tile2 = Main.tile[point.X, point.Y + 1];
        Tile tile3 = Main.tile[point.X, point.Y + 2];
        if (tile == null || tile2 == null || tile3 == null) {
            RestInPool();
            return;
        }

        if (!(WorldGenHelper.SolidTile2(point.X, point.Y) || (!WorldGenHelper.SolidTile2(point.X, point.Y + 1) && !WorldGenHelper.SolidTile2(point.X, point.Y + 2))))
            flag = true;

        if (FadeIn <= 0.3f)
            flag = true;

        if (FadeIn < 5f && Velocity.Length() < 1f) {
            flag = true;
        }

        Helper.ApplyWindPhysics(Position, ref Velocity);
        Velocity.X *= !Helper.OnSurface(Position, ref Velocity) ? 0.99f : (0.9f + (FadeIn2 * 0.0025f));
        Velocity.Y = 0f;
        if (!flag) {
            if (Alpha > 200) {
                Alpha = (int)Utils.Remap(FadeIn, FadeIn2, FadeIn2 * 0.8f, 255, 200);
            }
        }
        else {
            Alpha = (int)Utils.Remap(FadeIn, 0f, FadeIn2 * 0.2f, 255, 200);
            if (Alpha >= 255) {
                RestInPool();
                return;
            }
        }

        Position = Vector2.Lerp(Position, Position + Velocity, 0.5f);
    }
}
