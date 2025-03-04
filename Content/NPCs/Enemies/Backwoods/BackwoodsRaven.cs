using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class BackwoodsRaven : ModNPC {
    public override string Texture => NPCLoader.GetNPC(ModContent.NPCType<SummonedRaven>()).Texture;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 5;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(2f, 1f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = -20f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.BackwoodsRaven")
        ]);
    }

    public override void FindFrame(int frameHeight) {
        int num83 = Main.npcFrameCount[NPC.type] - 1;
        if (NPC.frame.Y < frameHeight) {
            NPC.frame.Y = frameHeight;
        }
        NPC.frameCounter += 1.0;
        if (NPC.frameCounter >= 6.0) {
            NPC.frame.Y += frameHeight;
            NPC.frameCounter = 0.0;
        }

        if (NPC.frame.Y >= frameHeight * num83)
            NPC.frame.Y = frameHeight;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        NPC.spriteDirection = NPC.direction = 1;

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }

    public override void SetDefaults() {
        NPC.CloneDefaults(NPCID.Raven);

        NPC.width = 36;
        NPC.height = 26;

        NPC.value = 0;

        NPC.aiStyle = AIType = -1;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override void AI() {
        NPC.KillNPC();
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            Main.BestiaryTracker.Kills.RegisterKill(NPC);
        }
    }
}