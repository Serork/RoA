using System.Collections.Generic;

using Terraria.GameContent.ItemDropRules;

namespace RoA.Common.Items.ItemDropRules;

sealed class AllOptionsNotScaledWithLuckDropRule : IItemDropRule {
    public int[] dropIds;
    public int chanceDenominator;
    public int chanceNumerator;

    public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }

    public AllOptionsNotScaledWithLuckDropRule(int chanceDenominator, int chanceNumerator, params int[] options) {
        this.chanceDenominator = chanceDenominator;
        dropIds = options;
        this.chanceNumerator = chanceNumerator;
        ChainedRules = new List<IItemDropRuleChainAttempt>();
    }

    public bool CanDrop(DropAttemptInfo info) => true;

    public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
        ItemDropAttemptResult result;
        if (info.rng.Next(chanceDenominator) < chanceNumerator) {
            foreach (int itemType in dropIds) {
                CommonCode.DropItem(info, itemType, 1);
            }
            result = default(ItemDropAttemptResult);
            result.State = ItemDropAttemptResultState.Success;
            return result;
        }

        result = default(ItemDropAttemptResult);
        result.State = ItemDropAttemptResultState.FailedRandomRoll;
        return result;
    }

    public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
        float num = (float)chanceNumerator / (float)chanceDenominator;
        float num2 = num * ratesInfo.parentDroprateChance;
        float dropRate = 1f / (float)dropIds.Length * num2;
        for (int i = 0; i < dropIds.Length; i++) {
            drops.Add(new DropRateInfo(dropIds[i], 1, 1, dropRate, ratesInfo.conditions));
        }

        Chains.ReportDroprates(ChainedRules, num, drops, ratesInfo);
    }
}
