using RoA.Common.Druid;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.HandsOn)]
sealed class GardeningGloves : NatureItem {
    protected override void SafeSetDefaults() {
        Item.DefaultToAccessory(26, 34);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.1f;
        player.GetCommon().IsGardeningGlovesEffectActive = true;
    }
}
