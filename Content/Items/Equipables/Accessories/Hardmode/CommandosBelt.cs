using RoA.Common.Players;
using RoA.Common.UI;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class CommandosBelt : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(34, 32);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDamage(DamageClass.Ranged) += 0.1f;
        player.GetModPlayer<RangedArmorSetPlayer>().AllAmmoConsumptionReduce += 0.1f;
        player.GetModPlayer<RangedArmorSetPlayer>().ExtraCustomAmmoAmount += 1;
    }
}
