using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.Shield)]
sealed class BansheesGuard : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(28, 32);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));

        Item.defense = 15;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsBansheesGuardEffectActive = true;
    }
}
