using Microsoft.Xna.Framework;

using RoA.Core.Defaults;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class IceShard : ModProjectile {
    private static ushort MAXTIMELEFT => 160;

    public override void SetDefaults() {
        Projectile.SetSizeValues(8);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = MAXTIMELEFT;

        Projectile.friendly = true;
        Projectile.penetrate = 1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI() {
        if (Projectile.velocity.Y >= 1f) {
            Projectile.velocity.X *= 0.98f;
        }

        Projectile.velocity.Y += 0.1f;

        if (Math.Abs(Projectile.velocity.X) <= 1f) {
            Projectile.Opacity -= 0.05f;
            if (Projectile.Opacity <= 0f) {
                Projectile.Kill();
            }
        }

        Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
    }
}
