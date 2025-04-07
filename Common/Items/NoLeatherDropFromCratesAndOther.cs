using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class NoLeatherDropFromCratesAndOther : GlobalItem {
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot) {
        itemLoot.RemoveWhere((itemDrop) => itemDrop is CommonDrop commonDrop && commonDrop.itemId == ItemID.Leather);
    }
}
