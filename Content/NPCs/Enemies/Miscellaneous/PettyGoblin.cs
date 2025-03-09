using Microsoft.Xna.Framework;

using RoA.Content.Items.Miscellaneous;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace RoA.Content.NPCs.Enemies.Miscellaneous;

sealed partial class PettyGoblin : ModNPC {
    private sealed class ExtraGoblinTinkererQuote : GlobalNPC {
        public override void GetChat(NPC npc, ref string chat) {
            if (npc.type != NPCID.GoblinTinkerer) {
                return;
            }
            if (!Main.rand.NextBool(10)) {
                return;
            }

            chat = Language.GetTextValue($"Mods.RoA.NPCs.Town.GoblinTinkerer.PettyGoblinQuote{/*Main.rand.NextBool().ToInt() + */1}");
        }
    }


    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = FRAMES_COUNT;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(0f, 0f),

            PortraitPositionXOverride = 0f,
            PortraitPositionYOverride = 0f,

            Velocity = 1.5f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
    }

    public override void SetDefaults() {
        NPC.width = 20;
        NPC.height = 46;

        NPC.damage = 15;
        NPC.defense = 4;
        NPC.lifeMax = 70;

        NPC.HitSound = SoundID.DD2_GoblinBomberHurt;
        NPC.DeathSound = SoundID.DD2_GoblinBomberScream;
        NPC.value = Item.buyPrice(silver: 5, copper: 75);
        NPC.knockBackResist = 0.5f;
        NPC.aiStyle = -1;
        NPC.rarity = 2;

        Banner = Type;
        BannerItem = ModContent.ItemType<PettyGoblinBanner>();
        ItemID.Sets.KillsToBanner[BannerItem] = 10;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo) {
        return !NPC.AnyNPCs(Type) && NPC.downedGoblins ? SpawnCondition.Overworld.Chance * 0.025f : 0f;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) => npcLoot.Add(new CommonDrop(ModContent.ItemType<PettyBag>(), 10, 1, 1, 4));

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.PettyGoblin")
        ]);
    }


    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life > 0) {
            for (int num828 = 0; (double)num828 < hit.Damage / (double)NPC.lifeMax * 100.0; num828++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num829 = 0; num829 < 50; num829++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2.5f * (float)hit.HitDirection, -2.5f);
        }

        if (!Main.dedServ) {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "PettyGoblinGore1".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "PettyGoblinGore2".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "PettyGoblinGore3".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "PettyGoblinGore4".GetGoreType(), Scale: NPC.scale);
        }
    }
}
