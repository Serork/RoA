using RoA.Content;
using RoA.Core.Utility;

using Terraria;

namespace RoA.Core.Defaults;

static class ProjectileDefaults {
    public static void SetDefaultsToNatureProjectile(this Projectile projectile) => projectile.DamageType = DruidClass.NatureDamage;

    public static void MakeProjectileNature(this Projectile projectile) {
        if (projectile.IsDamageable() && projectile.friendly) {
            projectile.SetDefaultsToNatureProjectile();
        }
    }
}
