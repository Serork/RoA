using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class QuicksilverBolt : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    //public override void SetStaticDefaults()
    //	=> DisplayName.SetDefault("Quicksilver Bolt");

    public override void SetDefaults() {
        int width = 14; int height = width;
        Projectile.Size = new Vector2(width, height);

        //Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);

        Projectile.DamageType = DamageClass.Magic;

        Projectile.aiStyle = -1;

        Projectile.penetrate = 3;

        Projectile.timeLeft = 140;

        Projectile.friendly = true;

        Projectile.tileCollide = true;

        Projectile.alpha = 255;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 8;

        return true;
    }

    public override void AI() {
        Lighting.AddLight(Projectile.Center, new Color(79, 211, 123).ToVector3() * 0.5f);

        var dust = Dust.NewDust(Projectile.position + Vector2.One, 1, 1, ModContent.DustType<MercuriumDust>(), Projectile.velocity.X, Projectile.velocity.Y, 0, new Color(), Main.rand.NextFloat(0.9f, 1.1f));
        Main.dust[dust].velocity *= 0.1f;
        Main.dust[dust].scale *= 0.75f;

        if (Projectile.ai[0] == 0f)
            Projectile.ai[0] = Main.rand.Next(-2, 3);
        if (Main.rand.Next(40) == 0 && Projectile.timeLeft < 130) {
            Projectile.damage += (int)(Projectile.damage / 3);
            Projectile.velocity.Y += Projectile.ai[0];
            Projectile.velocity.X += Projectile.ai[0] / 2;
            Projectile.ai[0] += Main.rand.Next(-2, 3);
            int k = Main.rand.Next(26, 31);
            for (int i = 0; i < k; i++) {
                int x = (int)((double)Projectile.position.X - 3.0 + (double)Projectile.width / 2.0);
                int y = (int)((double)Projectile.position.Y - 8.0 + (double)Projectile.height / 2.0);
                Vector2 vector3 = (new Vector2((float)Projectile.width / 2f, Projectile.height) * 0.8f).RotatedBy((float)(i - (k / 2 - 1)) * ((float)Math.PI * 2f) / (float)k) + new Vector2((float)x, (float)y);
                Vector2 vector2 = -(vector3 - new Vector2((float)x, (float)y));
                int dust2 = Dust.NewDust(vector3 + vector2 * 2f * Main.rand.NextFloat() - new Vector2(1f, 2f), 0, 0, ModContent.DustType<MercuriumDust>(), vector2.X * 2f, vector2.Y * 2f, 0, default(Color), Main.rand.NextFloat(2.5f, 3.3f));
                Main.dust[dust2].noGravity = true;
                Main.dust[dust2].noLight = true;
                Main.dust[dust2].scale *= 0.3f;
                Main.dust[dust2].velocity = -Vector2.Normalize(vector2) * Main.rand.NextFloat(1.5f, 3f) * Main.rand.NextFloat();
            }
            SoundEngine.PlaySound(SoundID.Item118, new Vector2(Projectile.position.X, Projectile.position.Y));
        }
    }
    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 5; i++) {
            var dust2 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), 1, 1, ModContent.DustType<MercuriumDust>(), 0, 0, 0, new Color(), 0.9f);
            Main.dust[dust2].velocity *= 0.25f;
        }
        SoundEngine.PlaySound(SoundID.Item118, new Vector2(Projectile.position.X, Projectile.position.Y));
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 180);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(ModContent.BuffType<Buffs.ToxicFumes>(), 180);
    }
}