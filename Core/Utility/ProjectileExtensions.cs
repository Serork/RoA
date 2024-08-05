using RoA.Content.Projectiles.Friendly;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static class ProjectileExtensions {
    public static bool IsDamageable(this Projectile projectile) => projectile.damage > 0;

    public static bool IsDruidic(this Projectile projectile) => projectile.ModProjectile is NatureProjectile;

    public static bool IsDruidic(this Projectile projectile, out NatureProjectile result) {
        if (projectile.ModProjectile is NatureProjectile natureProjectile) {
            result = natureProjectile;
            return true;
        }

        result = null;
        return false;
    }

    public static Player GetOwnerAsPlayer(this Projectile projectile) => Main.player[projectile.owner];
    public static bool IsOwnerMyPlayer(this Projectile projectile, Player player) => player.whoAmI == Main.myPlayer;

    public static T As<T>(this Projectile projectile) where T : ModProjectile => projectile.ModProjectile as T;
}
