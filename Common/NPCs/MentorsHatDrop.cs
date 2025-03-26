using RoA.Content.Items.Equipables.Vanity;

using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class MentorsHatDrop : GlobalNPC {
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
        if (npc.type != NPCID.ArmoredViking) {
            return;
        }

        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<MentorsHat>(), 50));
    }
}