using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using RoA.Core;
using Microsoft.Xna.Framework;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class LothorAngleAttack2 : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;
    public override bool PreDraw(ref Color lightColor) => false;

    public override void SetDefaults() {
        Projectile.aiStyle = 14;
        Projectile.width = 4;
        Projectile.height = 4;
        Projectile.timeLeft = 180;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.scale = 1.1f;
    }

    public override void AI() {
        if (Main.netMode != NetmodeID.Server) {
            if (Main.rand.NextBool(Projectile.localAI[2] == 10f ? 2 : 1)) {
                int dust = Dust.NewDust(Projectile.position + Projectile.velocity, 2, 2, DustID.PoisonStaff, 0f, -0.5f, 0, default, 1.35f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.5f;
            }
        }
        Vector2 lastVelocity = Projectile.velocity;
        if (Projectile.velocity.X != lastVelocity.X)
            Projectile.velocity.X = lastVelocity.X * -0.5f;
        if (Projectile.velocity.Y != lastVelocity.Y && lastVelocity.Y > 1f)
            Projectile.velocity.Y = lastVelocity.Y * -0.5f;

        Projectile.localAI[2] = 0f;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.velocity = Vector2.Zero;
        Projectile.localAI[2] = 10f;
        return false;
    }

    public override void OnKill(int timeLeft) {
        if (Main.netMode != NetmodeID.Server)
            for (int value = 0; value < 11 + Main.rand.Next(0, 5); value++) {
                int dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width,
                                        Projectile.height, DustID.PoisonStaff, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 0, default, 0.6f);
                Main.dust[dust].noGravity = true;
            }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        int time = 120;
        target.AddBuff(BuffID.Poisoned, time, true);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        int time = 120;
        target.AddBuff(BuffID.Poisoned, time, true);
    }
}
