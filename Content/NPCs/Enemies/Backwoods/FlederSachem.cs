using RoA.Content.NPCs.Enemies.Backwoods;
using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RiseofAges.Content.NPCs.Backwoods;

sealed class FlederSachem : ModNPC {
	public override string Texture => ResourceManager.EmptyTexture;

	public override void SetDefaults() {
		NPC.lifeMax = 80;
		NPC.width = NPC.height = 2;

		NPC.aiStyle = -1;
		NPC.npcSlots = 1.5f;
	}

	private ref float Spawn  => ref NPC.ai[0];

	public override void AI() {
		if (Spawn <= 0f) {
			Spawn = 1f;
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int main = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Fleder>(), NPC.whoAmI);
				if (Main.netMode == NetmodeID.Server && main < Main.maxNPCs) {
					NetMessage.SendData(MessageID.SyncNPC, number: main);
				}
				for (int i = 0; i < Main.rand.Next(Main.expertMode ? 3 : 1, Main.expertMode ? 3 : 5); i++) {
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