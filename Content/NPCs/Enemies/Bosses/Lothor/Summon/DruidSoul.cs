using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;

sealed partial class DruidSoul : RoANPC {
    private const float MAXDISTANCETOALTAR = 300f;

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.DruidSoul")
        ]);
    }

    public override void SetDefaults() {
        NPC.lifeMax = 10;

        int width = 28; int height = 50;
        NPC.Size = new Vector2(width, height);

        NPC.noTileCollide = true;

        NPC.friendly = true;
        NPC.noGravity = true;

        NPC.immortal = NPC.dontTakeDamage = true;

        NPC.aiStyle = AIType = -1;

        NPC.npcSlots = 10f;

        NPC.rarity = 1;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        NPC.catchItem = ModContent.ItemType<Items.Consumables.DruidSoul>();
    }

    public override bool? CanBeCaughtBy(Item item, Player player) {
        return item.type == ItemID.GoldenBugNet || item.type == ItemID.FireproofBugNet;
    }
}