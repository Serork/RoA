using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class MaidensBracers : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(32, 34);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDamage(DamageClass.Generic) += 0.15f;

        player.GetCommon().IsMaidensBracersEffectActive = true;
    }
}
