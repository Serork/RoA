using Microsoft.Xna.Framework.Input;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class NoLeatherDropFromNPCs : GlobalNPC {
    public override void ModifyGlobalLoot(GlobalLoot globalLoot) {
        globalLoot.RemoveWhere(rule => rule is CommonDrop drop && drop.itemId == ItemID.Leather);
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
        npcLoot.RemoveWhere(rule => rule is CommonDrop drop && drop.itemId == ItemID.Leather);
    }
}
