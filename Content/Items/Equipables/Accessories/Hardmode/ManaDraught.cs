using RoA.Common.UI;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class ManaDraught : ModItem, IMagicItemForVisuals {
    public override void SetDefaults() {
        Item.DefaultToAccessory(26, 26);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().ExtraManaFromStarsModifier *= 1.5f;
    }
}
