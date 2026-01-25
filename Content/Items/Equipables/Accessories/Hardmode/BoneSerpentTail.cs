using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

sealed class BoneSerpentTail : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(28, 22);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));

        Item.defense = 1;
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (player.lifeRegen < 0) {
            player.GetCritChance(DamageClass.Generic) += 12;
        }
    }
}
