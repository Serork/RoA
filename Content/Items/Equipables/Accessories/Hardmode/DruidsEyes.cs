using RoA.Common.Druid;
using RoA.Common.Druid.Wreath;
using RoA.Core.Defaults;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using Terraria;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class DruidsEyes : NatureItem {
    public override void Load() {
        WreathHandler.OnHitByAnythingEvent += WreathHandler_OnHitByAnythingEvent1;
    }

    private void WreathHandler_OnHitByAnythingEvent1(Player player, Player.HurtInfo hurtInfo) {
        if (!player.GetDruidStats().IsDruidsEyesEffectActive) {
            return;
        }

        player.GetWreathHandler().IncreaseResourceValue(-MathUtils.Clamp01((hurtInfo.Damage / (float)player.statLifeMax2) * 3.5f), extra2: 1.5f);
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26, 26);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightRed4, Item.sellPrice());

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDruidStats().IsDruidsEyesEffectActive = true;

        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.2f;

        if (player.GetWreathHandler().IsFull1) {
            //player.statDefense += 6;
            player.lifeRegen += 6;
        }
    }
}
