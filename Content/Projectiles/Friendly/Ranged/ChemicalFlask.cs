using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Dusts;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class ChemicalFlask : ModProjectile {
    private static Asset<Texture2D> _glowTexture = null!;

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Chemical Flask");

        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;

        if (Main.dedServ) {
            return;
        }

        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    public override void SetDefaults() {
        //Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);

        int width = 18; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = 68;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;

        Projectile.penetrate = 1;
        Projectile.timeLeft = 1200;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 12;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
        for (int k = 0; k < Projectile.oldPos.Length; k++) {
            Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
            Color color2 = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
            spriteBatch.Draw(texture, drawPos, null, color2, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            //spriteBatch.DrawSelf(ModContent.Request<Texture2D>(Texture + "_Glow").Value, drawPos, null, DrawColor.White * ((Projectile.OldUseItemPos.Length - k) / (float)Projectile.OldUseItemPos.Length), Projectile._rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
        }
        SpriteEffects spriteEffects = (SpriteEffects)(Projectile.velocity.X > 0f).ToInt();
        Vector2 position = Projectile.Center - Main.screenPosition;
        Rectangle sourceRectangle = new(0, 0, texture.Width, texture.Height);
        Color baseColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates());
        Color color = baseColor * Projectile.Opacity;
        Vector2 origin = sourceRectangle.Size() / 2f;
        Color color3 = new Color(223, 255, 95).MultiplyRGB(baseColor) * Projectile.Opacity;
        color3.A = 150;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color3 * 0.25f * Helper.Wave(0.5f, 1f, 10f, Projectile.whoAmI), Projectile.rotation, origin, Projectile.scale * 2.25f * Helper.Wave(0.95f, 1.1f, 5f, Projectile.whoAmI), spriteEffects);
        Main.EntitySpriteDraw(texture, position, sourceRectangle, color, Projectile.rotation, origin, Projectile.scale, spriteEffects);
        texture = _glowTexture.Value;
        Main.EntitySpriteDraw(texture, position, sourceRectangle, Color.White, Projectile.rotation, origin, Projectile.scale, spriteEffects);

        return false;
    }

    public override void AI() {
        float num104 = Projectile.scale;
        //Lighting.AddLight((int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), num104 * 0.7f, num104, num104 * 0.8f);

        Player player = Main.player[Projectile.owner];
        if (Projectile.timeLeft <= 1125) {
            int dust = Dust.NewDust(new Vector2(Projectile.position.X + 7, Projectile.position.Y + 10) + new Vector2(0, -8).RotatedBy(Projectile.rotation), 2, 2, 44, 0f, 0f, 100, default, 1.1f);
            Main.dust[dust].scale += Main.rand.Next(40) * 0.01f;
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.8f;
        }

        float distance = 40f;
        for (int findProjectile = 0; findProjectile < Main.maxProjectiles; findProjectile++) {
            Projectile projectile = Main.projectile[findProjectile];
            if (projectile.owner == Projectile.owner && projectile.active && projectile.aiStyle == 1 && Vector2.Distance(Projectile.Center, projectile.Center) < distance)
                Projectile.Kill();
        }
    }

    public override void OnKill(int timeLeft) {
        Vector2 _position = new Vector2(20f, 20f);
        for (int i = 0; i < 20; i++) {
            int dust = Dust.NewDust(Projectile.Center - _position / 2f, (int)_position.X, (int)_position.Y, 44, 0f, 0f, 100, default, 2f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 5f;
            Main.dust[dust].noLight = Main.dust[dust].noLightEmittence = true;
            dust = Dust.NewDust(Projectile.Center - _position / 2f, (int)_position.X, (int)_position.Y, ModContent.DustType<LothorPoison>(), 0f, 0f, 100, default, 1.5f);
            Main.dust[dust].velocity *= 3f;
        }
        SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);
        if (!Main.dedServ) {
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.oldVelocity * 0.2f, ModContent.Find<ModGore>(RoA.ModName + "/ChemicalFlask1").Type, 1f);
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.oldVelocity * 0.2f, ModContent.Find<ModGore>(RoA.ModName + "/ChemicalFlask2").Type, 1f);
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.oldVelocity * 0.2f, ModContent.Find<ModGore>(RoA.ModName + "/ChemicalFlask3").Type, 1f);
            Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.oldVelocity * 0.2f, ModContent.Find<ModGore>(RoA.ModName + "/ChemicalFlask4").Type, 1f);
        }
        if (Projectile.owner == Main.myPlayer) {
            for (int i = 0; i < 3; i++) {
                float _velocityX = -Projectile.velocity.X * Main.rand.Next(10, 30) * 0.01f + Main.rand.Next(-20, 21) * 0.3f;
                float _velocityY = -Math.Abs(Projectile.velocity.Y) * Main.rand.Next(10, 30) * 0.01f + Main.rand.Next(-20, 5) * 0.3f;
                int randomPro = Main.rand.Next(1, 3);
                ushort _type = 0;
                if (randomPro == 1) _type = (ushort)ModContent.ProjectileType<ToxicCrystal1>();
                if (randomPro == 2) _type = (ushort)ModContent.ProjectileType<ToxicCrystal2>();
                if (randomPro == 3) _type = (ushort)ModContent.ProjectileType<ToxicCrystal3>();
                Projectile.NewProjectile(Projectile.GetSource_Death(), new Vector2(Projectile.Center.X + _velocityX, Projectile.Center.Y + _velocityY), new Vector2(_velocityX, _velocityY), _type, (int)(Projectile.damage * 0.6f), Projectile.knockBack / 2f, Projectile.owner);
            }
        }
    }
}