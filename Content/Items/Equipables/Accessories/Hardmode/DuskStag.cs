using RoA.Common.Druid;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class DuskStag : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(30, 36);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.2f;
        if (player.GetWreathHandler().IsFull1) {
            player.lifeRegen += 3;
        }
        player.GetModPlayer<DruidStats>().KeepBonusesForTime += 120f;
    }
}
