using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Miscellaneous;

sealed class RagingBootsWave : NatureProjectile {
    public override void SetStaticDefaults() {
        Main.projFrames[Projectile.type] = 4;
    }

    protected override void SafeSetDefaults() {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.aiStyle = 0;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        AIType = 14;

        ShouldApplyAttachedNatureWeaponCurrentDamage = false;
    }

    //public override bool? CanCutTiles() => false;

    public override void AI() {
        if (Projectile.localAI[2] == 0f) {
            Projectile.localAI[2] = 1f;

            Projectile.timeLeft = (int)Projectile.ai[0];
            Projectile.frameCounter = Projectile.frame = Main.rand.Next(Main.projFrames[Projectile.type]);
        }

        Projectile.rotation = Helper.VelocityAngle(Projectile.velocity) - MathHelper.PiOver2;

        Projectile.velocity *= 0.95f;

        Projectile.Opacity = Utils.GetLerpValue(0, 12, Projectile.timeLeft, true);
    }

    //public override bool? CanDamage() => Projectile.Opacity >= 0.5f;

    public override void SafePostAI() {
        ++Projectile.frameCounter;
        if (Projectile.frameCounter > 3) {
            ++Projectile.frame;
            Projectile.frameCounter = 0;
        }
        if (Projectile.frame > Main.projFrames[Projectile.type] - 1)
            Projectile.frame = 0;

        List<Color> colors = [new Color(147, 177, 253), new Color(50, 107, 197), new Color(9, 61, 191)];
        if (Main.rand.Next(5) == 0) {
            var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Snow, 0f, 0f, 50, Main.rand.NextFromList([.. colors]));
            d.scale *= 0.75f;
            d.velocity = -Projectile.velocity;
            d.noGravity = true;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D projectileTexture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        int frameHeight = projectileTexture.Height / Main.projFrames[Type];
        Rectangle frameRect = new Rectangle(0, Projectile.frame * frameHeight, projectileTexture.Width, frameHeight);
        Vector2 drawOrigin = new Vector2(projectileTexture.Width / 2f, projectileTexture.Height / Main.projFrames[Projectile.type] * 0.5f);
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        Color color = Projectile.GetAlpha(lightColor);
        spriteBatch.Draw(projectileTexture, drawPos, frameRect, color, Projectile.rotation, drawOrigin, 1f, Projectile.velocity.X > 0f ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);

        return false;
    }
}
