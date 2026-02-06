using RoA.Common.Druid;
using RoA.Common.Items;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.HandsOn)]
sealed class GardeningGloves : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(26, 34);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDamage(DruidClass.Nature) += 0.1f;
        player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.1f;

        if (player.GetFormHandler().IsInADruidicForm) {
            return;
        }

        if (player.GetWreathHandler().IsFull1) {
            if (!player.GetCommon().IsGardeningGlovesEffectActive) {
                player.GetCommon().ActivateGardeningGloveEffect();
                player.GetCommon().IsGardeningGlovesEffectActive = true;
            }
        }
        else {
            player.GetCommon().IsGardeningGlovesEffectActive = false;
        }
    }
}
