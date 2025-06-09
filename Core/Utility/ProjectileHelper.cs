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

    public ref struct SpawnProjectileArgs(Player player, IEntitySource source) {
        public Player Player = player;
        public IEntitySource Source = source;
        public Vector2? Position = null;
        public Vector2? Velocity = null;
        public int Damage = 0;
        public float KnockBack = 0f;
        public float AI0 = 0f;
        public float AI1 = 0f;
        public float AI2 = 0f;
    }

    public static Projectile SpawnPlayerOwnedProjectile<T>(in SpawnProjectileArgs spawnProjectileArgs) where T : ModProjectile {
        Player player = spawnProjectileArgs.Player;
        return Main.projectile[Projectile.NewProjectile(spawnProjectileArgs.Source, spawnProjectileArgs.Position ?? player.Center, spawnProjectileArgs.Velocity ?? Vector2.Zero, ModContent.ProjectileType<T>(), spawnProjectileArgs.Damage, spawnProjectileArgs.KnockBack, player.whoAmI, spawnProjectileArgs.AI0, spawnProjectileArgs.AI1, spawnProjectileArgs.AI2)];
    }
}
