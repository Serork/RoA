using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class FermentedSpiderEye : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(22, 28);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().IsFermentedSpiderEyeEffectActive = true;

        player.GetDruidStats().ClawsResetDecreaseModifier *= 0.7f;
    }
}
