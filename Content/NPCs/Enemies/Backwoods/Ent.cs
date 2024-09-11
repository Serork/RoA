using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class Ent : RoANPC {
	private int ParentNPCIndex => (int)StateTimer;
	private NPC Parent => Main.npc[ParentNPCIndex];

	public override void SetStaticDefaults() {
		Main.npcFrameCount[Type] = 18;
	}

	public override void SetDefaults() {
		NPC.lifeMax = 600;
		NPC.damage = 36;
		NPC.defense = 6;
		NPC.knockBackResist = 0f;

		int width = 80; int height = 100;
		NPC.Size = new Vector2(width, height);

		NPC.aiStyle = -1;

		NPC.npcSlots = 1f;

		NPC.value = Item.buyPrice(0, 0, 6, 7);

		NPC.noTileCollide = true;
		NPC.noGravity = true;

		NPC.HitSound = SoundID.NPCHit52;
		NPC.DeathSound = SoundID.NPCDeath27;
	}

	public override void AI() {
		NPC.realLife = NPC.whoAmI;

		NPC npc = Parent;
		if (!npc.active) {
			NPC.KillNPC();
		}
		NPC.defense = npc.defense;
		NPC.Center = npc.Center - Vector2.UnitY * 32f;
		NPC.velocity = npc.velocity;
	}

	public override void OnKill() => Parent.KillNPC();

	public override void FindFrame(int frameHeight) {
		NPC npc = Parent;
		if (!npc.active) {
			return;
		}
        NPC.direction = npc.direction;
		NPC.spriteDirection = -NPC.direction;
		ChangeFrame(((int)(npc.ModNPC as RoANPC).CurrentFrame, frameHeight));
	}

	//public override void ModifyNPCLoot(NPCLoot npcLoot)
	//	=> npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NaturesHeart>(), 2));

	//public override void HitEffect (NPC.HitInfo hit) {
	//	if (Main.netMode == NetmodeID.Server) {
	//		return;
	//	}
	//	if (NPC.life <= 0) {
	//		for (int i = 0; i < Main.rand.Next(3); i++) {
	//			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.Find<ModGore>(nameof(RiseofAges) + "/EntGore1").Type, 1f);
	//		}
	//		for (int i = 0; i < 3; i++) {
	//			Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, ModContent.Find<ModGore>(nameof(RiseofAges) + "/EntGore" + (i + 1).ToString()).Type, 1f);
	//		}
	//		for (int i = 0; i < Main.rand.Next(5, 16); i++) {
	//			Item.NewItem(NPC.GetSource_Loot(), (int) NPC.position.X, (int) NPC.position.Y, NPC.width, NPC.height, ModContent.ItemType<Elderwood>());
	//		}
	//	} else {
	//		if (Main.rand.NextBool(4))
	//			Item.NewItem(NPC.GetSource_Loot(), (int) NPC.position.X, (int) NPC.position.Y, NPC.width, NPC.height, ModContent.ItemType<Elderwood>());
	//	}
	//}
}