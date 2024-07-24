using Microsoft.Xna.Framework;

using RoA.Core;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class GutwrenchingHooks : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);

        Item.SetWeaponValues(8, 4f);
    }

    protected override (Color, Color) SlashColors() => (new Color(216, 73, 73), new Color(255, 114, 114));
}
