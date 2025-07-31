using Terraria;

namespace RoA.Core.Utility.Extensions;

static partial class ProjectileExtensions {
    public static void SetFrameCount(this Projectile projectile, int count) => Main.projFrames[projectile.type] = count;

    public static void SetDirection(this Projectile projectile, int direction, bool setSpriteDirectionToo = true) {
        projectile.direction = direction;
        if (setSpriteDirectionToo) {
            projectile.spriteDirection = projectile.direction;
        }
    }
}
