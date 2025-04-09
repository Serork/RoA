using RoA.Content.Buffs;

using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

using Terraria.ModLoader;

namespace RoA.Common.CustomConditions;

sealed class SkinningDropCondition : IItemDropRuleCondition {
    public bool CanDrop(DropAttemptInfo info) {
        if (!info.IsInSimulation) {
            return info.player.FindBuffIndex((ushort)ModContent.BuffType<Skinning>()) != -1;
        }

        return false;
    }

    public bool CanShowItemDropInUI() => true;

    public string GetConditionDescription() => Language.GetOrRegister("Mods.RoA.Conditions.TanningRack").Value;
}