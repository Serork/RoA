using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;

// for bestiary
sealed class DruidSoul2 : ModNPC {
    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(0f, -2f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = -21f,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!NPC.IsABestiaryIconDummy) {
            return false;
        }

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }

    public override void FindFrame(int frameHeight) {
        NPC.direction = 1;
        NPC.spriteDirection = -NPC.direction;
        double maxCounter = 6.0;
        if (++NPC.frameCounter >= maxCounter * (Main.npcFrameCount[Type] - 1)) {
            NPC.frameCounter = 0.0;
        }
        int currentFrame = (int)(NPC.frameCounter / maxCounter);
        NPC.frame.Y = currentFrame * frameHeight;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.DruidSoul")
        ]);
    }

    public override void SetDefaults() {
        int width = 28; int height = 50;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = AIType = -1;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        NPC.rarity = 1;
    }

    public override void AI() {
        NPC.KillNPC();
    }
}
