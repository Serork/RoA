using Microsoft.Xna.Framework;

using RoA.Common.Projectiles;

using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class IchorStream : NatureProjectile_NoTextureLoad {
    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldChargeWreath: true, shouldApplyAttachedItemDamage: false);

        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = true;
        Projectile.alpha = 255;
        Projectile.penetrate = -1;
        Projectile.extraUpdates = 1;
        Projectile.ignoreWater = true;
    }

    public override void AI() {
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;
            SoundEngine.PlaySound(SoundID.Item17, Projectile.position);
        }

        Projectile.scale -= 0.002f;
        if (Projectile.scale <= 0f)
            Projectile.Kill();

        Projectile.ai[0] = 4f;

        if (Projectile.ai[0] > 3f) {
            Projectile.velocity.Y += 0.075f;
            for (int num122 = 0; num122 < 3; num122++) {
                float num123 = Projectile.velocity.X / 3f * (float)num122;
                float num124 = Projectile.velocity.Y / 3f * (float)num122;
                int num125 = 14;
                int num126 = Dust.NewDust(new Vector2(Projectile.position.X + (float)num125, Projectile.position.Y + (float)num125), Projectile.width - num125 * 2, Projectile.height - num125 * 2, 170, 0f, 0f, 100);
                Main.dust[num126].noGravity = true;
                Dust dust2 = Main.dust[num126];
                dust2.velocity *= 0.1f;
                dust2 = Main.dust[num126];
                dust2.velocity += Projectile.velocity * 0.5f;
                Main.dust[num126].position.X -= num123;
                Main.dust[num126].position.Y -= num124;
            }

            if (Main.rand.Next(8) == 0) {
                int num127 = 16;
                int num128 = Dust.NewDust(new Vector2(Projectile.position.X + (float)num127, Projectile.position.Y + (float)num127), Projectile.width - num127 * 2, Projectile.height - num127 * 2, 170, 0f, 0f, 100, default(Color), 0.5f);
                Dust dust2 = Main.dust[num128];
                dust2.velocity *= 0.25f;
                dust2 = Main.dust[num128];
                dust2.velocity += Projectile.velocity * 0.5f;
            }
        }
        else {
            Projectile.ai[0] += 1f;
        }
    }

    public override void OnKill(int timeLeft) {
  
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = 10;
        height = 10;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(69, 900);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(69, 900);
    }
}
