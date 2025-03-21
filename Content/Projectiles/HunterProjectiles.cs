using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Projectiles.Friendly.Melee;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles;

sealed class HunterProjectile1 : ModProjectile {
    private float _extraScale;

    public override string Texture => ResourceManager.EmptyTexture;

    public override void SetDefaults() {
        Projectile.Size = Vector2.One * 10f;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 180;
        Projectile.aiStyle = -1;
        Projectile.Opacity = 0f;

        Projectile.tileCollide = false;
    }

    public override void AI() {
        //Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.5f);

        float edge = 0.25f;
        float max = 1.5f;
        float speed = 0.05f;
        Projectile.Opacity *= Utils.GetLerpValue(max, max - edge, Projectile.ai[1], true);
        if (Projectile.ai[1] < max - edge && Projectile.Opacity < 1f) {
            Projectile.Opacity += 0.1f;
        }
        Player player = Main.LocalPlayer;
        bool flag = Projectile.ai[2] == -1f;
        NPC target = null;
        if (flag) {
            player = Main.player[Projectile.owner];
        }
        else {
            target = Main.npc[(int)Projectile.ai[2]];
        }
        int direction = (player.Center - Projectile.Center).X.GetDirection();
        Projectile.rotation += (0.25f - Projectile.ai[1] / 10f) * direction;

        _extraScale = Utils.GetLerpValue(0f, edge, Projectile.ai[1], true) * Utils.GetLerpValue(max, max - edge, Projectile.ai[1], true);
        if (Projectile.ai[1] < max) {
            Projectile.ai[1] += speed;
        }
        else {
            if ((Projectile.owner == Main.myPlayer && flag) || (!flag && Main.netMode != NetmodeID.MultiplayerClient)) {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Helper.VelocityToPoint(Projectile.Center, flag ? player.Center : target.Center, 10f),
                    ModContent.ProjectileType<HunterProjectile2>(), Projectile.damage, Projectile.knockBack, flag ? Projectile.owner : Main.myPlayer);
            }
            Projectile.Kill();
        }

        Projectile.scale = 0.5f;

        if (Projectile.ai[0] < 0.5f) {
            Projectile.ai[0] += 0.025f;
        }
    }

    public override bool? CanDamage() => false;

    public override bool? CanCutTiles() => false;

    public override bool ShouldUpdatePosition() => false;

    public override bool PreDraw(ref Color lightColor) {
        Color color = Color.White;
        Texture2D sparkle = ModContent.Request<Texture2D>(ProjectileLoader.GetProjectile(ModContent.ProjectileType<FlederSlayer>()).Texture + "_Spark").Value;
        Texture2D bloom = ModContent.Request<Texture2D>(ResourceManager.Textures + "Bloom0").Value;

        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        float scale = Projectile.ai[0] / 3f;
        Main.EntitySpriteDraw(bloom, drawPosition, null, color.MultiplyAlpha(Helper.Wave(0.5f, 1f, speed: 2f)) * 0.15f, Projectile.rotation, bloom.Size() / 2f, (Projectile.scale * 0.5f + Helper.Wave(-0.25f, 0.25f, speed: 1f)) * _extraScale, 0, 0);
        Main.EntitySpriteDraw(sparkle, drawPosition, null, color, Projectile.rotation, sparkle.Size() / 2f, (Projectile.scale + scale) * _extraScale, 0, 0);
        Main.EntitySpriteDraw(sparkle, drawPosition, null, color, Projectile.rotation + MathHelper.PiOver2, sparkle.Size() / 2f, (Projectile.scale + scale) * _extraScale, 0, 0);

        float Opacity = Projectile.Opacity;
        float AdditiveAmount = 0f;
        color = Color.White * Opacity * 0.9f;
        color.A /= 2;
        Texture2D value = TextureAssets.Extra[98].Value;
        Color color2 = Color.White * Opacity * 0.5f;
        color2.A = (byte)((float)(int)color2.A * (1f - AdditiveAmount));
        Vector2 origin = value.Size() / 2f;
        Color color3 = color * 0.5f;
        Vector2 vector = new Vector2(0.35f + scale, 0.8f + scale) * Opacity;
        Vector2 vector2 = new Vector2(0.35f + scale, 0.8f + scale) * Opacity;
        Vector2 position = Projectile.Center - Main.screenPosition;
        SpriteEffects effects = SpriteEffects.None;
        Main.spriteBatch.Draw(value, position, null, color2, (float)Math.PI / 2f + Projectile.rotation, origin, vector * _extraScale, effects, 0f);
        Main.spriteBatch.Draw(value, position, null, color2, 0f + Projectile.rotation, origin, vector2 * _extraScale, effects, 0f);
        Main.spriteBatch.Draw(value, position, null, color3, (float)Math.PI / 2f + Projectile.rotation, origin, vector * 0.6f * _extraScale, effects, 0f);
        Main.spriteBatch.Draw(value, position, null, color3, 0f + Projectile.rotation, origin, vector2 * 0.6f * _extraScale, effects, 0f);

        return false;
    }
}

sealed class HunterProjectile2 : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override Color? GetAlpha(Color lightColor) => Color.White;

    public override void SetDefaults() {
        Projectile.width = 4;
        Projectile.height = 4;
        Projectile.aiStyle = 1;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.light = 0.5f;
        Projectile.alpha = 255;
        Projectile.timeLeft = 240;

        Projectile.extraUpdates = 2;

        bool flag = Projectile.owner != 255;
        if (!flag) {
            Projectile.friendly = true;
            Projectile.hostile = false;
            return;
        }
        Projectile.friendly = false;
        Projectile.hostile = true;
    }

    public override void AI() {
        Projectile.tileCollide = Projectile.timeLeft < 220;

        Projectile.ai[0] -= 1f;

        if (Main.rand.Next(6) == 0) {
            int num179 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.SilverFlame, Projectile.velocity.X, Projectile.velocity.Y, 100, default(Color), 1.2f);
            Main.dust[num179].noLightEmittence = true;
            Main.dust[num179].noGravity = true;
            Main.dust[num179].velocity *= 0.2f;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        if (Projectile.timeLeft > 240 - 1) {
            return false;
        }

        Main.instance.LoadProjectile(ProjectileID.SilverBullet);
        Texture2D texture = TextureAssets.Projectile[ProjectileID.SilverBullet].Value;
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(texture, drawPosition, null, Color.White, Projectile.rotation, texture.Size() / 2f, Projectile.scale, 0, 0);

        return false;
    }

    public override void OnKill(int timeLeft) {
        Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.SilverBulletSparkle, new ParticleOrchestraSettings {
            PositionInWorld = Projectile.Center,
            MovementVector = Vector2.Zero
        }, Projectile.owner);
    }
}
