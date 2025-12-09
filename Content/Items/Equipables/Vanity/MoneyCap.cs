using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class MoneyCap : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = false;
    }

    public override void SetDefaults() {
        Item.SetSizeValues(22, 16);

        Item.SetShopValues(ItemRarityColor.Blue1, Item.sellPrice(0, 1));

        Item.vanity = true;
    }
}
