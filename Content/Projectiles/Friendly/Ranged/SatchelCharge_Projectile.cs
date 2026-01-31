using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class SatchelChargeProjectile : ModProjectile {
    public override string Texture => ResourceManager.RangedProjectileTextures + "SatchelCharge";

    public override void SetStaticDefaults() {
        Projectile.SetFrameCount(2);
    }

    public override void SetDefaults() {
        Projectile.SetSizeValues(22, 28);
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.friendly = true;
        Projectile.tileCollide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.penetrate = -1;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        Projectile.velocity *= 0.9f;

        return false;
    }

    public override void AI() {
        Player player = Projectile.GetOwnerAsPlayer();
        if (Projectile.localAI[0] == 0f) {
            Projectile.localAI[0] = 1f;

            Projectile.Center = player.GetPlayerCorePoint();
            int index = 0;
            int max = 15;
            while (!WorldGenHelper.SolidTileNoPlatform(Projectile.Center.ToTileCoordinates())) {
                index++;
                Projectile.Center += player.DirectionTo(player.GetViableMousePosition());
                if (index > max) {
                    break;
                }
            }

            Projectile.Center -= Projectile.velocity;
        }

        Projectile.velocity.X *= 0.9f;
        if ((double)Math.Abs(Projectile.velocity.X) < 0.1)
            Projectile.velocity.X = 0f;
        else {
            Projectile.SetDirection(-Projectile.velocity.X.GetDirection());
        }

        Projectile.velocity.Y = Projectile.velocity.Y + 0.4f;
        if (Projectile.velocity.Y > 16f) {
            Projectile.velocity.Y = 16f;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        Projectile.QuickDrawAnimated(lightColor * Projectile.Opacity);

        return false;
    }
}
