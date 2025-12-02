using Microsoft.Xna.Framework;

using RoA.Content.Dusts;
using RoA.Core;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Melee;

sealed class OvergrownBolt : ModProjectile {
    public override string Texture => ResourceManager.EmptyTexture;

    private sbyte _dustSpread;
    private sbyte _dustAmount = 2;

    public override void SetDefaults() {
        int width = 32; int height = width;
        Projectile.Size = new Vector2(width, height);

        Projectile.DamageType = DamageClass.Melee;

        Projectile.friendly = true;
        Projectile.ignoreWater = true;

        Projectile.penetrate = 5;
        Projectile.extraUpdates = 2;

        Projectile.alpha = 0;
        Projectile.timeLeft = 150;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
    }

    public override void AI() {
        Projectile.tileCollide = Projectile.timeLeft < 120;
        Projectile.velocity *= 0.97f;
        if (Projectile.timeLeft <= 125 && Projectile.timeLeft > 100) {
            _dustAmount = 4;
            _dustSpread += 2;
        }
        else if (Projectile.timeLeft <= 100 && Projectile.timeLeft > 50) _dustSpread--;
        else if (Projectile.timeLeft == 50) _dustAmount = 2;

        for (int i = 0; i < _dustAmount; i++) {
            float velX = Projectile.velocity.X / 3f * i;
            float velY = Projectile.velocity.Y / 3f * i;
            int dust = Dust.NewDust(new Vector2(Projectile.Center.X - 3 - _dustSpread / 2, Projectile.Center.Y - 3 - _dustSpread / 2), 6 + _dustSpread, 6 + _dustSpread, ModContent.DustType<OvergrownSpearDust>(), 0f, -6f, 120, default(Color), Main.rand.NextFloat(0.8f, 1.6f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.1f;
            Main.dust[dust].velocity += Projectile.velocity * 0.5f;
            Dust dust2 = Main.dust[dust];
            dust2.position.X = dust2.position.X - velX;
            Dust dust3 = Main.dust[dust];
            dust3.position.Y = dust3.position.Y - velY;
        }
    }
}