using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Equipables.Accessories.Hardmode;

using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class AddExtraEmblemInWOFTreasureBag : GlobalItem {
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot) {
        if (item.type == ItemID.WallOfFleshBossBag) {
            foreach (IItemDropRule rule in itemLoot.Get()) {
                if (rule is OneFromOptionsNotScaledWithLuckDropRule oneFromOptionsDrop && oneFromOptionsDrop.dropIds.Contains(ItemID.RangerEmblem)) {
                    List<int> original = oneFromOptionsDrop.dropIds.ToList();
                    original.Add(ModContent.ItemType<DruidEmblem>());
                    oneFromOptionsDrop.dropIds = [.. original];
                }
            }
        }
    }
}
