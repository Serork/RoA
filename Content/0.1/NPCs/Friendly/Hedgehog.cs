using Microsoft.Xna.Framework;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core.Utility;

using System.IO;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Friendly;

sealed class Hedgehog : ModNPC {
    private bool alert;
    private bool chosen;
    private int choice;
    private bool _playerNearby;

    private float _curlUpTimer;

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Hedgehog");
        Main.npcFrameCount[NPC.type] = 5;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Velocity = 1f
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        NPCID.Sets.ShimmerTransformToNPC[Type] = NPCID.Shimmerfly;
    }

    public override void SetDefaults() {
        NPC.CloneDefaults(NPCID.Bunny);
        NPC.damage = 0;
        NPC.dontCountMe = true;
        NPC.catchItem = (short)ModContent.ItemType<Items.Miscellaneous.Hedgehog>();
        NPC.aiStyle = 7;

        AIType = NPCID.Bunny;

        Banner = Type;
        BannerItem = ModContent.ItemType<HedgehogBanner>();

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override bool? CanBeCaughtBy(Item item, Player player) => !alert;

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Hedgehog")
        ]);
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(chosen);
        writer.Write(choice);
        writer.Write(_playerNearby);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        chosen = reader.ReadBoolean();
        choice = reader.ReadInt32();
        _playerNearby = reader.ReadBoolean();
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        => alert;

    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life > 0) {
            for (int num493 = 0; (double)num493 < hit.Damage / (double)NPC.lifeMax * 20.0; num493++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num494 = 0; num494 < 10; num494++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f);
        }

        if (!Main.dedServ) {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "HedgehogGore1".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "HedgehogGore2".GetGoreType());
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = NPC.direction;
        if (alert) NPC.frame.Y = frameHeight * 4;
        else {
            NPC.frameCounter++;
            if (NPC.frameCounter < 5 || NPC.velocity.X == 0)
                NPC.frame.Y = 0;
            else if (NPC.frameCounter < 10)
                NPC.frame.Y = frameHeight * 1;
            else if (NPC.frameCounter < 15)
                NPC.frame.Y = frameHeight * 2;
            else if (NPC.frameCounter < 20)
                NPC.frame.Y = frameHeight * 3;
            else
                NPC.frameCounter = 0;
        }
    }

    public override void AI() {
        if (alert) {
            //Main.npcCatchable[NPC.type] = false;
            NPC.dontCountMe = false;
        }
        //else Main.npcCatchable[NPC.type] = true;
        NPC.dontCountMe = true;

        bool flag = _curlUpTimer >= 20f;
        if (!flag) {
            _curlUpTimer++;
        }

        if (!_playerNearby) {
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                foreach (Player player in Main.ActivePlayers) {
                    if (!player.dead && Vector2.Distance(player.position, NPC.position) <= 300) {
                        _playerNearby = true;

                        NPC.target = player.whoAmI;

                        NPC.netUpdate = true;

                        break;
                    }
                }
            }
        }

        if (flag && _playerNearby) {
            if (!chosen && Main.netMode != NetmodeID.MultiplayerClient) {
                choice = Main.rand.Next(0, 2);
                chosen = true;
                NPC.netUpdate = true;
            }
            if (choice == 0) alert = false;
            if (choice == 1 && !alert) {
                NPC.rotation += 0.3f * NPC.velocity.X;
                NPC.damage = 10;
                alert = true;
                NPC.aiStyle = 0;
            }
            if (Vector2.Distance(Main.player[NPC.target].position, NPC.position) > 300) {
                _playerNearby = false;
            }
        }
        else {
            chosen = false;
            NPC.rotation = 0;
            NPC.aiStyle = 7;
            NPC.damage = 0;
            alert = false;
        }
    }
}