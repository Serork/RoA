using RoA.Content;

using Terraria;

namespace RoA.Core;

static class ProjectileDefaults {
    public static void SetDefaultToDruidicProjectile(this Projectile projectile) => projectile.DamageType = DruidClass.NatureDamage;
}
