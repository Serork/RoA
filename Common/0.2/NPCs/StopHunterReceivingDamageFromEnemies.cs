using RoA.Content.NPCs.Friendly;
using RoA.Core;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class StopHunterReceivingDamageFromEnemies : IInitializer {
    public void Load(Mod mod) {
        On_NPC.BeHurtByOtherNPC += On_NPC_BeHurtByOtherNPC;
    }

    private void On_NPC_BeHurtByOtherNPC(On_NPC.orig_BeHurtByOtherNPC orig, NPC self, int npcIndex, NPC thatNPC) {
        if (self.type == ModContent.NPCType<Hunter>()) {
            return;
        }

        orig(self, npcIndex, thatNPC);
    }
}
