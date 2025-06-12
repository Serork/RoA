using RoA.Content.Biomes.Backwoods;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace RoA.Common.CustomConditions;

sealed class BackwoodsKeyCondition : IItemDropRuleCondition, IProvideItemConditionDescription {
    public bool CanDrop(DropAttemptInfo info) {
        if (info.npc.value > 0f && Main.hardMode && !info.IsInSimulation)
            return info.player.InModBiome<BackwoodsBiome>();

        return false;
    }

    public bool CanShowItemDropInUI() => true;
    public string GetConditionDescription() => Language.GetOrRegister("Mods.RoA.Conditions.BackwoodsKeyCondition").Value;
}