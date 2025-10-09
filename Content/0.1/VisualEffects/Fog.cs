using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.VisualEffects;
using RoA.Core.Utility;

using Terraria;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Content.VisualEffects;

sealed class Fog : VisualEffect<Fog> {
    private float _brightness;

    public float FadeIn { get; private set; }
    public int Alpha { get; private set; }

    public override Texture2D Texture => (Texture2D)ModContent.Request<Texture2D>(ModContent.GetModDust(ModContent.DustType<Dusts.Backwoods.Fog>()).Texture);

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spritebatch) {
        Color lightColor = Lighting.GetColor((int)(Position.X / 16f), (int)(Position.Y / 16f));
        float brightness = (lightColor.R / 255f + lightColor.G / 255f + lightColor.B / 255f) / 3f;
        float brightness2 = MathHelper.Clamp((brightness - 0.6f) * 5f, 0f, 1f);
        _brightness = MathHelper.Lerp(_brightness, 1f - brightness2, 0.15f);
        spritebatch.Draw(Texture, Position - Main.screenPosition, Frame, lightColor * _brightness * (1f - Alpha / 255f), Rotation, Origin, Scale, SpriteEffects.None, 0f);
    }

    protected override void SetDefaults() {
        Frame = new Rectangle(0, 12 * Main.rand.Next(4), 42, 12);
        Alpha = 255;
        FadeIn = 3f;

        DontEmitLight = true;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        TimeLeft = 2;

        FadeIn -= TimeSystem.LogicDeltaTime;
        if (FadeIn <= 0) {
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

        if (WorldGenHelper.SolidTile2(point.X, point.Y) || (!WorldGenHelper.SolidTile2(point.X, point.Y + 1) && !WorldGenHelper.SolidTile2(point.X, point.Y + 2)))
            flag = true;

        if (FadeIn <= 0.3f)
            flag = true;

        Helper.ApplyWindPhysics(Position, ref Velocity);
        Velocity.X *= 0.75f;
        Velocity.Y = 0f;
        if (!flag) {
            if (Alpha > 200) {
                Alpha--;
            }
        }
        else {
            Alpha++;
            if (Alpha >= 255) {
                RestInPool();
                return;
            }
        }

        Position += Velocity;
    }
}
