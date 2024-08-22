using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static class NPCExtensions {
    public static T As<T>(this NPC npc) where T : ModNPC => npc.ModNPC as T;

    public static bool NearestTheSame(this NPC NPC, out NPC npc2, int type = -1) {
        for (int i = 0; i < Main.npc.Length; i++) {
            NPC npc = Main.npc[i];
            if (i != NPC.whoAmI && npc.active && (npc.type == NPC.type || npc.type == type) && Math.Abs(NPC.position.X - npc.position.X) + Math.Abs(NPC.position.Y - npc.position.Y) < NPC.width) {
                npc2 = npc;
                return true;
            }
        }
        npc2 = null;
        return false;
    }

    public static void OffsetNPC(this NPC NPC, NPC npc, float offsetSpeed = 0.05f) {
        if (NPC.position.X < npc.position.X) {
            NPC.velocity.X -= offsetSpeed;
        }
        else {
            NPC.velocity.X += offsetSpeed;
        }
        if (NPC.position.Y < npc.position.Y) {
            NPC.velocity.Y -= offsetSpeed;
        }
        else {
            NPC.velocity.Y += offsetSpeed;
        }
    }

    public static void OffsetTheSameNPC(this NPC npc1, float offsetSpeed = 0.05f) {
        if (npc1.NearestTheSame(out NPC npc2)) {
            npc1.OffsetNPC(npc2, offsetSpeed);
        }
    }

    public static void KillNPC(this NPC npc) {
        npc.active = false;
        npc.life = -1;
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI, 0f, 0f, 0f, 0, 0, 0);
        }
    }
}
