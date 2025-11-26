using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

abstract class SkeletonBodyPart : ModProjectile {
    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults() {
        int width = 24; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false;

        Projectile.DamageType = DamageClass.Default;
    }

    public override void AI() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 3;

        Player player = Main.player[Projectile.owner];
        Vector2 targetPos = player.Center;
        float speed = 10f;
        float speedFactor = 0.7f;
        Vector2 projCenter = new Vector2(Projectile.position.X + (float)Projectile.width * 0.5f, Projectile.position.Y + (float)Projectile.height * 0.5f);
        float posX = targetPos.X - projCenter.X;
        float posY = targetPos.Y - projCenter.Y;
        float distance = (float)Math.Sqrt((posX * posX + posY * posY));
        if (distance > 3000f) Projectile.Kill();

        distance = speed / distance;
        posX *= distance;
        posY *= distance;

        if (Projectile.velocity.X < posX) {
            Projectile.velocity.X = Projectile.velocity.X + speedFactor;
            if (Projectile.velocity.X < 0f && posX > 0f)
                Projectile.velocity.X = Projectile.velocity.X + speedFactor;
        }
        else if (Projectile.velocity.X > posX) {
            Projectile.velocity.X = Projectile.velocity.X - speedFactor;
            if (Projectile.velocity.X > 0f && posX < 0f)
                Projectile.velocity.X = Projectile.velocity.X - speedFactor;
        }
        if (Projectile.velocity.Y < posY) {
            Projectile.velocity.Y = Projectile.velocity.Y + speedFactor;
            if (Projectile.velocity.Y < 0f && posY > 0f)
                Projectile.velocity.Y = Projectile.velocity.Y + speedFactor;
        }
        else if (Projectile.velocity.Y > posY) {
            Projectile.velocity.Y = Projectile.velocity.Y - speedFactor;
            if (Projectile.velocity.Y > 0f && posY < 0f)
                Projectile.velocity.Y = Projectile.velocity.Y - speedFactor;
        }
        if (player.Hitbox.Intersects(Projectile.Hitbox) || !player.active)
            Projectile.Kill();

        Projectile.rotation += 0.3f * Projectile.direction;
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        for (int i = 0; i < Projectile.oldPos.Length; i++) {
            Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + Projectile.Size / 2f;
            Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length) * 0.5f;
            spriteBatch.Draw(texture, drawPos, null, color, Projectile.oldRot[i], drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        }
        spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        return false;
    }
}