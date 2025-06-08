using Microsoft.Xna.Framework;

using System;

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class FlederFlame : FormProjectile {
    protected override void SafeSetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 300;
        Projectile.extraUpdates = 1;
    }

    public override void AI() {
        if (Projectile.timeLeft < 250) {
            Projectile.friendly = true;
        }

        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.ToRadians(90);
        Lighting.AddLight(Projectile.Center, 0.1f, 0.2f, 0.6f);

        if (Main.rand.NextBool(25)) {
            int trail = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, 59, Projectile.velocity.X * 0.4f, Projectile.velocity.Y * 0.4f, 0, default(Color), Main.rand.NextFloat(1f, 1.5f));
            Main.dust[trail].fadeIn = 1.3f;
            Main.dust[trail].velocity *= 1.5f;
            Main.dust[trail].noGravity = true;
            Main.dust[trail].scale *= 0.7f;
        }

        float num3 = Projectile.Center.X;
        float num4 = Projectile.Center.Y;
        float num5 = 500f;
        bool flag = false;
        for (int index = 0; index < 200; ++index) {
            if (Main.npc[index].CanBeChasedBy(Projectile, false) && (double)Projectile.Distance(Main.npc[index].Center) < num5 && Collision.CanHit(Projectile.Center, 1, 1, Main.npc[index].Center, 1, 1)) {
                float num1 = Main.npc[index].position.X + (Main.npc[index].width / 2);
                float num2 = Main.npc[index].position.Y + (Main.npc[index].height / 2);
                float num6 = Math.Abs(Projectile.position.X + (Projectile.width / 2) - num1) + Math.Abs(Projectile.position.Y + (Projectile.height / 2) - num2);
                if (num6 < num5) {
                    num5 = num6;
                    num3 = num1;
                    num4 = num2;
                    flag = true;
                }
            }
        }
        if (!flag) {
            Projectile.velocity *= 0.99f;
            return;
        }

        float num7 = 9f;
        Vector2 vector2 = new Vector2(Projectile.position.X + Projectile.width * 0.5f, Projectile.position.Y + Projectile.height * 0.5f);
        float num8 = num3 - vector2.X;
        float num9 = num4 - vector2.Y;
        float num10 = (float)Math.Sqrt(num8 * num8 + num9 * num9);
        float num11 = num7 / num10;
        float num12 = num8 * num11;
        float num13 = num9 * num11;
        Projectile.velocity.X = (float)((Projectile.velocity.X * 20.0 + num12) / 21.0);
        Projectile.velocity.Y = (float)((Projectile.velocity.Y * 20.0 + num13) / 21.0);
    }

    public override void OnKill(int timeLeft) {
        for (int index1 = 11; index1 > 0; --index1) {
            int index2 = Dust.NewDust(Projectile.position, 2, 2, 59, 0.0f, 0.0f, 100, new Color(), 2f);
            Main.dust[index2].noGravity = true;
            Vector2 velocity = Projectile.velocity;
            Main.dust[index2].velocity = velocity.RotatedBy((15 * (index1 + 2)), new Vector2()) * Main.rand.NextFloat();
        }
    }
}
