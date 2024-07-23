using RoA.Core;

using Terraria.ID;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

[Weapon(WeaponType.Claws)]
abstract class BaseClawsItem : NatureItem {
    protected sealed override void SafeSetDefaults2() {
        Item.noMelee = true;

        Item.SetDefaultToUsable(ItemUseStyleID.Swing, 18, false);
    }
}
