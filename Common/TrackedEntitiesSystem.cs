using RoA.Common.Networking;
using RoA.Common.Networking.Packets;
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

namespace RoA.Common;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
class TrackedAttribute : Attribute { }

sealed class TrackedEntitiesSystem : ModSystem {
    public static Dictionary<Type, List<Entity>> TrackedEntitiesByType { get; private set; } = [];

    public override void Load() {
        On_NPC.NewNPC += On_NPC_NewNPC;
        On_Projectile.NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float += On_Projectile_NewProjectile_IEntitySource_float_float_float_float_int_int_float_int_float_float_float;
    }

    public override void Unload() {
        TrackedEntitiesByType.Clear();
        TrackedEntitiesByType = null!;
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
        RegisterTrackedProjectile(Main.projectile[whoAmI]);

        return whoAmI;
    }

    public static void RegisterTrackedProjectile(Projectile projectile) {
        if (projectile.IsModded(out ModProjectile modProjectile)) {
            if (RegisterTrackedEntity(modProjectile)) {
                if (Main.netMode == NetmodeID.MultiplayerClient) {
                    MultiplayerSystem.SendPacket(new RegisterTrackedProjectilePacket(Main.player[projectile.owner], modProjectile.Projectile.identity));
                }
            }
        }
    }

    private int On_NPC_NewNPC(On_NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start, float ai0, float ai1, float ai2, float ai3, int Target) {
        int whoAmI = orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);
        RegisterTrackedNPC(Main.npc[whoAmI]);

        return whoAmI;
    }

    public static void RegisterTrackedNPC(NPC npc) {
        if (npc.IsModded(out ModNPC modNPC)) {
            RegisterTrackedEntity(modNPC);
        }
    }

    public static IEnumerable<Projectile> GetTrackedProjectile<T>(Predicate<Projectile>? filter = null, bool checkForType = true) where T : ModProjectile {
        Type projectileType = typeof(T);
        if (TrackedEntitiesByType.TryGetValue(projectileType, out List<Entity>? value)) {
            foreach (Projectile trackedProjectile in value.Cast<Projectile>()) {
                if (checkForType && trackedProjectile.type != ModContent.ProjectileType<T>()) {
                    continue;
                }
                if (filter != null && filter.Invoke(trackedProjectile)) {
                    continue;
                }
                yield return trackedProjectile;
            }
        }
    }

    public static IEnumerable<NPC> GetTrackedNPC<T>(Predicate<NPC>? filter = null, bool checkForType = true) where T : ModNPC {
        Type npcType = typeof(T);
        if (TrackedEntitiesByType.TryGetValue(npcType, out List<Entity>? value)) {
            foreach (NPC trackedNPC in value.Cast<NPC>()) {
                if (checkForType && trackedNPC.type != ModContent.NPCType<T>()) {
                    continue;
                }
                if (filter != null && filter.Invoke(trackedNPC)) {
                    continue;
                }
                yield return trackedNPC;
            }
        }
    }

    public static NPC GetSingleTrackedNPC<T>(Predicate<NPC>? filter = null, bool checkForType = true) where T : ModNPC => GetTrackedNPC<T>(filter, checkForType).ToList()[0];
    public static Projectile? GetSingleTrackedProjectile<T>(Predicate<Projectile>? filter = null, bool checkForType = true) where T : ModProjectile {
        IEnumerable<Projectile> trackedProjectiles = GetTrackedProjectile<T>(filter, checkForType);
        if (!trackedProjectiles.Any()) {
            return null;
        }
        return trackedProjectiles.ToList()[0];
    }

    public override void PostUpdateEverything() {
        UpdateTrackedEntityLists();
    }

    public static void UpdateTrackedEntityLists() {
        foreach (List<Entity> trackedEntities in TrackedEntitiesByType.Values) {
            for (int i = trackedEntities.Count - 1; i >= 0; i--) {
                Entity trackedEntity = trackedEntities[i];
                if (!trackedEntity.active) {
                    trackedEntities[i] = trackedEntities[^1];
                    trackedEntities.RemoveAt(trackedEntities.Count - 1);
                }
            }
        }
    }
}
