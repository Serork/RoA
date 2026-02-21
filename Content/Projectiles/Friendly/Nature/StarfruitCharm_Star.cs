using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

using RoA.Common;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class StarfruitCharmStar : ModProjectile {
    public override Color? GetAlpha(Color lightColor) {
        return new Color(255, 255, 255, 0) * Projectile.Opacity;
    }

    public override void SetDefaults() {
        Projectile.ArmorPenetration = 25; // Added by TML.

        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = 2;
        Projectile.alpha = 50;
        Projectile.tileCollide = false;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteEffects dir = SpriteEffects.None;
        if (Projectile.spriteDirection == -1)
            dir = SpriteEffects.FlipHorizontally;

        Texture2D value27 = TextureAssets.Projectile[Projectile.type].Value;
        Microsoft.Xna.Framework.Rectangle rectangle7 = new Microsoft.Xna.Framework.Rectangle(0, 0, value27.Width, value27.Height);
        Vector2 origin13 = rectangle7.Size() / 2f;
        Microsoft.Xna.Framework.Color color50 = Projectile.GetAlpha(lightColor);
        Texture2D value28 = TextureAssets.Extra[ExtrasID.FallingStar].Value;
        Microsoft.Xna.Framework.Rectangle value29 = value28.Frame();
        Vector2 origin14 = new Vector2((float)value29.Width / 2f, 10f);
        _ = Microsoft.Xna.Framework.Color.White * 0.2f;
        Vector2 vector42 = new Vector2(0f, Projectile.gfxOffY);
        Vector2 spinningpoint2 = new Vector2(0f, -5f);
        float num195 = (float)TimeSystem.TimeForVisualEffects / 60f;
        Vector2 vector43 = Projectile.Center + Projectile.velocity;
        float num196 = 1.5f;
        float num197 = 1.1f;
        float num198 = 1.3f;
        Microsoft.Xna.Framework.Color color51 = new Color(241, 172, 55) * 0.1f;
        Microsoft.Xna.Framework.Color color52 = Microsoft.Xna.Framework.Color.White * 0.3f;
        color52.A = 0;
        byte a = 0;
        float num199 = 1f;
        bool flag29 = true;
        float num200 = Projectile.scale + 0.1f;

        Color yellowColor = new Color(255, 214, 56);
        Color orangeColor = new Color(207, 127, 22);

        Microsoft.Xna.Framework.Color.Lerp(Microsoft.Xna.Framework.Color.Black, orangeColor, 0.75f);
        Microsoft.Xna.Framework.Color color54 = Microsoft.Xna.Framework.Color.Lerp(Microsoft.Xna.Framework.Color.Black, yellowColor, 0.5f);
        Microsoft.Xna.Framework.Color value32 = orangeColor * 0.75f;
        color54 = yellowColor * 0.5f;
        color51 = Microsoft.Xna.Framework.Color.Lerp(value32, color54, 0.2f) * 0.3f;
        color52 = Microsoft.Xna.Framework.Color.Lerp(value32, color54, 0.8f) * 0.4f;
        a = 0;
        float num203 = 0.5f;
        num196 -= num203;
        num197 -= num203;
        num198 -= num203;

        Microsoft.Xna.Framework.Color color55 = color51;
        Microsoft.Xna.Framework.Color color56 = color51;
        Microsoft.Xna.Framework.Color color57 = color51;
        if (flag29) {
            color55.A = a;
            color56.A = a;
            color57.A = a;
        }

        Main.EntitySpriteDraw(value28, vector43 - Main.screenPosition + vector42 + spinningpoint2.RotatedBy((float)Math.PI * 2f * num195), value29, color55, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin14, num196, SpriteEffects.None);
        Main.EntitySpriteDraw(value28, vector43 - Main.screenPosition + vector42 + spinningpoint2.RotatedBy((float)Math.PI * 2f * num195 + (float)Math.PI * 2f / 3f), value29, color56, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin14, num197, SpriteEffects.None);
        Main.EntitySpriteDraw(value28, vector43 - Main.screenPosition + vector42 + spinningpoint2.RotatedBy((float)Math.PI * 2f * num195 + 4.1887903f), value29, color57, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin14, num198, SpriteEffects.None);
        Vector2 vector44 = Projectile.Center - Projectile.velocity * 0.5f;
        for (float num207 = 0f; num207 < 1f; num207 += 0.5f) {
            float num208 = num195 % 0.5f / 0.5f;
            num208 = (num208 + num207) % 1f;
            float num209 = num208 * 2f;
            if (num209 > 1f)
                num209 = 2f - num209;

            Main.EntitySpriteDraw(value28, vector44 - Main.screenPosition + vector42, value29, color52 * num209, Projectile.velocity.ToRotation() + (float)Math.PI / 2f, origin14, (0.5f + num208 * 0.5f) * num199, SpriteEffects.None);
        }

        Main.EntitySpriteDraw(value27, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle7, color50, Projectile.rotation, origin13, num200, dir);

        return false;
    }

    public override void AI() {
        if (Projectile.Center.Y > Projectile.ai[1])
            Projectile.tileCollide = true;

        if (Projectile.soundDelay == 0) {
            Projectile.soundDelay = 20 + Main.rand.Next(40);
            SoundEngine.PlaySound(SoundID.Item9, Projectile.position);
        }

        Projectile.alpha -= 15;
        int num84 = 100;
        if (Projectile.Center.Y >= Projectile.ai[1])
            num84 = 0;

        if (Projectile.alpha < num84)
            Projectile.alpha = num84;

        Projectile.localAI[0] += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * (float)Projectile.direction;

        Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * (float)Projectile.direction;

        Vector2 vector12 = new Vector2(Main.screenWidth, Main.screenHeight);
        if (!Main.dedServ && Projectile.Hitbox.Intersects(Utils.CenteredRectangle(Main.screenPosition + vector12 / 2f, vector12 + new Vector2(400f))) && Main.rand.Next(6) == 0) {
            Gore.NewGore(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity * 0.2f, 16);
        }

        for (float num88 = 0f; num88 < 3f; num88 += 1f) {
            Vector2 vector13 = Projectile.Center + Main.rand.RandomPointInArea(4f) /*+ new Vector2(0f, 12f * Projectile.scale).RotatedBy(Projectile.position.Y / 200f + num88 * ((float)Math.PI * 2f) + Projectile.rotation)*/ - Projectile.velocity * 0.5f;
            Dust dust5 = Dust.NewDustPerfect(vector13, DustID.RainbowMk2, Projectile.velocity * 0.2f * num88, 0, new Color(255, 214, 56));
            dust5.noLight = true;
            dust5.noGravity = true;
            dust5 = Dust.NewDustPerfect(vector13, DustID.RainbowMk2, Projectile.velocity * 0.2f * num88, 0, Color.White, 0.4f);
            dust5.noLight = true;
            dust5.noGravity = true;
        }
    }

    public override void OnKill(int timeLeft) {
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        for (int num584 = 0; num584 < 10; num584++) {
            Dust dust48 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.GemTopaz, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, Color.LightYellow, 1.2f);
            dust48.noGravity = true;
            dust48.velocity.X *= 2f;
        }

        if (Projectile.IsOwnerLocal()) {
            ProjectileUtils.SpawnPlayerOwnedProjectile<Starfruit>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_Death()) {
                Position = Projectile.Center
            });
        }

        if (Main.dedServ) {
            return;
        }

        for (int num585 = 0; num585 < 3; num585++) {
            Gore gore3 = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.position, new Vector2(Projectile.velocity.X * 0.05f, Projectile.velocity.Y * 0.05f), 16);
            Gore gore2 = gore3;
            gore2.velocity *= 2f;
        }
    }
}
