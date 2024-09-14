using System;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Core.Utility;

static class NPCExtensions {
    public static T As<T>(this NPC npc) where T : ModNPC => npc.ModNPC as T;

    public static void PseudoGolemAI(this NPC npc, float maxSpeed = 1f, float speed = 0.035f) {
        npc.aiStyle = 3;
        npc.ModNPC.AIType = 243;
        npc.ai[2] = 0f;
        float num87 = maxSpeed * 3f;
        float num88 = speed;
        num87 += (1f - (float)npc.life / (float)npc.lifeMax) * 1.5f * 0.1f;
        num88 += (1f - (float)npc.life / (float)npc.lifeMax) * 0.15f * 0.1f;
        if (npc.velocity.X < 0f - num87 || npc.velocity.X > num87) {
            if (npc.velocity.Y == 0f)
                npc.velocity *= 0.7f;
        }
        else if (npc.velocity.X < num87 && npc.direction == 1) {
            npc.velocity.X += num88;
            if (npc.velocity.X > num87)
                npc.velocity.X = num87;
        }
        else if (npc.velocity.X > 0f - num87 && npc.direction == -1) {
            npc.velocity.X -= num88;
            if (npc.velocity.X < 0f - num87)
                npc.velocity.X = 0f - num87;
        }
        Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
    }

    public static void ResetAIStyle(this NPC npc) {
        npc.aiStyle = 0;
        npc.ModNPC.AIType = -1;
    }

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
