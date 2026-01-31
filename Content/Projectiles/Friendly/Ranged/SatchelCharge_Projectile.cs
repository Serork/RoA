using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Defaults;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Ranged;

sealed class SatchelChargeProjectile : ModProjectile {
    public override string Texture => ResourceManager.RangedProjectileTextures + "SatchelCharge";

    public override void SetDefaults() {
        Projectile.SetSizeValues(10);
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.friendly = true;
        Projectile.tileCollide = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;

        Projectile.penetrate = -1;
    }

    public override bool OnTileCollide(Vector2 oldVelocity) {
        return false;
    }

    public override void AI() {
        Projectile.velocity.Y = Projectile.velocity.Y + 0.4f;
        if (Projectile.velocity.Y > 16f) {
            Projectile.velocity.Y = 16f;
        }
    }
}
