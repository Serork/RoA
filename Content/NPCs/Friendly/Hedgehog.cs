using Microsoft.Xna.Framework;

using System.IO;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Friendly;

sealed class Hedgehog : ModNPC {
    private bool alert;
    private bool chosen;
    private int choice;

    private float _curlUpTimer;

    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Hedgehog");
        Main.npcFrameCount[NPC.type] = 5;
    }

    public override void SetDefaults() {
        NPC.CloneDefaults(NPCID.Bunny);
        NPC.damage = 0;
        NPC.dontCountMe = true;
        NPC.catchItem = (short)ModContent.ItemType<Items.Miscellaneous.Hedgehog>();
        NPC.npcSlots = 0;
        NPC.aiStyle = 7;
        AIType = NPCID.Bunny;
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(alert);
        writer.Write(chosen);
        writer.Write(choice);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        alert = reader.ReadBoolean();
        chosen = reader.ReadBoolean();
        choice = reader.ReadInt32();
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        => alert;

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
            Main.npcCatchable[NPC.type] = false;
            NPC.dontCountMe = false;
        }
        else Main.npcCatchable[NPC.type] = true;
        NPC.dontCountMe = true;

        bool flag = _curlUpTimer >= 20f;
        if (!flag) {
            _curlUpTimer++;
        }

        bool flag2 = false;
        foreach (Player player in Main.ActivePlayers) {
            if (!player.dead && Vector2.Distance(player.position, NPC.position) <= 300) {
                flag2 = true;

                break;
            }
        }

        if (flag && flag2) {
            if (!chosen) {
                choice = Main.rand.Next(0, 2);
                chosen = true;
                NPC.netUpdate = true;
            }
            if (choice == 0) alert = false;
            if (choice == 1) {
                NPC.rotation += 0.3f * NPC.velocity.X;
                NPC.damage = 10;
                alert = true;
                NPC.aiStyle = 0;
            }
            NPC.netUpdate = true;
        }
        else {
            chosen = false;
            NPC.rotation = 0;
            NPC.aiStyle = 7;
            NPC.damage = 0;
            alert = false;
            NPC.netUpdate = true;
        }

        if (NPC.ai[3] != 0f) {
            if (NPC.aiStyle == 0) {
                NPC.rotation = NPC.ai[3];
            }
            NPC.ai[3] = 0f;
        }
    }
}