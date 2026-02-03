using RoA.Common.Druid;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class PinCushion : NatureItem {
    protected override void SafeSetDefaults() {
        Item.DefaultToAccessory(38, 34);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDruidStats().IsDruidsEyesEffectActive = (true, DruidStats.DruidEyesType.PinCushion);

        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.2f;
        if (player.GetWreathHandler().IsFull1) {
            player.lifeRegen += 6;
        }

        player.GetDruidStats().IsCrystallineNeedleEffectActive = true;
    }
}
