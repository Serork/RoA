using RoA.Content.Projectiles.Friendly;

using Terraria;

namespace RoA.Core.Utility;

static class ProjectileExtensions {
    public static bool IsDamageable(this Projectile projectile) => projectile.damage > 0;

    public static bool IsDruidic(this Projectile projectile) => projectile.ModProjectile is NatureProjectile;
}
