using System;

using Terraria;

namespace RoA.Core.Utility.Extensions;

static partial class ProjectileExtensions {
    public static bool SameAs(this Projectile projectile, Projectile checkProjectile) => projectile.whoAmI == checkProjectile.whoAmI;

    public static byte GetFrameCount(this Projectile projectile) => (byte)Main.projFrames[projectile.type];

    public static void SetFrameCount(this Projectile projectile, int count) => Main.projFrames[projectile.type] = count;

    public static void SetDirection(this Projectile projectile, int direction, bool setSpriteDirectionToo = true) {
        projectile.direction = direction;
        if (setSpriteDirectionToo) {
            projectile.spriteDirection = projectile.direction;
        }
    }

    public static bool NearestTheSame(this Projectile projectile, out Projectile checkProjectile, int type = -1) {
        for (int i = 0; i < Main.projectile.Length; i++) {
            Projectile projectile2 = Main.projectile[i];
            if (i != projectile.whoAmI && projectile2.active && (projectile2.type == projectile.type || type == projectile2.type) && Math.Abs(projectile.position.X - projectile2.position.X) + Math.Abs(projectile.position.Y - projectile2.position.Y) < projectile.width) {
                checkProjectile = projectile2;
                return true;
            }
        }
        checkProjectile = null;
        return false;
    }

    public static void OffsetTheSameProjectile(this Projectile projectile, Projectile checkProjectile, float offsetSpeed = 0.05f) {
        if (projectile.position.X < checkProjectile.position.X) {
            projectile.velocity.X -= offsetSpeed;
        }
        else {
            projectile.velocity.X += offsetSpeed;
        }
        if (projectile.position.Y < checkProjectile.position.Y) {
            projectile.velocity.Y -= offsetSpeed;
        }
        else {
            projectile.velocity.Y += offsetSpeed;
        }
    }

    public static void OffsetTheSameProjectile(this Projectile checkProjectile, float offsetSpeed = 0.05f) {
        if (checkProjectile.NearestTheSame(out Projectile projectile)) {
            checkProjectile.OffsetTheSameProjectile(projectile, offsetSpeed);
        }
    }
}
