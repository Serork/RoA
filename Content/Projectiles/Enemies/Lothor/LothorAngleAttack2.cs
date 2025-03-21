using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Enemies.Lothor;

sealed class LothorAngleAttack2 : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;
    public override bool PreDraw(ref Color lightColor) => false;

    public override void SetDefaults() {
        Projectile.aiStyle = 14;
        Projectile.width = 12;
        Projectile.height = 12;
        Projectile.timeLeft = 180;
        Projectile.penetrate = -1;
        Projectile.tileCollide = true;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.scale = 1.1f;
        Projectile.light = 0f;
    }

    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
        width = height = 4;

        return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
    }

    public override void AI() {
        bool enraged = Projectile.ai[2] == 1f;

        if (Main.rand.NextBool(Projectile.localAI[2] == 10f ? 2 : 1)) {
            int dust = Dust.NewDust(Projectile.position + Projectile.velocity + Vector2.UnitY * 4f, 2, 2, enraged ? ModContent.DustType<LothorPoison2>() : ModContent.DustType<LothorPoison>(), 0f, -0.5f, 0, default, 1.35f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.5f;
            Main.dust[dust].noLight = Main.dust[dust].noLightEmittence = true;
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
        bool enraged = Projectile.ai[2] == 1f;

        for (int value = 0; value < 11 + Main.rand.Next(0, 5); value++) {
            int dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width,
                                    Projectile.height, enraged ? ModContent.DustType<LothorPoison2>() : ModContent.DustType<LothorPoison>(), Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 0, default, 0.6f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].noLight = Main.dust[dust].noLightEmittence = true;
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
