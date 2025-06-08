﻿using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BloodRain : NatureProjectile {
    protected override void SafeSetDefaults() {
        Projectile.ignoreWater = true;
        Projectile.width = 4;
        Projectile.height = 40;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.penetrate = 2;
        Projectile.timeLeft = 300;
        Projectile.scale = 1.1f;
        Projectile.extraUpdates = 1;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;
    }

    public override void AI() {
        int num359 = (int)(Projectile.Center.X / 16f);
        int num360 = (int)((Projectile.position.Y + (float)Projectile.height) / 16f);
        if (WorldGen.InWorld(num359, num360) && Main.tile[num359, num360] != null && Main.tile[num359, num360].LiquidAmount == byte.MaxValue && Main.tile[num359, num360].LiquidType == LiquidID.Shimmer && Projectile.velocity.Y > 0f) {
            Projectile.velocity.Y *= -1f;
            Projectile.netUpdate = true;
        }

        Projectile.alpha = 100;
    }

    public override void OnKill(int timeLeft) {
        if (Projectile.velocity.Y > 0f) {
            int num503 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y + (float)Projectile.height - 2f), 2, 2, 114);
            Main.dust[num503].noGravity = true;
            Main.dust[num503].position.X -= 2f;
            Main.dust[num503].alpha = 38;
            Dust dust2 = Main.dust[num503];
            dust2.velocity *= 0.1f;
            dust2 = Main.dust[num503];
            dust2.velocity += -Projectile.oldVelocity * 0.25f;
            Main.dust[num503].scale = 0.95f;
        }
    }
}
