using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class NoLeatherDropFromCratesAndOther : GlobalItem {
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot) {
        itemLoot.RemoveWhere((itemDrop) => itemDrop is IItemDropRule && itemDrop is CommonDropNotScalingWithLuck drop && drop.itemId == ItemID.Leather);
    }
}