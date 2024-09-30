using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class Ravencaller : ModNPC {
    private Player player;
    private readonly int frameTime = 3;
    private readonly int frameHeight = 48;
    private int curFrame;
    private bool retreat = false;
    private bool whenYouWalking = true;
    private float timer;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[NPC.type] = 23;
    }

    public override void SetDefaults() {
        NPC.width = 32; NPC.height = 40;
        NPC.value = Item.sellPrice(0, 0, 6, 30);
        NPC.damage = 32;
        NPC.lifeMax = 300;
        NPC.defense = 3;
        NPC.npcSlots = 0.8f;
        NPC.aiStyle = -1;
        NPC.knockBackResist = 0.2f;
        NPC.HitSound = SoundID.NPCHit19;
        NPC.DeathSound = SoundID.NPCDeath1;
    }

    public override void AI() {
        NPC npc = NPC;
        if (whenYouWalking)
            NPC.ApplyFighterAI(true);
        else {
            NPC.ResetAIStyle();
            NPC.velocity.X *= 0.8f;
        }

        NPC.chaseable = npc.alpha < 80;

        if (timer >= 270 && whenYouWalking) {
            retreat = false;
        }

        if (!retreat)
            player = Main.player[npc.target];

        //npc.TargetClosest();

        //if (Collision.CanHit(npc, Main.player[npc.target]))
        if (NPC.HasPlayerTarget)
            timer++;
        npc.spriteDirection = npc.direction;

        if (npc.alpha > 0 && timer >= 270 && timer < 440) {
            npc.alpha -= 5;
        }

        if (NPC.velocity.Y == 0f && timer > 280 && Collision.CanHit(npc, Main.player[npc.target]) && Vector2.Distance(player.position, npc.position) < 320.0 && whenYouWalking && Main.rand.Next(15) == 0) {
            timer = 280;
            whenYouWalking = false;
        }
        if (timer == 320 && !whenYouWalking)
            Summon();
        if (timer == 430 && !whenYouWalking) {
            for (int k = 0; k < 20; k++) {
                int dust5 = Dust.NewDust(new Vector2(npc.Center.X, npc.Center.Y), npc.width, npc.height, 108, 0f, 0f, 120, new Color(30, 30, 55), 1f + Main.rand.NextFloat(0, 1f));
                Main.dust[dust5].position = new Vector2(npc.Center.X + Main.rand.Next(-20, 20), npc.Center.Y - 6 + Main.rand.Next(-20, 20));
                Main.dust[dust5].velocity = Vector2.Normalize(npc.Center - Main.dust[dust5].position) * 0.5f;
                Main.dust[dust5].noGravity = true;
            }
        }
        if (timer >= 434 && timer < 442 && !whenYouWalking) {
            if (npc.alpha < 200) {
                npc.alpha += 30;
            }
        }
        if (timer == 480 && !whenYouWalking) {
            npc.target = 0;
            //npc.TargetClosest(false);
            retreat = true;
            timer = 0;
            whenYouWalking = true;
        }
    }

    private void Summon() {
        Vector2 position = new Vector2(NPC.Center.X + 6 * NPC.direction, NPC.Center.Y - 4);
        SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap);
        for (int i = 0; i < 12; i++) {
            Dust.NewDust(position, 16, 16, DustID.Smoke, (float)Math.Cos(MathHelper.Pi / 6 * i), (float)Math.Sin(MathHelper.Pi / 6 * i), 0, Color.Black, 0.7f);
        }
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }
        for (int i = 0; i < Main.rand.Next(2, 4); i++) {
            Vector2 spreadOld = new Vector2(Main.player[NPC.target].position.X - NPC.Center.X, Main.player[NPC.target].position.Y - NPC.Center.Y).RotatedByRandom(MathHelper.ToRadians(45));
            Vector2 spread = Vector2.Normalize(spreadOld) * 2.25f;

            int npc = NPC.NewNPC(NPC.GetSource_Death(), (int)position.X, (int)position.Y, ModContent.NPCType<SummonedRaven>(), 0, spread.X, spread.Y);
            if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
                NetMessage.SendData(MessageID.SyncNPC, number: npc);
            }
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC npc = NPC;
        npc.frame.Y = curFrame * frameHeight;
        if (whenYouWalking) {
            if (npc.velocity.Y != 0) {
                curFrame = 0;
            }
            else {
                if (npc.frameCounter >= frameTime) {
                    ++curFrame;
                    npc.frameCounter = 0;
                }
                else {
                    npc.frameCounter++;
                }
                if (curFrame >= 15) {
                    curFrame = 1;
                }
            }
        }
        else if (timer == 280) {
            curFrame = 15;
        }
        /*else if (timer == 320)
        {
            curFrame = 15;
        }*/
        else if ((timer > 300 && curFrame < 19 && curFrame != 1) || (timer > 420 && curFrame <= 21 && curFrame != 1)) {
            if (npc.frameCounter >= frameTime) {
                ++curFrame;
                npc.frameCounter = 0;
            }
            else {
                npc.frameCounter++;
            }
        }
        else if (curFrame > 21) {
            curFrame = 1;
        }
    }
}
