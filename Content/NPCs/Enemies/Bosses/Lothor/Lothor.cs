using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Core;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor;

[AutoloadBossHead]
sealed partial class Lothor : ModNPC {
    public sealed override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 1;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            CustomTexturePath = ResourceManager.BestiaryTextures + "Lothor",
            Position = new Vector2(0f, 24f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 12f,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
			new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Lothor")
        ]);
    }

    public override void SetDefaults() {
        NPC.damage = 46;
        NPC.lifeMax = 6000;
        NPC.defense = 8;

        NPC.Size = Vector2.One * 72f;

        NPC.aiStyle = AIType = -1;

        NPC.npcSlots = 10f;

        NPC.boss = true;

        NPC.HitSound = new SoundStyle(ResourceManager.NPCSounds + "LothorHit") { Volume = 0.8f };

        NPC.value = Item.buyPrice(gold: 5);

        if (!Main.dedServ) {
            Music = MusicLoader.GetMusicSlot(ResourceManager.Music + "Lothor");
        }

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override void Unload() => UnloadAnimations();

    partial void UnloadAnimations();
}
