using RoA.Content.NPCs.Enemies.Bosses.Lothor;

using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;

namespace RoA.Common.CustomConditions;

sealed class FlederSlayerDropCondition : IItemDropRuleCondition {
    public bool CanDrop(DropAttemptInfo info) => info.npc.ModNPC is Lothor lothor && lothor.CanDropFlederSlayer;

    public bool CanShowItemDropInUI() => true;

    public string GetConditionDescription() => Language.GetOrRegister("Mods.RoA.Conditions.FlederSlayer").Value;
}