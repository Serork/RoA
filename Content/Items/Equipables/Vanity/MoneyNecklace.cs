using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Neck)]
sealed class MoneyNecklace : ModItem {
    public override void SetStaticDefaults() {

    }

    public override void SetDefaults() {
        Item.SetSizeValues(28, 24);

        Item.SetShopValues(ItemRarityColor.Blue1, Item.sellPrice(0, 1));

        Item.accessory = true;

        Item.vanity = true;
    }
}
