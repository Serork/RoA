using RoA.Common.Druid.Wreath;
using RoA.Core.Defaults;
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

        player.GetWreathHandler().IncreaseResourceValue(0f);
    }

    protected override void SafeSetDefaults() {
        Item.SetSizeValues(26, 24);
        Item.SetShopValues(Terraria.Enums.ItemRarityColor.LightRed4, Item.sellPrice());

        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDruidStats().IsDruidsEyesEffectActive = true;

        if (player.GetWreathHandler().IsFull1) {
            player.statDefense += 6;
            player.lifeRegen += 3;
        }
    }
}
