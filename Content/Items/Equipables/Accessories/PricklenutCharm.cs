using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class PricklenutCharm : NatureItem {
    protected override void SafeSetDefaults() {
        Item.DefaultToAccessory(26, 34);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDruidStats().IsDruidsEyesEffectActive = (true, Common.Druid.DruidStats.DruidEyesType.PricklenutCharm);
    }
}
