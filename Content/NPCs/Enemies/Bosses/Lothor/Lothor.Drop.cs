using RoA.Common.CustomConditions;

using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

sealed partial class Lothor : ModNPC {
    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(RoAConditions.ShouldDropFlederSlayer);
    }
}
