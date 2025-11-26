using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Dusts;
using RoA.Content.Items.Weapons.Ranged.Hardmode;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class NewMoneyBat : ModProjectile {
    private static Asset<Texture2D> _trailTexture = null!;

    public override void SetStaticDefaults() {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60; // The length of old position to be recorded
        ProjectileID.Sets.TrailingMode[Projectile.type] = 3; // The recording mode

        Projectile.SetFrameCount(3);

        if (!Main.dedServ) {
            _trailTexture = ModContent.Request<Texture2D>(Texture + "_Trail");
        }
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;

        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;
    }

    public override void AI() {
        if (Projectile.localAI[1] == 0f && Projectile.localAI[2] == 0f) {
            Projectile.localAI[1] = NewMoney.BITE_ANIMATIONTIME * 0.625f;
            Projectile.localAI[2] = 1f;
        }
        Lighting.AddLight(Projectile.Center, NewMoneyBullet.BulletColor.ToVector3() * 0.5f);

        Projectile.Animate(NewMoney.BAT_ANIMATIONTIME);

        Projectile.rotation = Projectile.velocity.X * 0.025f;

        Player player = Projectile.GetOwnerAsPlayer();
        if (Projectile.localAI[1] >= 0f) {
            Projectile.SlightlyMoveTo(player.Center, speed: 7.5f);
        }
        Projectile.localAI[1] += 1f;
        if (player.Distance(Projectile.Center) < 40f || Projectile.localAI[0] > 0f) {
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] > 20f) {
                Projectile.Kill();

                for (int num363 = 0; num363 < 20; num363++) {
                    int num364 = Dust.NewDust(Projectile.Center + Main.rand.RandomPointInArea(24f, 14f).RotatedBy(Projectile.rotation), 0, 0, ModContent.DustType<NewMoneyDust>(),
                        Alpha: 50);
                    Main.dust[num364].noGravity = true;
                    Main.dust[num364].velocity += Projectile.velocity * 0.5f * Main.rand.NextFloat();
                    if (Main.rand.Next(2) == 0) {
                        Main.dust[num364].noGravity = true;
                        Main.dust[num364].scale = 1.5f * Projectile.scale;
                    }
                    else {
                        Main.dust[num364].scale = 0.8f + 0.2f * Main.rand.NextFloat() * Projectile.scale;
                    }
                }
            }
        }
    }

    public override bool ShouldUpdatePosition() => true;

    public override bool? CanDamage() => false;

    public override bool PreDraw(ref Color lightColor) {
        if (_trailTexture?.IsLoaded != true) {
            return false;
        }

        Projectile projectile = Projectile;
        Texture2D mainTex = projectile.GetTexture();
        float scale = Projectile.scale;
        Color color = Color.White;
        int frameSize = mainTex.Height / Main.projFrames[projectile.type];
        Rectangle frameBox = new(0, frameSize * projectile.frame, mainTex.Width, frameSize);
        SpriteEffects effects = projectile.spriteDirection.ToSpriteEffects();
        Vector2 origin = frameBox.Size() / 2;
        float trailOpacity = Ease.CubeOut(MathUtils.Clamp01(Projectile.localAI[1] / 50f));
        trailOpacity = MathUtils.Clamp01(trailOpacity);
        bool flag = false/*Projectile.localAI[0] > 10f*/;
        if (flag) {
            trailOpacity *= 1f - (Projectile.localAI[0] - 10f) / 10f;
        }
        int num3 = Projectile.oldPos.Length;
        int num4 = num3 - 1 - 3;
        int num5 = 0;
        int num6 = 5;
        float num10 = 4f;
        float num12 = (float)Math.PI;
        float num7 = 32f;
        float num8 = 16f;
        if (flag) {
            scale *= trailOpacity;
        }
        if (trailOpacity >= 0.5f) {
            for (int num13 = num4; num13 >= num5; num13 -= num6) {
                Vector2 vector2 = Projectile.oldPos[num13] - Projectile.position;
                float num14 = Utils.Remap(num13, 0f, num3, 1f, 0f);
                float num15 = 1f - num14;
                if (num14 < 0.9f) {
                    continue;
                }
                Vector2 spinningpoint = new Vector2((float)Math.Sin((double)((float)Projectile.whoAmI / 1f) + Main.timeForVisualEffects / (double)num10 + (double)(num14 * 2f * ((float)Math.PI * 2f))) * num8, 0f - num7) * num15;
                vector2 += spinningpoint.RotatedBy(num12);
                Color color3 = NewMoneyBullet.BulletColor;
                color3.A = (byte)(color3.A * 0.625f);
                Main.spriteBatch.Draw(_trailTexture.Value, projectile.Center - Main.screenPosition + vector2 * trailOpacity, frameBox, color3 * num14 * 0.5f, projectile.oldRot[num13], origin,
                    scale * Utils.Remap(num14 * num14, 0f, 1f, 0f, 2.5f) * trailOpacity * 0.75f * 0.875f, effects, 0f);
            }
        }

        Main.spriteBatch.Draw(mainTex, projectile.Center - Main.screenPosition, frameBox, color * trailOpacity, projectile.rotation,
                              origin, scale, effects, 0);

        float trailOpacity2 = trailOpacity;
        trailOpacity = 1f - trailOpacity;
        trailOpacity *= Utils.GetLerpValue(0f, 0.5f, trailOpacity2, true);
        for (int num13 = num4; num13 >= num5; num13 -= num6) {
            Vector2 vector2 = Projectile.oldPos[num13] - Projectile.position;
            float num14 = Utils.Remap(num13, 0f, num3, 1f, 0f);
            float num15 = 1f - num14;
            Vector2 spinningpoint = new Vector2((float)Math.Sin((double)((float)Projectile.whoAmI / 1f) + Main.timeForVisualEffects / (double)num10 + (double)(num14 * 2f * ((float)Math.PI * 2f))) * num8, 0f - num7) * num15;
            vector2 += spinningpoint.RotatedBy(num12);
            Color color3 = NewMoneyBullet.BulletColor;
            color3.A = (byte)(color3.A * 0.625f);
            Main.spriteBatch.Draw(_trailTexture.Value, projectile.Center - Main.screenPosition + vector2, frameBox, color3 * num14 * 0.5f, projectile.rotation, origin,
                scale * Utils.Remap(num14 * num14, 0f, 1f, 0.25f, 2.5f) * 1f * trailOpacity, effects, 0f);
        }

        return false;
    }
}
