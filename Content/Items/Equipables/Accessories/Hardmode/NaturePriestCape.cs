using RoA.Common.Druid;
using RoA.Common.Items;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.Back, EquipType.Front)]
sealed class NaturePriestCape : NatureItem {
    protected override void SafeSetDefaults() {
        Item.DefaultToAccessory(30, 30);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        player.GetDamage(DruidClass.Nature) += 0.1f;
        player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier += 0.1f;

        if (player.GetFormHandler().IsInADruidicForm) {
            return;
        }

        if (player.GetWreathHandler().IsFull1) {
            if (!player.GetCommon().IsNaturePriestCapeEffectActive) {
                player.GetCommon().ActivateNaturePriestCapeEffect();
                player.GetCommon().IsNaturePriestCapeEffectActive = true;
            }
        }
        else {
            player.GetCommon().IsNaturePriestCapeEffectActive = false;
        }
    }
}
