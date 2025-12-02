using Microsoft.Xna.Framework;

using RoA.Core;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature.Forms;

sealed class LilPhoenixFlames : FormProjectile {
    public override string Texture => ResourceManager.EmptyTexture;
    public override bool PreDraw(ref Color lightColor) => false;

    protected override void SafeSetDefaults() {
        Projectile.aiStyle = 14;
        Projectile.width = 4;
        Projectile.height = 4;
        Projectile.timeLeft = 260;
        Projectile.penetrate = 6;
        Projectile.tileCollide = true;
        Projectile.friendly = true;
        Projectile.scale = 1.1f;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override void ModifyDamageHitbox(ref Rectangle hitbox) {
        hitbox.Inflate(4, 4);
    }

    public override void AI() {
        if (Main.rand.NextBool(Projectile.localAI[2] == 10f ? 2 : 1)) {
            int dust = Dust.NewDust(Projectile.position + Projectile.velocity, 2, 2, 6, 0f, -0.5f, 0, default, 2f);
            Main.dust[dust].noGravity = true;
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
        for (int value = 0; value < 11 + Main.rand.Next(0, 5); value++) {
            int dust = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width,
                                    Projectile.height, 6, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 0, default, 0.6f);
            Main.dust[dust].noGravity = true;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        target.AddBuff(BuffID.OnFire, Main.rand.Next(90, 181));
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) {
        target.AddBuff(BuffID.OnFire, Main.rand.Next(90, 181));
    }
}
