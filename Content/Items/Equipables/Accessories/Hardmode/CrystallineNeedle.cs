using RoA.Common.Druid.Wreath;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class CrystallineNeedle : NatureItem {
    public override void Load() {
        WreathHandler.OnHitByAnythingEvent += WreathHandler_OnHitByAnythingEvent1;
    }

    private void WreathHandler_OnHitByAnythingEvent1(Player player, Player.HurtInfo hurtInfo) {
        if (!player.GetDruidStats().IsCrystallineNeedleEffectActive) {
            return;
        }

        player.GetWreathHandler().ForcedHardReset(makeDusts: true);
    }

    protected override void SafeSetDefaults() {
        Item.DefaultToAccessory(34, 34);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDruidStats().IsCrystallineNeedleEffectActive = true;
    }
}
