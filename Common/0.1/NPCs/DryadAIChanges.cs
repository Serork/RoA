using RoA.Content.Emotes;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.NPCs;

sealed class DryadAIChanges : GlobalNPC {
    public override bool? CanChat(NPC npc) {
        if (npc.type == NPCID.Dryad) {
            return npc.ai[0] != -20f;
        }

        return base.CanChat(npc);
    }

    public override void FindFrame(NPC npc, int frameHeight) {
        if (npc.type != NPCID.Dryad) {
            return;
        }
        if (npc.ai[0] != -25f) {
            return;
        }
        int num237 = Main.npcFrameCount[npc.type] - NPCID.Sets.AttackFrameCount[npc.type];
        npc.localAI[2]++;
        int num269 = 0;
        num269 = (npc.localAI[2] % 16.0 < 8.0) ? (num237 - 2) : 0;
        npc.frame.Y = frameHeight * num269;
    }

    public override void AI(NPC npc) {
        if (npc.type != NPCID.Dryad) {
            return;
        }
        if (npc.ai[0] == -25f) {
            if (npc.ai[1] > 0f) {
                npc.ai[1] -= 1f;

                npc.velocity.X *= 0.8f;

                int dir = (Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].Center.X - npc.Center.X).GetDirection();
                npc.direction = npc.spriteDirection = dir;
            }
            else {
                npc.ai[1] = npc.ai[0] = 0f;
                npc.frameCounter = 0.0;
                npc.ai[2] = 0;
                npc.netUpdate = true;
            }
        }
        if (npc.ai[0] != -20f) {
            return;
        }
        if (npc.ai[1] > 0f) {
            npc.ai[1] -= 1;

            npc.velocity.X *= 0.8f;

            if (npc.ai[1] > 50f) {
                npc.ai[1] -= 1;
                npc.spriteDirection = npc.direction = (int)npc.ai[2];
            }
            else {
                if (npc.ai[1] == 49f) {
                    npc.direction *= -1;
                    npc.spriteDirection = npc.direction;
                }
            }
        }
        else {
            npc.ai[1] = 80;
            npc.ai[0] = -25f;
            npc.ai[2] = npc.localAI[2] = 0f;
            npc.frameCounter = 0.0;
            npc.netUpdate = true;

            int emoteType = ModContent.EmoteBubbleType<BackwoodsEmote>();
            EmoteBubble.NewBubble(emoteType, new WorldUIAnchor(npc), (int)npc.ai[1]);
        }
    }
}