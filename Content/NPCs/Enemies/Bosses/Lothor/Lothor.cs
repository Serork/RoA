using Microsoft.Xna.Framework;

using RoA.Common.NPCs;
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
            //CustomTexturePath = ResourceManager.BestiaryTextures + "Lothor",
            Position = new Vector2(0f, 24f),
            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 0f,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Lothor")
        ]);
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
		NPC.lifeMax = (int)((float)NPC.lifeMax * 0.8f * balance * bossAdjustment);
        //NPC.damage = (int)((double)NPC.damage * 0.65f);

        NPC.defense += 4;
    }

    public override void SetDefaults() {
        NPC.damage = 45;
        NPC.lifeMax = 6000;
        NPC.defense = 15;

        NPC.Size = Vector2.One * 72f;

        NPC.aiStyle = AIType = -1;

        NPC.npcSlots = 9f;

        NPC.boss = true;

        NPC.HitSound = new SoundStyle(ResourceManager.NPCSounds + "LothorHit") { Volume = 0.8f };

        NPC.value = Item.buyPrice(gold: 6);

        if (!Main.dedServ) {
            Music = MusicLoader.GetMusicSlot(ResourceManager.Music + "Lothor");
        }

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override void Unload() => UnloadAnimations();

    partial void UnloadAnimations();

    public override void BossLoot(ref string name, ref int potionType) {
        potionType = ItemID.HealingPotion;
    }
}
