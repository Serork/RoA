using Terraria;

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
}
