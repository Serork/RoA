using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Items.Equipables.Accessories;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Friendly;

sealed class FireflyMimic : ModNPC {
    private static Asset<Texture2D> _glowTexture = null!;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;

        NPCID.Sets.TakesDamageFromHostilesWithoutBeingFriendly[Type] = true;
        NPCID.Sets.CountsAsCritter[Type] = true;
        NPCID.Sets.ShimmerTransformToNPC[Type] = 677;

        NPCID.Sets.SpecificDebuffImmunity[Type][31] = true;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(0f, 2f),
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

        if (Main.dedServ) {
            return;
        }

        _glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.AddTags(BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.FireflyMimic"));
    }

    public override void SetDefaults() {
        NPC.width = 12;
        NPC.height = 12;
        NPC.aiStyle = 64;
        NPC.damage = 0;
        NPC.defense = 0;
        NPC.lifeMax = 5;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.npcSlots = 0.2f;
        NPC.noGravity = true;
        NPC.catchItem = ModContent.ItemType<FireflyPin>();

        NPC.rarity = 4;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        SpriteEffects spriteEffects = SpriteEffects.None;
        if (NPC.spriteDirection == 1)
            spriteEffects = SpriteEffects.FlipHorizontally;
        Texture2D texture = _glowTexture.Value;
        Vector2 halfSize = new Vector2(TextureAssets.Npc[Type].Width() / 2, TextureAssets.Npc[Type].Height() / Main.npcFrameCount[Type] / 2);
        spriteBatch.Draw(texture,
            new Vector2(NPC.position.X - screenPos.X + (float)(NPC.width / 2) -
            (float)TextureAssets.Npc[Type].Width() * NPC.scale / 2f + halfSize.X * NPC.scale,
            NPC.position.Y - screenPos.Y + (float)NPC.height -
            (float)TextureAssets.Npc[Type].Height() * NPC.scale / (float)Main.npcFrameCount[Type] + 4f + halfSize.Y * NPC.scale),
            NPC.frame, new Microsoft.Xna.Framework.Color(255, 255, 255, 0), NPC.rotation, halfSize, NPC.scale, spriteEffects, 0f);
    }

    public override void AI() {
        float num1008 = NPC.ai[0];
        float num1009 = NPC.ai[1];
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            NPC.localAI[0] -= 1f;
            if (NPC.ai[3] == 0f)
                NPC.ai[3] = (float)Main.rand.Next(75, 111) * 0.01f;

            if (NPC.localAI[0] <= 0f) {
                NPC.TargetClosest();
                NPC.localAI[0] = Main.rand.Next(60, 180);
                float num1010 = Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X);
                if (num1010 > 700f && NPC.localAI[3] == 0f) {
                    float num1011 = (float)Main.rand.Next(50, 151) * 0.01f;
                    if (num1010 > 1000f)
                        num1011 = (float)Main.rand.Next(150, 201) * 0.01f;
                    else if (num1010 > 850f)
                        num1011 = (float)Main.rand.Next(100, 151) * 0.01f;

                    int num1012 = NPC.direction * Main.rand.Next(100, 251);
                    int num1013 = Main.rand.Next(-50, 51);
                    if (NPC.position.Y > Main.player[NPC.target].position.Y - 100f)
                        num1013 -= Main.rand.Next(100, 251);

                    float num1014 = num1011 / (float)Math.Sqrt(num1012 * num1012 + num1013 * num1013);
                    num1008 = (float)num1012 * num1014;
                    num1009 = (float)num1013 * num1014;
                }
                else {
                    NPC.localAI[3] = 1f;
                    float num1015 = (float)Main.rand.Next(5, 151) * 0.01f;
                    int num1016 = Main.rand.Next(-100, 101);
                    int num1017 = Main.rand.Next(-100, 101);
                    float num1018 = num1015 / (float)Math.Sqrt(num1016 * num1016 + num1017 * num1017);
                    num1008 = (float)num1016 * num1018;
                    num1009 = (float)num1017 * num1018;
                }

                NPC.netUpdate = true;
            }
        }

        NPC.scale = NPC.ai[3];
        if (NPC.localAI[2] > 0f) {
            int i3 = (int)NPC.Center.X / 16;
            int j3 = (int)NPC.Center.Y / 16;
            if (NPC.localAI[2] > 3f) {
                Lighting.AddLight(i3, j3, 1f * NPC.scale, 0.9f * NPC.scale, 0.35f * NPC.scale);
            }

            NPC.localAI[2] -= 1f;
        }
        else if (NPC.localAI[1] > 0f) {
            NPC.localAI[1] -= 1f;
        }
        else {
            NPC.localAI[1] = Main.rand.Next(30, 180) * 2;
            if (!Main.dayTime || (double)(NPC.position.Y / 16f) > Main.worldSurface + 10.0)
                NPC.localAI[2] = Main.rand.Next(10, 30) * 2;
        }

        int num1024 = 80;
        NPC.velocity.X = (NPC.velocity.X * (float)(num1024 - 1) + num1008) / (float)num1024;
        NPC.velocity.Y = (NPC.velocity.Y * (float)(num1024 - 1) + num1009) / (float)num1024;
        if (NPC.velocity.Y > 0f) {
            int num1025 = 4;
            int num1026 = (int)NPC.Center.X / 16;
            int num1027 = (int)NPC.Center.Y / 16;
            for (int num1028 = num1027; num1028 < num1027 + num1025; num1028++) {
                if (WorldGen.InWorld(num1026, num1028, 2) && Main.tile[num1026, num1028] != null && ((Main.tile[num1026, num1028].HasUnactuatedTile && Main.tileSolid[Main.tile[num1026, num1028].TileType]) || Main.tile[num1026, num1028].LiquidAmount > 0)) {
                    num1009 *= -1f;
                    if (NPC.velocity.Y > 0f)
                        NPC.velocity.Y *= 0.9f;
                }
            }
        }

        if (NPC.velocity.Y < 0f) {
            int num1029 = 30;
            bool flag51 = false;
            int num1030 = (int)NPC.Center.X / 16;
            int num1031 = (int)NPC.Center.Y / 16;
            for (int num1032 = num1031; num1032 < num1031 + num1029; num1032++) {
                if (WorldGen.InWorld(num1030, num1032, 2) && Main.tile[num1030, num1032] != null && Main.tile[num1030, num1032].HasUnactuatedTile && Main.tileSolid[Main.tile[num1030, num1032].TileType])
                    flag51 = true;
            }

            if (!flag51) {
                num1009 *= -1f;
                if (NPC.velocity.Y < 0f)
                    NPC.velocity.Y *= 0.9f;
            }
        }

        if (NPC.collideX) {
            num1008 = ((!(NPC.velocity.X < 0f)) ? (0f - Math.Abs(num1008)) : Math.Abs(num1008));
            NPC.velocity.X *= -0.2f;
        }

        if (NPC.velocity.X < 0f)
            NPC.direction = -1;

        if (NPC.velocity.X > 0f)
            NPC.direction = 1;

        NPC.ai[0] = num1008;
        NPC.ai[1] = num1009;
    }

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = NPC.direction;
        NPC.frameCounter += 1.0;
        if (NPC.frameCounter < 4.0) {
            NPC.frame.Y = 0;
        }
        else {
            NPC.frame.Y = frameHeight;
            if (NPC.frameCounter >= 7.0)
                NPC.frameCounter = 0.0;
        }
        if (NPC.localAI[2] <= 0f)
            NPC.frame.Y += frameHeight * 2;
    }

    public override void HitEffect(NPC.HitInfo hit) {
        for (int num361 = 0; num361 < 6; num361++) {
            int num362 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.FireflyHit, 2 * hit.HitDirection, -2f);
            if (Main.rand.Next(2) == 0) {
                Main.dust[num362].noGravity = true;
                Main.dust[num362].scale = 1.5f * NPC.scale;
            }
            else {
                Main.dust[num362].scale = 0.8f * NPC.scale;
            }
        }
    }
}
