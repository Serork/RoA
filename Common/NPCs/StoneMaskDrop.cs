using RoA.Common.CustomConditions;
using RoA.Content.Items.Equipables.Vanity;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class StoneMaskDrop : GlobalNPC {
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.ByCondition(new BackwoodsDropCondition(), ModContent.ItemType<StoneMask>(), 350));
    }
}
