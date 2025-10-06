using Microsoft.Xna.Framework;

using RoA.Common.Druid;
using RoA.Core.Defaults;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Weapons.Nature.Hardmode.Claws;

[WeaponOverlay(WeaponType.Claws)]
sealed class SolScorchers : ClawsBaseItem {
    public override bool IsHardmodeClaws => true;

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(34, 36);
        Item.SetWeaponValues(30, 4.2f);

        Item.rare = ItemRarityID.LightRed;

        Item.value = Item.sellPrice(0, 2, 50, 0);

        Item.SetUsableValues(ItemUseStyleID.Swing, 18, false, autoReuse: true);

        NatureWeaponHandler.SetPotentialDamage(Item, 34);
        NatureWeaponHandler.SetFillingRateModifier(Item, 1f);
    }

    protected override (Color, Color) SlashColors(Player player) => (new Color(255, 221, 71), new Color(175, 152, 49));
}
