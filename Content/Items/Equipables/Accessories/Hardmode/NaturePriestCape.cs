using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Accessories.Hardmode;

[AutoloadEquip(EquipType.Back, EquipType.Front)]
sealed class NaturePriestCape : ModItem {
    public override void SetDefaults() {
        Item.DefaultToAccessory(30, 30);

        Item.SetShopValues(ItemRarityColor.LightRed4, Item.sellPrice(0, 1));
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {

    }
}
