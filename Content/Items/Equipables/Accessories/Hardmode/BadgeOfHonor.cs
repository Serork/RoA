using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class BadgeOfHonor : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(28, 38);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateEquip(Player player) {
        player.GetCommon().IsBadgeOfHonorEffectActive = true;
    }
}
