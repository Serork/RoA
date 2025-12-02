using Terraria.GameContent.ItemDropRules;

namespace RoA.Common.Items.ItemDropRules;

sealed class CustomDropRules {
    public static IItemDropRule AllOptionsNotScaledWithLuck(int chanceDenominator, params int[] options) => new AllOptionsNotScaledWithLuckDropRule(chanceDenominator, 1, options);
}
