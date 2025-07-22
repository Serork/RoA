using Terraria;

namespace RoA.Core.Utility.Extensions;

static partial class ProjectileExtensions {
    public static void SetFrameCount(this Projectile projectile, int count) => Main.projFrames[projectile.type] = count;
}
