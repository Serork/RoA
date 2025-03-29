using Microsoft.Xna.Framework;

using RoA.Content.Items.Miscellaneous;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
namespace RoA.Content.NPCs.Enemies.Miscellaneous;

sealed partial class PettyGoblin : ModNPC {
    private class ExtraGoblinTinkererQuote : GlobalNPC {
        public override void GetChat(NPC npc, ref string chat) {
            if (npc.type != NPCID.GoblinTinkerer) {
                return;
            }

            bool flag7 = false;
            bool flag15 = false;
            for (int i = 0; i < 200; i++) {
                if (Main.npc[i].active) {
                    if (Main.npc[i].type == NPCID.Mechanic)
                        flag7 = true;
                    else if (Main.npc[i].type == NPCID.Stylist)
                        flag15 = true;
                }
            }

            object obj = Lang.CreateDialogSubstitutionObject(npc);
            string result = string.Empty;
            if (npc.homeless) {
                int max = 6;
                switch (Main.rand.Next(max)) {
                    case 0:
                        result = Lang.dialog(121);
                        break;
                    case 1:
                        result = Lang.dialog(122);
                        break;
                    case 2:
                        result = Lang.dialog(123);
                        break;
                    case 3:
                        result = Lang.dialog(124);
                        break;
                    case 4:
                        if (Main.rand.NextBool(max)) {
                            result = Language.GetTextValue($"Mods.RoA.NPCs.Town.GoblinTinkerer.PettyGoblinQuote1");
                        }
                        else {
                            result = Lang.dialog(124);
                        }
                        break;
                    default:
                        result = Lang.dialog(125);
                        break;
                }
            }
            else if (npc.HasSpecialEventText("GoblinTinkerer", out string specialEventText)) {
                result = specialEventText;
            }
            else if (flag7 && Main.rand.Next(5) == 0) {
                result = Lang.dialog(126);
            }
            else if (flag15 && Main.rand.Next(5) == 0) {
                result = Lang.dialog(309);
            }
            else {
                LocalizedText[] array2 = Language.FindAll(Lang.CreateDialogFilter("GoblinTinkererChatter.", obj));
                int num4 = Main.rand.Next(array2.Length + 5);
                if (Main.rand.NextBool(array2.Length + 5)) {
                    result = Language.GetTextValue($"Mods.RoA.NPCs.Town.GoblinTinkerer.PettyGoblinQuote1");
                }
                else if (num4 >= 5) {
                    result = array2[num4 - 5].FormatWith(obj);
                }
                else if (!Main.dayTime) {
                    switch (num4) {
                        case 0:
                            result = Lang.dialog(127);
                            break;
                        case 1:
                            result = Lang.dialog(128);
                            break;
                        case 2:
                            result = Lang.dialog(129);
                            break;
                        case 3:
                            result = Lang.dialog(130);
                            break;
                        default:
                            result = Lang.dialog(131);
                            break;
                    }
                }
                else {
                    switch (num4) {
                        case 0:
                            result = Lang.dialog(132);
                            break;
                        case 1:
                            result = Lang.dialog(133);
                            break;
                        case 2:
                            result = Lang.dialog(134);
                            break;
                        case 3:
                            result = Lang.dialog(135);
                            break;
                        default:
                            result = Lang.dialog(136);
                            break;
                    }
                }
            }

            chat = result;
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
        int width = 28; int height = 48;
        NPC.Size = new Vector2(width, height);

        NPC.damage = 15;
        NPC.defense = 4;
        NPC.lifeMax = 90;

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
