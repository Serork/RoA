using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class StarfruitCharm : NatureItem {
    protected override void SafeSetDefaults() {
        Item.DefaultToAccessory(30, 36);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDruidStats().IsDruidsEyesEffectActive = (true, Common.Druid.DruidStats.DruidEyesType.StarfruitCharm);

        player.GetDruidStats().IsStarfruitCharmEffectActive = true;

        player.longInvince = true;

        player.starCloakItem = null;
    }
}
