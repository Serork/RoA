using RoA.Common.Items;
using RoA.Content;
using RoA.Core.Utility;

using Terraria;

namespace RoA.Core.Defaults;

static class ProjectileDefaults {
    public static void SetSizeValues(this Projectile projectile, int width, int height) {
        projectile.width = width;
        projectile.height = height;
    }

    public static void SetSizeValues(this Projectile projectile, int width) => projectile.width = projectile.height = width;

    public static void SetDefaultsToNatureProjectile(this Projectile projectile) => projectile.DamageType = DruidClass.Nature;

    public static void MakeProjectileNature(this Projectile projectile) {
        if (projectile.IsDamageable() && projectile.friendly) {
            projectile.SetDefaultsToNatureProjectile();
        }
    }
}
