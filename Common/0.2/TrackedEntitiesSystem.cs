using RoA.Common.Networking;
using RoA.Content.Projectiles.Friendly.Nature;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
class TrackedAttribute : Attribute { }

sealed class TrackedEntitiesSystem : ModSystem {
    public static Dictionary<Type, List<Entity>> TrackedEntitiesByType { get; private set; } = [];

    public override void Load() {
        On_NPC.NewNPC += On_NPC_NewNPC;
        On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += On_Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float;
    }

    public static bool RegisterTrackedEntity<TEntity, TModType>(ModType<TEntity, TModType> modType) where TModType : ModType<TEntity, TModType> {
        Type projectileType = modType.GetType();
        TrackedAttribute? trackedAttribute = projectileType.GetCustomAttribute<TrackedAttribute>();

        if (trackedAttribute == null) {
            return false;
        }

        if (!projectileType.IsAbstract) {
            if (!TrackedEntitiesByType.TryGetValue(projectileType, out List<Entity>? Projectiles)) {
                Projectiles = [];
                TrackedEntitiesByType.Add(projectileType, Projectiles);
            }
            Entity? entityToRegister = modType is ModProjectile modProjectile ? modProjectile.Projectile : modType is ModNPC modNPC ? modNPC.NPC : null;
            if (entityToRegister == null) {
                throw new Exception("Not supported");
            }
            Projectiles.Add(entityToRegister);

            return true;
        }

        return false;
    }

    private int On_Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float(On_Projectile.orig_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float orig, IEntitySource spawnSource, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner, float ai0, float ai1, float ai2) {
        int whoAmI = orig(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
        if (Main.projectile[whoAmI].IsModded(out ModProjectile modProjectile)) {
            if (RegisterTrackedEntity(modProjectile)) {
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new RegisterTrackedProjectilePacket(Main.player[Owner], modProjectile.Projectile.identity));
                }
            }
        }

        return whoAmI;
    }

    private int On_NPC_NewNPC(On_NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start, float ai0, float ai1, float ai2, float ai3, int Target) {
        int whoAmI = orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);
        if (Main.npc[whoAmI].IsModded(out ModNPC modNPC)) {
            RegisterTrackedEntity(modNPC);
        }

        return whoAmI;
    }

    public static IEnumerable<Projectile> GetTrackedProjectile<T>(Predicate<Projectile>? filter = null) where T : ModProjectile {
        Type projectileType = typeof(T);
        if (TrackedEntitiesByType.TryGetValue(projectileType, out List<Entity>? value)) {
            foreach (Projectile trackedProjectile in value.Cast<Projectile>()) {
                if (filter != null && filter.Invoke(trackedProjectile)) {
                    continue;
                }
                yield return trackedProjectile;
            }
        }
    }

    public static IEnumerable<NPC> GetTrackedNPC<T>(Predicate<NPC>? filter = null) where T : ModNPC {
        Type npcType = typeof(T);
        if (TrackedEntitiesByType.TryGetValue(npcType, out List<Entity>? value)) {
            foreach (NPC trackedNPC in value.Cast<NPC>()) {
                if (filter != null && filter.Invoke(trackedNPC)) {
                    continue;
                }
                yield return trackedNPC;
            }
        }
    }

    public override void PostUpdateProjectiles() {
        foreach (List<Entity> trackedProjectiles in TrackedEntitiesByType.Values) {
            for (int i = trackedProjectiles.Count - 1; i >= 0; i--) {
                if (!trackedProjectiles[i].active) {
                    trackedProjectiles[i] = trackedProjectiles[^1];
                    trackedProjectiles.RemoveAt(trackedProjectiles.Count - 1);
                }
            }
        }
    }
}
