using RoA.Content.NPCs.Friendly;
using RoA.Core.Utility;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class FireflyMimicActuallyMimic : GlobalNPC {
    public override void OnSpawn(NPC npc, IEntitySource source) {
        Player closestPlayer = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];
        if (npc.type == NPCID.Firefly && closestPlayer.RollLuck(25) == 0) {
            NPC.NewNPCDirect(source, npc.position, ModContent.NPCType<FireflyMimic>());
            npc.KillNPC();
        }
    }
}
