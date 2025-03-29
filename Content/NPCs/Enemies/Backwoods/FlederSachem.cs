using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class FlederSachem : ModNPC {
    public override string Texture => ResourceManager.EmptyTexture;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

    public override void SetStaticDefaults() {
        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Hide = true
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifier);
    }

    public override void SetDefaults() {
        NPC.lifeMax = 80;
        NPC.width = NPC.height = 2;

        NPC.aiStyle = -1;
        NPC.npcSlots = 1.5f;
    }

    private ref float Spawn => ref NPC.ai[0];

    public override void AI() {
        if (Spawn <= 0f) {
            Spawn = 1f;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                int main = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Fleder>(), NPC.whoAmI);
                Main.npc[main].npcSlots = 1.25f;
                Main.npc[main].netUpdate = true;
                if (Main.netMode == NetmodeID.Server && main < Main.maxNPCs) {
                    NetMessage.SendData(MessageID.SyncNPC, number: main);
                }
                for (int i = 0; i < Main.rand.Next(Main.expertMode ? 2 : 1, Main.expertMode ? 3 : 4); i++) {
                    int npc = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<BabyFleder>(), NPC.whoAmI);
                    NPC babyFleder = Main.npc[npc];
                    if (babyFleder.ModNPC is BabyFleder fleder) {
                        fleder.ParentIndex = Main.npc[main].whoAmI;
                        babyFleder.netUpdate = true;
                        NPC.netUpdate = true;
                    }
                    if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
                        NetMessage.SendData(MessageID.SyncNPC, number: npc);
                    }
                }
            }
            NPC.KillNPC();
        }
    }
}