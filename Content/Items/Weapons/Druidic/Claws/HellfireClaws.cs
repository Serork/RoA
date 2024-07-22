using RoA.Core;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class HellfireClaws : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);

        Item.SetWeaponValues(16, 4.2f);
    }
}
