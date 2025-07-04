using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
class ProjectileTrackedAttribute<T> : Attribute where T : ModProjectile { }

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
class ProjectileTrackedAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
class NPCTrackedAttribute<T> : Attribute where T : ModNPC { }

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
class NPCTrackedAttribute : Attribute { }

sealed class TrackedEntitiesSystem : ModSystem {
    public static Dictionary<Type, List<Entity>> TrackedEntitiesByType { get; private set; } = [];

    public override void Load() {
        On_NPC.NewNPC += On_NPC_NewNPC;
        On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += On_Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float;
    }

    private int On_Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float(On_Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig, IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2) {
        int whoAmI = orig(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
        if (Main.projectile[whoAmI].IsModded(out ModProjectile modProjectile)) {
            Type projectileType = modProjectile.GetType();
            ProjectileTrackedAttribute? trackedAttribute = projectileType.GetCustomAttribute<ProjectileTrackedAttribute>();

            if (trackedAttribute == null) {
                return whoAmI;
            }

            if (!projectileType.IsAbstract) {
                if (!TrackedEntitiesByType.TryGetValue(projectileType, out List<Entity>? Projectiles)) {
                    Projectiles = [];
                    TrackedEntitiesByType.Add(projectileType, Projectiles);
                }
                Projectiles.Add(modProjectile.Projectile);
            }
        }

        return whoAmI;
    }

    private int On_NPC_NewNPC(On_NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start, float ai0, float ai1, float ai2, float ai3, int Target) {
        int whoAmI = orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);
        if (Main.npc[whoAmI].IsModded(out ModNPC modNPC)) {
            Type npcType = modNPC.GetType();
            NPCTrackedAttribute? trackedAttribute = npcType.GetCustomAttribute<NPCTrackedAttribute>();

            if (trackedAttribute == null) {
                return whoAmI;
            }

            if (!npcType.IsAbstract) {
                if (!TrackedEntitiesByType.TryGetValue(npcType, out List<Entity>? NPCs)) {
                    NPCs = [];
                    TrackedEntitiesByType.Add(npcType, NPCs);
                }
                NPCs.Add(modNPC.NPC);
            }
        }

        return whoAmI;
    }

    public static int SpawnTrackedProjectile<T>(IEntitySource spawnSource, Vector2 position, Vector2 velocity, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f) where T : ModProjectile => SpawnTrackedProjectile<T>(spawnSource, position.X, position.Y, velocity.X, velocity.Y, Damage, KnockBack, Owner, ai0, ai1, ai2);

    public static int SpawnTrackedProjectile<T>(IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f) where T : ModProjectile {
        ushort type = (ushort)ModContent.ProjectileType<T>();
        int whoAmI = Projectile.NewProjectile(spawnSource, X, Y, SpeedX, SpeedY, type, Damage, KnockBack, Owner, ai0, ai1, ai2);

        Projectile projectile = Main.projectile[whoAmI];

        Type projectileType = typeof(T);
        ProjectileTrackedAttribute<T>? trackedAttribute = projectileType.GetCustomAttribute<ProjectileTrackedAttribute<T>>();

        if (trackedAttribute == null) {
            return whoAmI;
        }

        if (!projectileType.IsAbstract) {
            if (!TrackedEntitiesByType.TryGetValue(projectileType, out List<Entity>? projectiles)) {
                projectiles = [];
                TrackedEntitiesByType.Add(projectileType, projectiles);
            }
            projectiles.Add(projectile);
        }

        return whoAmI;
    }

    public static IEnumerable<Projectile> GetTrackedProjectile<T>(int checkOwnerWhoAmI) where T : ModProjectile {
        Type projectileType = typeof(T);
        if (TrackedEntitiesByType.TryGetValue(projectileType, out List<Entity>? value)) {
            foreach (Projectile trackedProjectile in value.Cast<Projectile>()) {
                if (trackedProjectile.owner != checkOwnerWhoAmI) {
                    continue;
                }
                yield return trackedProjectile;
            }
        }
    }

    public static IEnumerable<NPC> GetTrackedNPC<T>() where T : ModNPC {
        Type npcType = typeof(T);
        if (TrackedEntitiesByType.TryGetValue(npcType, out List<Entity>? value)) {
            foreach (NPC trackedNPC in value.Cast<NPC>()) {
                yield return trackedNPC;
            }
        }
    }

    public override void PostUpdateProjectiles() {
        foreach (List<Entity> trackedProjectiles in TrackedEntitiesByType.Values) {
            for (int i = 0; i < trackedProjectiles.Count; i++) {
                if (!trackedProjectiles[i].active) {
                    trackedProjectiles.RemoveAt(i);
                }
            }
        }
    }
}
