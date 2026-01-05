using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class MarineMulcherTentacle : ModProjectile {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(20f);

    public override string Texture => ResourceManager.EmptyTexture;

    private Vector2 _center;

    public override void SetStaticDefaults() {

    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(40);

        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.tileCollide = false;

        Projectile.timeLeft = TIMELEFT;

        Projectile.alpha = 255;

        Projectile.MaxUpdates = 3;
    }

    public override void AI() {
        if (Projectile.localAI[1] == 0f) {
            _center = Projectile.Center;

            Projectile.localAI[1] = 1f;

            bool flag3 = false;

            Vector2 vector10 = Projectile.velocity;
            vector10.Normalize();
            vector10 *= 4f;
            if (!flag3) {
                Vector2 vector11 = new Vector2(Main.rand.Next(-100, 101), Main.rand.Next(-100, 101));
                vector11.Normalize();
                vector10 += vector11;
            }

            vector10.Normalize();
            vector10 *= 16f;
            float num17 = (float)Main.rand.Next(10, 80) * 0.001f;
            if (Main.rand.Next(2) == 0)
                num17 *= -1f;

            float num18 = (float)Main.rand.Next(10, 80) * 0.001f;
            if (Main.rand.Next(2) == 0)
                num18 *= -1f;

            if (flag3)
                num18 = (num17 = 0f);

            Projectile.ai[0] = num17;
            Projectile.ai[1] = num18;
        }

        Vector2 center15 = Projectile.Center;
        Projectile.scale = 1f - Projectile.localAI[0] * 0.75f;
        Projectile.width = (int)(30f * Projectile.scale);
        Projectile.height = Projectile.width;
        Projectile.position.X = center15.X - (float)(Projectile.width / 2);
        Projectile.position.Y = center15.Y - (float)(Projectile.height / 2);
        if ((double)Projectile.localAI[0] < 0.1)
            Projectile.localAI[0] += 0.01f * 1.625f;
        else
            Projectile.localAI[0] += 0.025f * 1.625f;

        if (Projectile.localAI[0] >= 2f)
            Projectile.Kill();

        Projectile.velocity.X += Projectile.ai[0] * 1f;
        Projectile.velocity.Y += Projectile.ai[1] * 1f;
        float maxSpeed = 16f;
        if (Projectile.velocity.Length() > maxSpeed) {
            Projectile.velocity.Normalize();
            Projectile.velocity *= maxSpeed;
        }

        Projectile.ai[0] *= 1.1f;
        Projectile.ai[1] *= 1.1f;
        if (Projectile.scale < 1f) {
            for (int num807 = 0; (float)num807 < Projectile.scale * 10f; num807++) {
                if (Main.rand.NextBool()) {
                    continue;
                }
                if (Projectile.scale < 0.2f) {
                    continue;
                }
                int num808 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, ModContent.DustType<MarineMulcherTentacleDust>(), Projectile.velocity.X, Projectile.velocity.Y, 100, default(Color), 1.1f);
                Main.dust[num808].position = (Main.dust[num808].position + Projectile.Center) / 2f;
                Main.dust[num808].noGravity = true;
                Main.dust[num808].velocity += Main.dust[num808].position.DirectionTo(_center) * 5f;
                Dust dust2 = Main.dust[num808];
                dust2.velocity *= 0.1f;
                dust2 = Main.dust[num808];
                dust2.velocity -= Projectile.velocity * (1.3f - Projectile.scale) * 1.5f;
                Main.dust[num808].fadeIn = 100 + Projectile.owner;
                dust2 = Main.dust[num808];
                dust2.scale += Projectile.scale * 0.75f;
            }
        }
    }
}