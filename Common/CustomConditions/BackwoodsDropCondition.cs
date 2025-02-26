using RoA.Content.Biomes.Backwoods;

using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace RoA.Common.CustomConditions;

public class BackwoodsDropCondition : IItemDropRuleCondition, IProvideItemConditionDescription {
    public bool CanDrop(DropAttemptInfo info) {
        return info.player.InModBiome<BackwoodsBiome>();
    }

    public bool CanShowItemDropInUI() {
        return false;
    }

    public string GetConditionDescription() => Language.GetOrRegister("Mods.RoA.Conditions.InBackwoods").Value;
}