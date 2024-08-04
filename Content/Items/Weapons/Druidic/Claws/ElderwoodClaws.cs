using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Core;

using Terraria;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class ElderwoodClaws : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);
        Item.SetWeaponValues(12, 3f);
    }

    protected override (Color, Color) SlashColors() => (new(72, 86, 214), new(114, 126, 255));
}
