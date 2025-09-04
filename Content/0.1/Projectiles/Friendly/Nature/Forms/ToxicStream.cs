using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class ToxicStream : FormProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(ref Color lightColor) => false;

    protected override void SafeSetDefaults() {
        Projectile.width = 4;
        Projectile.height = 4;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 1;
        Projectile.extraUpdates = 2;
        Projectile.alpha = 0;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Inflate(4, 4);
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f && Projectile.ai[2] == 2f) {
            Projectile.localAI[0] = 1f;
            for (int num58 = 0; num58 < 2; num58++) {
                int num59 = Dust.NewDust(Projectile.position + Projectile.velocity * 2.5f, 0, 0, 107, 0f, 0f, 0, default(Color), 1f + Main.rand.NextFloatRange(0.1f));
                Main.dust[num59].velocity = Main.dust[num59].velocity.RotatedByRandom(MathHelper.PiOver4);
                Main.dust[num59].velocity *= 0.2f;
                Main.dust[num59].velocity += Projectile.velocity / 5f;
                Main.dust[num59].noGravity = true;
                Main.dust[num59].fadeIn = 1.25f;
            }
        }

        Lighting.AddLight(Projectile.Center, 0, 0.2f * Projectile.scale, 0);
        Projectile.scale -= 0.002f;
        if (Projectile.scale <= 0f)
            Projectile.Kill();
        if (Projectile.ai[0] <= 3f) {
            Projectile.ai[0] += 1f;
            return;
        }
        Projectile.velocity.Y = Projectile.velocity.Y + 0.075f;
        for (int i = 0; i < 3; i++) {
            int num4 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Projectile.ai[1] == 1f ? DustID.Ichor : 157, 0f, 0f, 100, default(Color), Main.rand.NextFloat(0.9f, 1.2f));
            Main.dust[num4].noGravity = true;
            Main.dust[num4].velocity *= 0.2f;
            Main.dust[num4].velocity += Projectile.velocity * 0.1f;
        }
        if (Main.rand.Next(8) == 0) {
            int dust = Dust.NewDust(Projectile.position, Projectile.width + 6, Projectile.height + 6, Projectile.ai[1] == 1f ? DustID.Ichor : 157, 0f, 0f, 100, default(Color), 1.5f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.25f;
            Main.dust[dust].fadeIn = 1f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (Main.rand.NextBool(4)) {
            target.AddBuff(BuffID.Poisoned, 450);
        }
    }
}
