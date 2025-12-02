using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

// for bestiary
sealed class Lothor2 : ModNPC {
    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 1;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(0f, -2f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = -2f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Lothor2")
        ]);
    }

    public override void SetDefaults() {
        NPC.width = 40; NPC.height = 46;

        NPC.aiStyle = AIType = -1;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        NPC.rarity = 5;
    }

    public override void AI() {
        NPC.KillNPC();
    }
}
