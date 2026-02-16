using RoA.Common.Players;
using RoA.Common.UI;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories;

sealed class AmmoBelt : ModItem, IRangedItemForVisuals {
    public override void SetDefaults() {
        Item.DefaultToAccessory(30, 30);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<RangedArmorSetPlayer>().AllAmmoConsumptionReduce += 0.1f;
        player.GetModPlayer<RangedArmorSetPlayer>().ExtraCustomAmmoAmount += 1;
    }
}
