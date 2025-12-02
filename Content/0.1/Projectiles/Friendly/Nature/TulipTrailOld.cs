using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class TulipTrailOld : NatureProjectile {
    private float _drawScale = 0.4f;
    private float _drawScaleMax;
    private bool _initialize = false;

    public override void SetStaticDefaults() {
        Main.projFrames[Type] = 3;

        ProjectileID.Sets.TrailCacheLength[Type] = 3;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    protected override void SafeSetDefaults() {
        int width = 20; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.penetrate = 1;
        Projectile.aiStyle = -1;

        Projectile.friendly = true;
        Projectile.timeLeft = 100;
        Projectile.alpha = 55;

        Projectile.appliesImmunityTimeOnSingleHits = true;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;
    }

    //public override bool? CanDamage() => Projectile.Opacity >= 0.45f;

    public override void AI() {
        if (!_initialize) {
            Projectile.rotation = Main.rand.Next(360);
            _drawScaleMax = 0.2f + Main.rand.NextFloat(1f, 1.4f) / Projectile.ai[1] * 8f;
            Projectile.frame = (int)Projectile.ai[0];
            _initialize = true;
        }
        if (_drawScale < _drawScaleMax) _drawScale += 0.05f;
        Projectile.velocity = Vector2.Zero;
        if (Projectile.timeLeft <= 50) Projectile.alpha += 6;
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D projectileTexture = Projectile.GetTexture();
        int frameHeight = projectileTexture.Height / Main.projFrames[Type];
        Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, projectileTexture.Width, frameHeight);
        Vector2 drawOrigin = new Vector2(projectileTexture.Width * 0.5f, projectileTexture.Height / Main.projFrames[Projectile.type] * 0.5f);
        Vector2 drawPos = Projectile.oldPos[0] - Main.screenPosition + new Vector2(DrawOffsetX + 5, Projectile.gfxOffY + 5) + Projectile.Size / 4f;
        Color color = Projectile.GetAlpha(lightColor);
        spriteBatch.Draw(projectileTexture, drawPos, frameRect, color, Projectile.rotation, drawOrigin, _drawScale, SpriteEffects.None, 0f);

        return false;
    }
}