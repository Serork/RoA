using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class NaturePriestHood : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(28, 28);

        Item.SetShopValues(ItemRarityColor.Blue1, Item.sellPrice(0, 1));

        Item.vanity = true;
    }
}
