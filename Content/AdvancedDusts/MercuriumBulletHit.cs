using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.VisualEffects;
using RoA.Core;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;

namespace RoA.Content.AdvancedDusts;

sealed class MercuriumBulletParticle : AdvancedDust<MercuriumBulletParticle> {
    private float _scale;

    public override Texture2D Texture => ResourceManager.DefaultSparkle;

    protected override void SetDefaults() {
        TimeLeft = MaxTimeLeft = 25;
    }

    public override void Update(ref ParticleRendererSettings settings) {
        float t = (float)((MaxTimeLeft - TimeLeft) / (double)MaxTimeLeft * 60.0);
        float scale = Utils.GetLerpValue(0f, MaxTimeLeft / 3f, t, true) * Utils.GetLerpValue((float)(MaxTimeLeft - MaxTimeLeft / 5f), MaxTimeLeft / 2f, t, true);
        _scale = scale * Scale;

        if (--TimeLeft <= 0) {
            RestInPool();
        }
    }

    public override void Draw(ref ParticleRendererSettings settings, SpriteBatch spriteBatch) {
        Color color = DrawColor;
        Texture2D Texture = TextureAssets.Extra[98].Value;
        Vector2 origin = Texture.Size() / 2f;
        Vector2 position = Position - Main.screenPosition;
        SpriteEffects effects = SpriteEffects.None;
        Color shineColor = new(255, 200, 150);
        Color color3 = color * 1f * 0.5f;
        color3.A = (byte)(color3.A * (1.0 - (double)1f));
        Color color4 = color3 * 1f * 0.5f;
        color4.G = (byte)(color4.G * (double)1f);
        color4.B = (byte)(color4.R * (0.25 + (double)1f * 0.75));
        spriteBatch.Draw(Texture, position, null, color4, MathHelper.PiOver2 + Rotation, origin, _scale * 0.3f * new Vector2(1f, 2.5f), effects, 0f);
        spriteBatch.Draw(Texture, position, null, color3, Rotation, origin, _scale * 0.3f * new Vector2(1f, 2.5f), effects, 0f);
        spriteBatch.Draw(Texture, position, null, color3, MathHelper.PiOver2 + Rotation, origin, _scale * 0.3f * new Vector2(2f, 1f), effects, 0f);
        spriteBatch.Draw(Texture, position, null, color4, Rotation, origin, _scale * 0.3f * new Vector2(2f, 2f), effects, 0f);
    }
}
