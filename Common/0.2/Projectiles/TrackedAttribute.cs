using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Reflection;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
class TrackedAttribute<T> : Attribute where T : ModProjectile { }

sealed class TrackedProjectilesSystem : ModSystem {
    public static Dictionary<Type, List<Projectile>> TrackedProjectilesByType { get; private set; } = [];

    public static int SpawnTrackedProjectile<T>(IEntitySource spawnSource, Vector2 position, Vector2 velocity, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f) where T : ModProjectile => SpawnTrackedProjectile<T>(spawnSource, position.X, position.Y, velocity.X, velocity.Y, Damage, KnockBack, Owner, ai0, ai1, ai2);

    public static int SpawnTrackedProjectile<T>(IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f) where T : ModProjectile {
        ushort type = (ushort)ModContent.ProjectileType<T>();
        int whoAmI = Projectile.NewProjectile(spawnSource, X, Y, SpeedX, SpeedY, type, Damage, KnockBack, Owner, ai0, ai1, ai2);

        Projectile projectile = Main.projectile[whoAmI];

        Type projectileType = typeof(T);
        TrackedAttribute<T>? trackedAttribute = projectileType.GetCustomAttribute<TrackedAttribute<T>>();

        if (trackedAttribute == null) {
            return whoAmI;
        }

        if (!projectileType.IsAbstract) {
            if (!TrackedProjectilesByType.TryGetValue(projectileType, out List<Projectile>? projectiles)) {
                projectiles = [];
                TrackedProjectilesByType.Add(projectileType, projectiles);
            }
            projectiles.Add(projectile);
        }

        return whoAmI;
    }

    public static IEnumerable<Projectile> GetTrackedProjectile<T>(int checkOwnerWhoAmI) where T : ModProjectile {
        foreach (Projectile trackedProjectile in TrackedProjectilesByType[typeof(T)]) {
            if (trackedProjectile.owner != checkOwnerWhoAmI) {
                continue;
            }
            yield return trackedProjectile;
        }
    }

    public override void PostUpdateProjectiles() {
        foreach (List<Projectile> trackedProjectiles in TrackedProjectilesByType.Values) {
            for (int i = 0; i < trackedProjectiles.Count; i++) {
                if (!trackedProjectiles[i].active) {
                    trackedProjectiles.RemoveAt(i);
                }
            }
        }
    }
}
