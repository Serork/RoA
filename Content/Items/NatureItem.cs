using System;
using System.Collections.Generic;

using RoA.Core.Defaults;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items;

sealed class CrossmodNatureContent : ModSystem {
    internal static HashSet<int> natureItems = [], natureProjectiles = [];

    public static void RegisterNatureItem(Item item) => natureItems.Add(item.type);
    public static void RegisterNatureProjectile(Projectile projectile) => natureProjectiles.Add(projectile.type);

    public static void MakeProjectileDruidic(Projectile projectile) {
        RegisterNatureProjectile(projectile);
        projectile.MakeProjectileDruidic();
    }

    public static bool IsItemNature(Item item) => natureItems.Contains(item.type);
    public static bool IsProjectileNature(Projectile projectile) => natureProjectiles.Contains(projectile.type);

    public override void Unload() {
        natureItems.Clear();
        natureItems = null;

        natureProjectiles.Clear();
        natureProjectiles = null;
    }
}

abstract class NatureItem : ModItem {
    protected override bool CloneNewInstances => true;

    public sealed override void SetDefaults() {
        SafeSetDefaults();

        if (Item.IsAWeapon()) {
            Item.SetDefaultToDruidicWeapon();
        }

        SafeSetDefaults2();

        if (Item.IsAWeapon()) {
            //NatureWeaponHandler handler = Item.GetGlobalItem<NatureWeaponHandler>();
            //if (handler.HasPotentialDamage()) {
            //    int damageGap = handler.PotentialDamage - 2;
            //    if (Item.damage > damageGap) {
            //        Item.damage = damageGap;
            //    }
            //}
        }
    }

    protected virtual void SafeSetDefaults() { }
    protected virtual void SafeSetDefaults2() { }

    public virtual void WhileBeingHold(Player player, float progress) { }
}
