using Microsoft.Xna.Framework;

using RoA.Core;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class HellfireClaws : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);
        Item.SetWeaponValues(16, 4.2f);
    }

    protected override (Color, Color) SlashColors() => (new Color(255, 150, 20), new Color(200, 80, 10));
}
