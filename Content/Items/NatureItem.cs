using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items;

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
