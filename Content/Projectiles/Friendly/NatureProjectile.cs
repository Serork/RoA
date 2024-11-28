using Microsoft.Xna.Framework;

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

    internal Item Item { get; private set; } = null;

    public bool ShouldIncreaseWreathPoints { get; protected set; } = true;

    public float WreathPointsFine {
        get => _wreathPointsFine;
        private set {
            _wreathPointsFine = Math.Clamp(value, -1f, 1f);
        }
    }

    private void SetItem(Item item) {
        if (Projectile.owner == Main.myPlayer) {
            Player player = Projectile.GetOwnerAsPlayer();
            Item = item;
            float fillingRate = Item.GetGlobalItem<NatureWeaponHandler>().GetFillingRate(player);
            WreathPointsFine = fillingRate <= 1f ? 1f - fillingRate : -(fillingRate - 1f);
        }
    }

    public static void CreateNatureProjectile(IEntitySource spawnSource, Item item, Vector2 position, Vector2 velocity, int Type, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f) {
        CreateNatureProjectile(spawnSource, item, position.X, position.Y, velocity.X, velocity.Y, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
    }

    public static void CreateNatureProjectile(IEntitySource spawnSource, Item item, float X, float Y, float SpeedX, float SpeedY, int Type, int Damage, float KnockBack, int Owner = -1, float ai0 = 0f, float ai1 = 0f, float ai2 = 0f) {
        int projectile = Projectile.NewProjectile(spawnSource, X, Y, SpeedX, SpeedY, Type, Damage, KnockBack, Owner, ai0, ai1, ai2);
        Main.projectile[projectile].As<NatureProjectile>().SetItem(item);
    }

    public sealed override void OnSpawn(IEntitySource source) {
        SetItem(Projectile.GetOwnerAsPlayer().GetSelectedItem());
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
