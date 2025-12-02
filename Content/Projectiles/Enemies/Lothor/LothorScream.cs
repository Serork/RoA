using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Cache;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class LothorScream : ModProjectile {
    public override void SetDefaults() {
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;

        Projectile.timeLeft = 120;
        Projectile.penetrate = -1;

        Projectile.aiStyle = -1;

        Projectile.height = Projectile.width = 490;
        Projectile.scale = 0f;

        Projectile.Opacity = 0.8f;
    }

    public override void AI() {
        Projectile.frame = (int)Projectile.ai[0];

        Projectile.scale += 0.25f;
        Projectile.Opacity -= 0.05f;

        if (Projectile.scale >= 5f) {
            Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        SpriteBatchSnapshot snapshot = SpriteBatchSnapshot.Capture(spriteBatch);
        spriteBatch.Begin(snapshot with { blendState = BlendState.AlphaBlend, samplerState = SamplerState.PointClamp }, true);
        Texture2D texture = Projectile.GetTexture();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Color color = Lighting.GetColor((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16);
        spriteBatch.Draw(texture,
                         position,
                         new Rectangle?(new Rectangle(0, 490 * Projectile.frame, 490, 490)),
                         (Projectile.frame == 1 ? lightColor : color.MultiplyRGB(Color.Black)) * Projectile.Opacity,
                         Projectile.rotation,
                         new Vector2(245f, 245f),
                         Projectile.scale,
                         SpriteEffects.None,
                         0);
        spriteBatch.Begin(snapshot, true);

        return false;
    }
}
