using RoA.Content.Items.Miscellaneous;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class ReplaceLeatherDropFromNPCs : GlobalNPC {
    public override void ModifyGlobalLoot(GlobalLoot globalLoot) {
        //globalLoot.RemoveWhere(rule => rule is CommonDrop drop && drop.itemId == ItemID.Leather);
        foreach (var rule in globalLoot.Get()) {
            if (rule is CommonDrop drop && drop.itemId == ItemID.Leather) {
                drop.itemId = ModContent.ItemType<AnimalLeather>();
                drop.amountDroppedMinimum = drop.amountDroppedMaximum = 1;
                bool flag = false;
                if (drop.chanceNumerator == 1 && drop.chanceDenominator == 1) {
                    drop.chanceDenominator = 2;
                    flag = true;
                }
                if (drop.chanceNumerator == 1 && drop.chanceDenominator > 1) {
                    drop.chanceDenominator *= 2;
                    flag = true;
                }
                if (!flag && drop.chanceNumerator > 1 && drop.chanceDenominator > 1) {
                    drop.chanceNumerator /= 2;
                }
            }
        }
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
        //npcLoot.RemoveWhere(rule => rule is CommonDrop drop && drop.itemId == ItemID.Leather);
        foreach (var rule in npcLoot.Get()) {
            if (rule is CommonDrop drop && drop.itemId == ItemID.Leather) {
                drop.itemId = ModContent.ItemType<AnimalLeather>();
                drop.amountDroppedMinimum = drop.amountDroppedMaximum = 1;
                bool flag = false;
                if (drop.chanceNumerator == 1 && drop.chanceDenominator == 1) {
                    drop.chanceDenominator = 2;
                    flag = true;
                }
                if (drop.chanceNumerator == 1 && drop.chanceDenominator > 1) {
                    drop.chanceDenominator *= 2;
                    flag = true;
                }
                if (!flag && drop.chanceNumerator > 1 && drop.chanceDenominator > 1) {
                    drop.chanceNumerator /= 2;
                }
            }
        }
    }
}
