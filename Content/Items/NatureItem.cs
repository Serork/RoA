using System;
using System.Collections.Generic;

using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items;

sealed class NatureItems : ModSystem {
    internal static HashSet<int> natureItems = [];

    public static void RegisterNatureItem(ModItem modItem) {
        int type = modItem.Item.type;
        natureItems.Add(type);
    }

    public static bool IsItemNature(ModItem modItem) => modItem is not null && natureItems.Contains(modItem.Item.type);

    public override void Unload() {
        natureItems.Clear();
        natureItems = null;
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
