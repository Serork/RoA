using Microsoft.Xna.Framework;

using RoA.Core;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class ThornyClaws : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);

        Item.SetWeaponValues(14, 4f);
    }

    protected override (Color, Color) SlashColors() => (new Color(75, 167, 85), new Color(100, 200, 110));
}
