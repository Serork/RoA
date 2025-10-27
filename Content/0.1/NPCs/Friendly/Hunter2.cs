using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Friendly;

[Autoload(false)]
// for bestiary
sealed class Hunter2 : ModNPC {
    public override string Texture => NPCLoader.GetNPC(ModContent.NPCType<Hunter>()).Texture;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 26;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Velocity = 1f,
            Direction = -1
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.RemoveAt(3);
        bestiaryEntry.Info.RemoveAt(4);
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Hunter"),
            new BossBestiaryInfoElement()
        ]);
        bestiaryEntry.Info.RemoveAt(3);
    }

    public override void FindFrame(int frameHeight) {
        int num236 = 10;
        if (NPC.IsGrounded()) {
            if (NPC.direction == 1)
                NPC.spriteDirection = 1;

            if (NPC.direction == -1)
                NPC.spriteDirection = -1;

            int num237 = Main.npcFrameCount[Type] - NPCID.Sets.AttackFrameCount[Type];
            if (NPC.ai[0] == 23f) {

            }
            else if (NPC.ai[0] >= 20f && NPC.ai[0] <= 22f) {

            }
            else if (NPC.ai[0] == 2f) {

            }
            else if (NPC.velocity.X == 0f) {
                NPC.frame.Y = 0;
                NPC.frameCounter = 0.0;
            }
            else {
                int num287 = 6;

                NPC.frameCounter += 2.0;

                int num288 = frameHeight * 2;

                if (NPC.frame.Y < num288)
                    NPC.frame.Y = num288;

                if (NPC.frameCounter > (double)num287) {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0.0;
                }

                if (NPC.frame.Y / frameHeight >= Main.npcFrameCount[Type] - num236)
                    NPC.frame.Y = num288;
            }

            return;
        }

        NPC.frameCounter = 0.0;
        NPC.frame.Y = frameHeight;
    }

    public override void SetDefaults() {
        NPC.width = 18;
        NPC.height = 40;

        NPC.aiStyle = AIType = -1;

        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;

        NPC.friendly = false;

        NPC.boss = true;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        NPC.rarity = 5;
    }

    public override void AI() {
        NPC.KillNPC();
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            Main.BestiaryTracker.Kills.RegisterKill(NPC);
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Items.Weapons.Magic.FireLighter>()));
    }
}
