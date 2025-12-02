using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class PerfectedConcoction : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(18, 30);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetCommon().ExtraManaFromStarsModifier *= 1.5f;
        player.GetCommon().ExtraLifeFromHeartsModifier *= 1.5f;
    }
}
