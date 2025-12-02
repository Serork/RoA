using Microsoft.Xna.Framework;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Magic;

sealed class BloodyFeather : ModProjectile {
    //public override void SetStaticDefaults()	
    //	=> DisplayName.SetDefault("Bloody Feather");

    public override void SetDefaults() {
        //Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);

        int width = 20, height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.aiStyle = -1;

        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;

        Projectile.penetrate = 1;
        Projectile.timeLeft = 120;

        //Projectile.extraUpdates = 1;
        Projectile.alpha = 255;
    }

    public override void AI() {
        if (Projectile.alpha >= 0) Projectile.alpha -= 22;
        if (Projectile.timeLeft == 60) Projectile.tileCollide = true;
        if (Projectile.localAI[0] == 0f) {
            Player player = Main.player[Projectile.owner];
            Projectile.localAI[0] = -player.direction;
        }
        Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X) + 1.57f;
        int direction = (int)Projectile.localAI[0];
        Projectile.ai[0] += 1 * direction;
        if (Math.Abs(Projectile.ai[0]) % 8 == 0 && Projectile.velocity.Length() > 6f) {
            bool flag = Main.rand.NextBool(3);
            int dust = Dust.NewDust(Projectile.Center - Vector2.One * 2 - Projectile.velocity * 3f, 4, 4, flag ? 60 : 96, Projectile.velocity.X, Projectile.velocity.Y, 0, default, flag ? 1.2f : 0.8f);
            Main.dust[dust].velocity *= 0.75f;
        }
        Projectile.velocity.X = (float)(Math.Cos(60) * Projectile.ai[0] / 8);
        Projectile.velocity.X *= 1.2f;
        Projectile.velocity.Y *= 0.98f;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 10;
        if (Projectile.timeLeft > 60) return false;
        else return true;
    }

    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 4; i++) {
            bool flag = Main.rand.NextBool(3);
            int dust2 = Dust.NewDust(Projectile.Center - Vector2.One * 2, 4, 4, flag ? 60 : 96, 0, 0, 0, default, Main.rand.NextFloat(1f, 1.2f) * (flag ? 1.5f : 1.25f));
            Main.dust[dust2].noGravity = true;
            Main.dust[dust2].fadeIn = 0.75f;

            if (Main.rand.NextBool()) {
                flag = Main.rand.NextBool(3);
                int dust = Dust.NewDust(Projectile.Center - Vector2.One * 2, 4, 4, flag ? 60 : 96, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 0, default, Main.rand.NextFloat(1f, 1.2f) * (flag ? 1.25f : 1f));
                Main.dust[dust].velocity *= 0.75f;
            }
        }
    }
}