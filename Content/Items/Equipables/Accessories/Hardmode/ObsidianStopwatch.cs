using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.Waist)]
sealed class ObsidianStopwatch : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(32, 32);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (player.GetFormHandler().IsInADruidicForm) {
            return;
        }

        player.GetCommon().IsObsidianStopwatchEffectActive = true;
        player.GetCommon().IsObsidianStopwatchEffectActive_Hidden = hideVisual;
    }
}
