using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Common.Druid.Claws;
using RoA.Content.Projectiles.Friendly.Druidic;
using RoA.Core;

using Terraria;

namespace RoA.Content.Items.Weapons.Druidic.Claws;

sealed class HorrorPincers : BaseClawsItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(26);

        NatureWeaponStats.SetPotentialDamage(Item, 24);

        Item.SetWeaponValues(8, 4f);
    }

    protected override (Color, Color) SlashColors() => (new Color(132, 75, 140), new Color(160, 100, 200));

    //public override void SafeOnUse(Player player, ClawsStats clawsStats) => clawsStats.SetSpecialAttackData<ClawsSlash>(Item, player.Center, Vector2.Zero);
}
