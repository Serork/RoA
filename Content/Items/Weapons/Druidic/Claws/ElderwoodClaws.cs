using RoA.Core;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class ElderwoodClaws : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);

        Item.SetWeaponValues(12, 3f);
    }
}
