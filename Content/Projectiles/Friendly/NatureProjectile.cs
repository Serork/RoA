using RoA.Common.Druid;
using RoA.Content.Items;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly;

abstract class NatureProjectile : ModProjectile {
    private float _wreathPointsFine;

    public bool ShouldApplyWreathPoints { get; protected set; } = true;

    // if the value is a negative number we get more wreath points
    public float WreathPointsFine {
        get => _wreathPointsFine;
        private set {
            _wreathPointsFine = Math.Clamp(value, -1f, 1f);
        }
    }

    public sealed override void OnSpawn(IEntitySource source) {
        float fillingRate = Projectile.GetOwnerAsPlayer().GetSelectedItem().GetGlobalItem<NatureWeaponHandler>().FillingRate;
        WreathPointsFine = fillingRate <= 1f ? 1f - fillingRate : -(fillingRate - 1f);
        SafeOnSpawn(source);
    }

    protected virtual void SafeOnSpawn(IEntitySource source) { }

    public sealed override void SetDefaults() {
        SafeSetDefaults();

        if (Projectile.IsDamageable() && Projectile.friendly) {
            Projectile.SetDefaultToDruidicProjectile();
        }

        SafeSetDefaults2();
    }

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafeSetDefaults2() { }
}
