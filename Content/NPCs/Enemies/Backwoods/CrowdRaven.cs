using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
﻿using RoA.Common.World;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class CrowdRaven : ModNPC {
    private static Asset<Texture2D> _glowTexture = null!;

    public override void SetStaticDefaults() {
        //base.SetStaticDefaults();

        // DisplayName.SetDefault("Summoned Raven");
        Main.npcFrameCount[Type] = 5;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            //Position = new Vector2(2f, -10f),
            //PortraitPositionXOverride = 0f,
            //PortraitPositionYOverride = -31f
            Hide = true
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        if (Main.dedServ) {
            return;
        }

        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.CrowdRaven")
        ]);
    }

    public override void FindFrame(int frameHeight) {
        int num83 = Main.npcFrameCount[NPC.type] - 1;
        if (NPC.IsABestiaryIconDummy) {
            if (NPC.frame.Y < frameHeight) {
                NPC.frame.Y = frameHeight;
            }
            NPC.frameCounter += 1.0;
            if (NPC.frameCounter >= 6.0) {
                NPC.frame.Y += frameHeight;
                NPC.frameCounter = 0.0;
            }

            if (NPC.frame.Y >= frameHeight * num83)
                NPC.frame.Y = frameHeight;

            return;
        }

        NPC.spriteDirection = -NPC.direction;
        NPC.rotation = NPC.velocity.X * 0.1f;
        if (NPC.velocity.X == 0f && NPC.IsGrounded()) {
            NPC.frame.Y = 0;
            NPC.frameCounter = 0.0;
            return;
        }

        NPC.frameCounter += 1.0;
        if (NPC.frameCounter >= 6.0) {
            NPC.frame.Y += frameHeight;
            NPC.frameCounter = 0.0;
        }

        if (NPC.frame.Y >= frameHeight * num83)
            NPC.frame.Y = frameHeight;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life <= 0 && Main.netMode != NetmodeID.MultiplayerClient) {
            int npc = NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<BackwoodsRaven>());
            if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
                NetMessage.SendData(MessageID.SyncNPC, number: npc);
            }
        }

        if (NPC.life > 0) {
            for (int num828 = 0; (double)num828 < hit.Damage / (double)NPC.lifeMax * 100.0; num828++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num510 = 0; num510 < 50; num510++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f);
        }

        if (!Main.dedServ) {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "SummonedRavenGore1".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + 14f, NPC.position.Y), NPC.velocity, "SummonedRavenGore2".GetGoreType());
        }
    }

    public override void SetDefaults() {
        NPC.width = 36;
        NPC.height = 26;

        NPC.friendly = false;

        NPC.aiStyle = -1;

        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 15;

        NPC.HitSound = SoundID.NPCHit1;
        NPC.knockBackResist = 0.85f;
        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.npcSlots = 0.4f;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<BackwoodsRavenBanner>();
    }

    public override void AI() {
        NPC.chaseable = false;

        Vector3 rgb3 = new Vector3(1f, 0f, 0.1f) * 0.35f;
        Lighting.AddLight(NPC.Top + new Vector2(0f, 10f), rgb3);

        NPC.noGravity = true;
        if (NPC.localAI[0] == 0f) {
            NPC.localAI[0] = 1f;
            NPC.direction = Main.rand.NextFromList(-1, 1);
        }
        if (NPC.ai[1] == 1f) {
            if (NPC.Opacity <= 0f) {
                NPC.KillNPC();
            }
            else {
                NPC.Opacity -= TimeSystem.LogicDeltaTime;
            }
        }
        if (NPC.ai[3] == 0f) {
            NPC.ai[3] = 1f;
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                for (int i = 0; i < Main.rand.Next(3, 6); i++) {
                    int x = (int)(NPC.Center.X + Main.rand.NextFloatRange(50f));
                    int y = (int)NPC.Center.Y;
                    bool flag = false;
                    while (WorldGen.SolidTile(x / 16, y / 16)) {
                        y -= 16;
                        if (Math.Abs(NPC.Center.Y - y) > 100f) {
                            flag = true;
                            break;
                        }
                        if (TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(x / 16, y / 16).TileType]) {
                            break;
                        }
                    }
                    if (flag) {
                        continue;
                    }
                    int npcSlot = NPC.NewNPC(NPC.GetSource_FromAI(), x, y, Type, ai3: 2f);
                    Main.npc[npcSlot].netUpdate = true;
                    NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcSlot);
                }
            }
        }
        if (NPC.ai[0] != 0f) {
            NPC.noTileCollide = true;
            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height)) {
                NPC.ai[1] = 1f;
            }
        }
        if (NPC.ai[0] == 0f) {
            NPC.noGravity = false;
            NPC.TargetClosest();
            if (NPC.ai[2] > 0f) {
                if (NPC.localAI[1] < NPC.ai[2]) {
                    NPC.localAI[1]++;
                }
                else {
                    NPC.ai[0] = 1f;
                    NPC.velocity.Y -= 6f;
                    NPC.direction = Main.rand.NextFromList(-1, 1);
                    if (NPC.ai[3] == 1f && BackwoodsFogHandler.IsFogActive && NPC.downedBoss2) {
                        Ravencaller.SummonItself(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y);
                    }
                    NPC.netUpdate = true;
                }
            }
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                Rectangle rectangle2 = new((int)Main.player[NPC.target].position.X, (int)Main.player[NPC.target].position.Y, Main.player[NPC.target].width, Main.player[NPC.target].height);
                if (new Rectangle((int)NPC.position.X - 250, (int)NPC.position.Y - 250, NPC.width + 500, NPC.height + 500).Intersects(rectangle2) || NPC.life < NPC.lifeMax) {
                    foreach (NPC npc in Main.ActiveNPCs) {
                        if (npc.whoAmI != NPC.whoAmI && npc.type == Type && NPC.Distance(npc.Center) < 100f && npc.ai[2] == 0f) {
                            npc.ai[2] = Main.rand.NextFloat(15f, 30f);
                            npc.netUpdate = true;
                        }
                    }
                    NPC.ai[0] = 1f;
                    NPC.velocity.Y -= 6f;
                    NPC.direction = Main.rand.NextFromList(-1, 1);
                    if (NPC.ai[3] == 1f && BackwoodsFogHandler.IsFogActive && NPC.downedBoss2) {
                        Ravencaller.SummonItself(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y);
                    }
                    NPC.netUpdate = true;
                }
            }
        }
        else if (NPC.ai[0] == 2f) {
            NPC.velocity.X *= 0.98f;
            if (NPC.IsGrounded()) {
                NPC.ai[0] = 0f;
                NPC.velocity.X = 0f;
            }

            NPC.velocity.Y += 0.05f;
            if (NPC.velocity.Y > 2f)
                NPC.velocity.Y = 2f;
        }
        else if (!Main.player[NPC.target].dead) {
            float num343 = 3f;
            num343 = 4f;

            //if (NPC.collideX) {
            //    NPC.direction *= -1;
            //    velocity.X = oldVelocity.X * -0.5f;
            //    if (direction == -1 && velocity.X > 0f && velocity.X < num343 - 1f)
            //        velocity.X = num343 - 1f;

            //    if (direction == 1 && velocity.X < 0f && velocity.X > 0f - num343 + 1f)
            //        velocity.X = 0f - num343 + 1f;
            //}

            //if (collideY) {
            //    velocity.Y = oldVelocity.Y * -0.5f;
            //    if (velocity.Y > 0f && velocity.Y < 1f)
            //        velocity.Y = 1f;

            //    if (velocity.Y < 0f && velocity.Y > -1f)
            //        velocity.Y = -1f;
            //}

            if (NPC.direction == -1 && NPC.velocity.X > 0f - num343) {
                NPC.velocity.X -= 0.1f;
                if (NPC.velocity.X > num343)
                    NPC.velocity.X -= 0.1f;
                else if (NPC.velocity.X > 0f)
                    NPC.velocity.X -= 0.05f;

                if (NPC.velocity.X < 0f - num343)
                    NPC.velocity.X = 0f - num343;
            }
            else if (NPC.direction == 1 && NPC.velocity.X < num343) {
                NPC.velocity.X += 0.1f;
                if (NPC.velocity.X < 0f - num343)
                    NPC.velocity.X += 0.1f;
                else if (NPC.velocity.X < 0f)
                    NPC.velocity.X += 0.05f;

                if (NPC.velocity.X > num343)
                    NPC.velocity.X = num343;
            }

            int num344 = (int)((NPC.position.X + (float)(NPC.width / 2)) / 16f) + NPC.direction;
            int num345 = (int)((NPC.position.Y + (float)NPC.height) / 16f);
            bool flag23 = true;
            int num346 = 15;
            bool flag24 = false;
            for (int num347 = num345; num347 < num345 + num346; num347++) {
                if (!WorldGen.InWorld(num344, num347))
                    continue;

                //if (Main.tile[num344, num347] == null)
                //    Main.tile[num344, num347] = new Tile();

                if ((Main.tile[num344, num347].HasUnactuatedTile && Main.tileSolid[Main.tile[num344, num347].TileType]) || Main.tile[num344, num347].LiquidAmount > 0) {
                    if (num347 < num345 + 5)
                        flag24 = true;

                    flag23 = false;
                    break;
                }
            }

            if (flag23)
                NPC.velocity.Y += 0.05f;
            else
                NPC.velocity.Y -= 0.1f;

            if (flag24)
                NPC.velocity.Y -= 0.2f;

            if (NPC.velocity.Y > 2f)
                NPC.velocity.Y = 2f;

            if (NPC.velocity.Y < -4f)
                NPC.velocity.Y = -4f;
        }
    }

    public override bool? CanFallThroughPlatforms() => true;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Texture2D texture = NPC.GetTexture();
        spriteBatch.Draw(texture, NPC.position - screenPos + new Vector2(NPC.width, NPC.height) / 2, NPC.frame,
            drawColor * (1f - NPC.alpha / 255f), NPC.rotation, new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) / 2, NPC.scale, NPC.velocity.X > 0f ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        texture = _glowTexture.Value;
        spriteBatch.Draw(texture, NPC.position - screenPos + new Vector2(NPC.width, NPC.height) / 2, NPC.frame, new Color(200, 200, 200, 100) * (1f - NPC.alpha / 255f), NPC.rotation, new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) / 2, NPC.scale, NPC.velocity.X > 0f ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        return false;
    }
}