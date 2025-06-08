using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static class ProjectileHelper {
    public static void Animate(Projectile projectile, int frameCounter) {
        if (++projectile.frameCounter >= frameCounter) {
            projectile.frameCounter = 0;
            if (++projectile.frame >= Main.projFrames[projectile.type]) {
                projectile.frame = 0;
            }
        }
    }

    public static void SpawnPlayerOwnedNoDamageProjectile<T>(Player player, IEntitySource source, Vector2? position = null, Vector2? velocity = null) where T : ModProjectile {
        Projectile.NewProjectile(source, position ?? player.Center, velocity ?? Vector2.Zero, ModContent.ProjectileType<T>(), 0, 0f, player.whoAmI);
    }
}
