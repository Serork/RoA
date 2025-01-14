using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class ChemicalFlask : ModProjectile {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Chemical Flask");

        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }

    public override void SetDefaults() {
        Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);

        int width = 14; int height = 20;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = 68;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;

        Projectile.penetrate = 1;
        Projectile.timeLeft = 1200;
    }

    public override void AI() {
        Player player = Main.player[Projectile.owner];
        Lighting.AddLight(Projectile.Center, 0.1f, 0.3f, 0.1f);
        if (Projectile.timeLeft <= 1175) {
            int _dust = Dust.NewDust(new Vector2(Projectile.position.X + 7, Projectile.position.Y + 10) + new Vector2(0, -8).RotatedBy(Projectile.rotation), 2, 2, 44, 0f, 0f, 100, default, 1.1f);
            Main.dust[_dust].scale += Main.rand.Next(40) * 0.01f;
            Main.dust[_dust].noGravity = true;
            Main.dust[_dust].velocity *= 0.8f;
        }

        float distance = 10f;
        for (int findProjectile = 0; findProjectile < Main.maxProjectiles; findProjectile++) {
            Projectile projectile = Main.projectile[findProjectile];
            if (projectile.active && projectile.aiStyle == 1 && Vector2.Distance(Projectile.Center, projectile.Center) < distance)
                Projectile.Kill();
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = (Texture2D)ModContent.Request<Texture2D>(Texture);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < Projectile.oldPos.Length; k++) {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            spriteBatch.Draw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        }
        return true;
    }

    public override void OnKill(int timeLeft) {
        Vector2 _position = new Vector2(20f, 20f);
        for (int i = 0; i < 20; i++) {
            int dust = Dust.NewDust(Projectile.Center - _position / 2f, (int)_position.X, (int)_position.Y, 44, 0f, 0f, 100, default, 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 5f;
            dust = Dust.NewDust(Projectile.Center - _position / 2f, (int)_position.X, (int)_position.Y, 75, 0f, 0f, 100, default, 1.5f);
            Main.dust[dust].velocity *= 3f;
        }
        SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
        Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.oldVelocity * 0.2f, ModContent.Find<ModGore>(RoA.ModName + "/ChemicalFlask1").Type, 1f);
        Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.oldVelocity * 0.2f, ModContent.Find<ModGore>(RoA.ModName + "/ChemicalFlask2").Type, 1f);
        Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.oldVelocity * 0.2f, ModContent.Find<ModGore>(RoA.ModName + "/ChemicalFlask3").Type, 1f);
        Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.oldVelocity * 0.2f, ModContent.Find<ModGore>(RoA.ModName + "/ChemicalFlask4").Type, 1f);
        if (Projectile.owner == Main.myPlayer) {
            for (int i = 0; i < 3; i++) {
                float _velocityX = -Projectile.velocity.X * Main.rand.Next(10, 30) * 0.01f + Main.rand.Next(-20, 21) * 0.3f;
                float _velocityY = -Math.Abs(Projectile.velocity.Y) * Main.rand.Next(10, 30) * 0.01f + Main.rand.Next(-20, 5) * 0.3f;
                int randomPro = Main.rand.Next(1, 3);
                ushort _type = 0;
                if (randomPro == 1) _type = (ushort)ModContent.ProjectileType<ToxicCrystal1>();
                if (randomPro == 2) _type = (ushort)ModContent.ProjectileType<ToxicCrystal2>();
                if (randomPro == 3) _type = (ushort)ModContent.ProjectileType<ToxicCrystal3>();
                Projectile.NewProjectile(Projectile.GetSource_Death(), new Vector2(Projectile.Center.X + _velocityX, Projectile.Center.Y + _velocityY), new Vector2(_velocityX, _velocityY), _type, (int)(Projectile.damage * 0.33f), 1f, Projectile.owner);
            }
        }
    }
}