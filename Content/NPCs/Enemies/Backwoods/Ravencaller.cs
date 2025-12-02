using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.BackwoodsSystems;
﻿using RoA.Common.World;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Placeable.Banners;
using RoA.Content.Items.Weapons.Magic;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class Ravencaller : ModNPC {
    private readonly int frameTime = 3;
    private readonly int frameHeight = 48;
    private int curFrame;
    private bool retreat = false;
    private bool whenYouWalking = true;
    private float timer;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[NPC.type] = 23;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RavencallersCloak>(), 13));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RavensEye>(), 17));
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
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "RavencallerGore2".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "RavencallerGore1".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "RavencallerGore1".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "RavencallerGore3".GetGoreType(), Scale: NPC.scale);
        }
    }

    public override void SetDefaults() {
        NPC.width = 32; NPC.height = 40;
        NPC.value = Item.sellPrice(0, 0, 10, 0);
        NPC.damage = 30;
        NPC.lifeMax = 250;
        NPC.defense = 8;
        NPC.npcSlots = 1.5f;
        NPC.aiStyle = -1;
        NPC.knockBackResist = 0.2f;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.alpha = 175;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type, ModContent.GetInstance<BackwoodsBiomeFog>().Type];

        DrawOffsetY = 1;

        Banner = Type;
        BannerItem = ModContent.ItemType<RavencallerBanner>();
        ItemID.Sets.KillsToBanner[BannerItem] = 25;

        NPC.rarity = 1;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Ravencaller"),
            new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<BackwoodsBiomeFog>().ModBiomeBestiaryInfoElement)
        ]);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (NPC.IsABestiaryIconDummy) {
            NPC.Opacity = 1f;
            if (NPC.frame.Y < 48) {
                NPC.frame.Y = 48;
            }
        }

        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(retreat);
        writer.Write(whenYouWalking);
        writer.Write(timer);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        retreat = reader.ReadBoolean();
        whenYouWalking = reader.ReadBoolean();
        timer = reader.ReadSingle();
    }

    public override bool? CanFallThroughPlatforms() {
        if (Main.player[NPC.target].dead) {
            return true;
        }
        else {
            return Main.player[NPC.target].position.Y > NPC.position.Y + NPC.height;
        }
    }

    public static void SummonItself(IEntitySource source, int x, int y) {
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }

        if (y / 16 >= BackwoodsVars.FirstTileYAtCenter + 35) {
            return;
        }

        int npcSlot = NPC.NewNPC(source, x, y + 5, ModContent.NPCType<Ravencaller>());
        (Main.npc[npcSlot].ModNPC as Ravencaller).timer = 300;
        (Main.npc[npcSlot].ModNPC as Ravencaller).whenYouWalking = false;
        (Main.npc[npcSlot].ModNPC as Ravencaller).curFrame = 16;
        Main.npc[npcSlot].TargetClosest();
        Main.npc[npcSlot].ai[3] = 1f;
        Main.npc[npcSlot].netUpdate = true;
        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcSlot);
    }

    public override void AI() {
        Player player = Main.player[NPC.target];

        bool flag5 = BackwoodsFogHandler.IsFogActive;
        if (whenYouWalking)
            NPC.ApplyFighterAI(true, flag5);
        else {
            NPC.ResetAIStyle();
            if (NPC.IsGrounded()) {
                NPC.velocity.X *= 0.8f;
            }
        }

        NPC.chaseable = NPC.alpha < 50;

        if (NPC.chaseable) {
            for (int i = 0; i < NPC.maxBuffs; i++) {
                if (NPC.buffType[i] >= 0) {
                    continue;
                }
                if (NPC.buffType[i] != 0) {
                    NPC.DelBuff(i);
                }
            }
        }

        if (timer >= 270 && whenYouWalking && retreat) {
            retreat = false;
            NPC.netUpdate = true;
        }

        //NPC.TargetClosest();

        //if (Collision.CanHit(npc, Main.player[NPC.target]))
        bool flag = NPC.CountNPCS(ModContent.NPCType<SummonedRaven>()) < 8;
        if (NPC.HasPlayerTarget && flag)
            timer++;
        else {
            timer = 438;
        }
        NPC.spriteDirection = NPC.direction;

        if (NPC.alpha > 0 && ((timer >= 270 && timer < 440) || !NPC.HasPlayerTarget)) {
            NPC.alpha -= 5;
        }

        if (NPC.IsGrounded() && timer > 280 && Collision.CanHit(NPC, Main.player[NPC.target]) && Vector2.Distance(player.position, NPC.position) < 320.0 && whenYouWalking) {
            if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.Next(15) == 0) {
                timer = 280;
                whenYouWalking = false;
                NPC.netUpdate = true;
            }
        }
        if (NPC.ai[3] != 1f && timer == 320 && !whenYouWalking)
            Summon();
        if (timer == 430 && !whenYouWalking) {
            for (int k = 0; k < 20; k++) {
                int dust5 = Dust.NewDust(new Vector2(NPC.Center.X, NPC.Center.Y), NPC.width, NPC.height, 108, 0f, 0f, 120, new Color(30, 30, 55), 1f + Main.rand.NextFloat(0, 1f));
                Main.dust[dust5].position = new Vector2(NPC.Center.X + Main.rand.Next(-20, 20), NPC.Center.Y - 6 + Main.rand.Next(-20, 20));
                Main.dust[dust5].velocity = Vector2.Normalize(NPC.Center - Main.dust[dust5].position) * 0.5f;
                Main.dust[dust5].noGravity = true;
            }
        }
        if (timer >= 439 && timer < 447/* && !whenYouWalking*/) {
            if (NPC.alpha < 175) {
                NPC.alpha += 30;
            }
        }
        if (timer == 480 && !whenYouWalking) {
            NPC.target = 0;
            NPC.ai[3] = 0f;
            //NPC.TargetClosest(false);
            retreat = true;
            timer = 0;
            whenYouWalking = true;
            NPC.netUpdate = true;
        }
        if (!flag) {
            whenYouWalking = true;
            timer = 200;
        }
    }

    private void Summon() {
        Vector2 position = new Vector2(NPC.Center.X + 6 * NPC.direction, NPC.Center.Y - 4);
        SoundEngine.PlaySound(SoundID.DD2_LightningAuraZap with { Volume = 1.25f, Pitch = 0.2f }, NPC.Center);
        SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "Crow") { Volume = 2f, Pitch = 0.3f, PitchVariance = 0.1f }, NPC.Center);
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
        NPC.frame.Y = curFrame * frameHeight;

        if (NPC.IsABestiaryIconDummy) {
            if (curFrame == 0) {
                curFrame = 1;
            }
            if (NPC.frameCounter >= frameTime) {
                ++curFrame;
                NPC.frameCounter = 0;
            }
            else {
                NPC.frameCounter++;
            }
            if (curFrame >= 15) {
                curFrame = 1;
            }

            return;
        }
        if (whenYouWalking) {
            if (NPC.velocity.Y != 0) {
                curFrame = 0;
            }
            else {
                if (NPC.frameCounter >= frameTime) {
                    ++curFrame;
                    NPC.frameCounter = 0;
                }
                else {
                    NPC.frameCounter++;
                }
                if (curFrame >= 15) {
                    curFrame = 1;
                }
            }
        }
        else if (timer >= 280 && timer <= 300) {
            curFrame = 15;
        }
        else if ((timer > 300 && curFrame < 19 && curFrame != 1) || (timer > 420 && curFrame <= 21 && curFrame != 1)) {
            if (NPC.frameCounter >= frameTime) {
                ++curFrame;
                NPC.frameCounter = 0;
            }
            else {
                NPC.frameCounter++;
            }
        }
        else if (curFrame > 21) {
            curFrame = 1;
        }
    }
}
