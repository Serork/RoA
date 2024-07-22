using RoA.Core;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class ThornyClaws : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);

        Item.SetWeaponValues(14, 4f);
    }
}
