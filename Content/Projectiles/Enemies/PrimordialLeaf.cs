using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Dusts;
using RoA.Core;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies;

sealed class PrimordialLeaf : ModProjectile {
    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        Main.projFrames[Projectile.type] = 5;
    }

    public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.White, 0.9f);

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = ResourceManager.DefaultSparkle;
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
        SpriteEffects effects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        for (int k = 0; k < Projectile.oldPos.Length - 1; k++) {
            Vector2 drawPos = Projectile.oldPos[k] + new Vector2(Projectile.width, Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition;
            Color color = new Color(0, 150 - k * 10, 200 + k * 5, 50);
            spriteBatch.Draw(texture, drawPos, null, color * 0.2f, Projectile.oldRot[k] + (float)Math.PI / 2, drawOrigin, Projectile.scale - k / (float)Projectile.oldPos.Length, effects, 0f);
            //spriteBatch.DrawSelf(texture, drawPos - Projectile.OldUseItemPos[k] * 0.5f + Projectile.OldUseItemPos[k + 1] * 0.5f, null, color * 0.45f, Projectile.OldUseItemRot[k] * 0.5f + Projectile.OldUseItemRot[k + 1] * 0.5f + (float)Math.PI / 2, drawOrigin, Projectile.scale - k / (float)Projectile.OldUseItemPos.Length, effects, 0f);
        }
        return true;
    }

    public override void SetDefaults() {
        Projectile.hostile = true;
        Projectile.width = Projectile.height = 12;
        Projectile.aiStyle = 0;
        Projectile.extraUpdates = 1;
        Projectile.timeLeft = 360;

        DrawOffsetX = -6;
        DrawOriginOffsetY = -2;
    }

    public override void AI() {
        Projectile.rotation = Projectile.velocity.ToRotation();
        if (Main.netMode != NetmodeID.Server) {
            if (Main.rand.NextBool(5)) {
                int num2 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 2, 2, 15, 0f, 0f, 40, default, 1f);
                Main.dust[num2].velocity *= 0.2f;
            }
        }
        /*if (Projectile.ai[1] == 0) Projectile.ai[1] = Main.rand.NextFloat(-0.002f, 0.002f);
        Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[1]);*/
    }

    public override void PostAI() {
        Projectile.frameCounter++;
        if (Projectile.frameCounter > 5) {
            Projectile.frame++;
            Projectile.frameCounter = 0;
            if (Projectile.frame >= 5) {
                Projectile.frame = 0;
            }
        }
    }

    public override void OnKill(int timeLeft) => Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 2, 2, ModContent.DustType<GhostLeaf>(), Projectile.velocity.X, Projectile.velocity.Y * 0.02f, 125, default, 1f);
}
