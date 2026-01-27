using RoA.Core.Defaults;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Equipables.Vanity;

[AutoloadEquip(EquipType.Head)]
sealed class HazelnutMask : ModItem {
    public override void SetStaticDefaults() {
        ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
    }

    public override void SetDefaults() {
        Item.SetSizeValues(24, 24);

        Item.SetShopValues(ItemRarityColor.Blue1, Item.sellPrice(0, 1));

        Item.vanity = true;
    }
}
