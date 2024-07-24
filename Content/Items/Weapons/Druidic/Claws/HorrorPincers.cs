using Microsoft.Xna.Framework;

using RoA.Core;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class HorrorPincers : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);

        Item.SetWeaponValues(8, 4f);
    }

    protected override (Color, Color) SlashColors() => (new Color(132, 75, 140), new Color(160, 100, 200));
}
