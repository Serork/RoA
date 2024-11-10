using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class CrowdRaven : ModNPC {
	public override void SetStaticDefaults() {
		//base.SetStaticDefaults();

		// DisplayName.SetDefault("Summoned Raven");
		Main.npcFrameCount[Type] = 5;

		//NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
		//	CustomTexturePath = nameof(RiseofAges) + "/Assets/Textures/Bestiary/SummonedRaven",
		//	Position = new Vector2(0f, -16f),
		//	Velocity = 0f,
		//	Frame = 1
		//};
		//NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
	}

	public override void SetDefaults() {
		NPC.CloneDefaults(NPCID.Bird);

        NPC.width = 36;
        NPC.height = 26;

        NPC.friendly = false;

        //SpawnModBiomes = new int[] {
        //	ModContent.GetInstance<BackwoodsBiome>().Type
        //};

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override bool? CanFallThroughPlatforms() => true;

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = -NPC.direction;
        NPC.rotation = NPC.velocity.X * 0.1f;
        if (NPC.velocity.X == 0f && NPC.velocity.Y == 0f) {
            NPC.frame.Y = 0;
            NPC.frameCounter = 0.0;
            return;
        }

        int num83 = Main.npcFrameCount[NPC.type] - 1;
        NPC.frameCounter += 1.0;
        if (NPC.frameCounter >= 4.0) {
            NPC.frame.Y += frameHeight;
            NPC.frameCounter = 0.0;
        }

        if (NPC.frame.Y >= frameHeight * num83)
            NPC.frame.Y = frameHeight;
    }
}