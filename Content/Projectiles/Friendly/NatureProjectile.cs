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

    public bool ShouldIncreaseWreathPoints { get; protected set; } = true;

    public float WreathPointsFine {
        get => _wreathPointsFine;
        private set {
            _wreathPointsFine = Math.Clamp(value, -1f, 1f);
        }
    }

    public sealed override void OnSpawn(IEntitySource source) {
        if (Projectile.owner == Main.myPlayer) {
            Player player = Projectile.GetOwnerAsPlayer();
            float fillingRate = player.GetSelectedItem().GetGlobalItem<NatureWeaponHandler>().GetFillingRate(player);
            WreathPointsFine = fillingRate <= 1f ? 1f - fillingRate : -(fillingRate - 1f);
        }
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
