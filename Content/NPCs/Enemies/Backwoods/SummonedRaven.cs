using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class SummonedRaven : ModNPC {
    private static Asset<Texture2D> _glowTexture = null!;

    public enum States {
        Spawn,
        Attacking
    }

    private const short SPAWN = (short)States.Spawn;
    private const short ATTACKING = (short)States.Attacking;

    private ref float State => ref NPC.ai[2];
    private ref float Acceleration => ref NPC.ai[3];

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
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.SummonedRaven")
        ]);
    }

    public override void SetDefaults() {
        NPC.CloneDefaults(NPCID.Raven);

        NPC.aiStyle = -1;

        NPC.alpha = 255;

        NPC.lifeMax = 30;
        NPC.damage = 22;
        NPC.defense = 4;

        NPC.npcSlots = 0.25f;

        NPC.noTileCollide = false;

        //SpawnModBiomes = new int[] {
        //	ModContent.GetInstance<BackwoodsBiome>().Type
        //};

        NPC.value = 0;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<BackwoodsRavenBanner>();
    }

    public override bool? CanFallThroughPlatforms() => true;

    public override void AI() {
        Vector3 rgb3 = new Vector3(1f, 0f, 0.1f) * 0.35f;
        Lighting.AddLight(NPC.Top + new Vector2(0f, 10f), rgb3);

        if (NPC.NearestTheSame(out NPC npc)) {
            NPC.OffsetNPC(npc, 0.2f);
        }

        if (Acceleration < 1.5f) {
            Acceleration += 0.065f;
            Acceleration *= 1.05f;
        }

        short state = (short)State;
        if (state == SPAWN) {
            NPC.noGravity = true;

            if (NPC.ai[0] == 0f && NPC.ai[1] == 0f) {
                NPC.alpha = 0;

                State = ATTACKING;
            }

            NPC.velocity = new Vector2(NPC.ai[0], NPC.ai[1]) * Acceleration * 0.75f;
            NPC.rotation = NPC.velocity.ToRotation() + MathHelper.ToRadians(90f);

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool()) {
                int dust = Dust.NewDust(NPC.position, 16, 16, 108, NPC.velocity.X * 0.01f, NPC.velocity.Y * 0.01f, NPC.alpha, new Color(30, 30, 55), Main.rand.NextFloat(1.1f, 1.3f));
                Main.dust[dust].noLight = true;
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity.X *= 0.4f;
                Main.dust[dust].velocity.Y *= 0.4f;
            }

            if (NPC.alpha > 0) {
                NPC.alpha -= 20 - Main.rand.Next(1, 10);

                if (Main.netMode != NetmodeID.Server) {
                    if (Main.rand.NextBool(10)) {
                        for (int i = 0; i < 16; i++) {
                            int dust = Dust.NewDust(NPC.position, 20, 20, 108, (float)Math.Cos(MathHelper.Pi / 6 * i), (float)Math.Sin(MathHelper.Pi / 6 * i), 140, new Color(30, 30, 55), 1.2f);
                            Main.dust[dust].noGravity = true;
                        }
                    }
                }
                return;
            }
            State = ATTACKING;
            NPC.netUpdate = true;
            return;
        }

        NPC.ApplyAdvancedFlierAI();

        float value = NPC.velocity.X * 0.1f;
        NPC.rotation = Helper.SmoothAngleLerp(NPC.rotation, value, (Math.Abs(value) * 0.2f + 0.2f));

        NPC.alpha = 0;
        //AnimationType = 301;
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

        short state = (short)State;
        int num = 1;
        if ((double)NPC.velocity.X > 0.5)
            NPC.spriteDirection = -1;
        if ((double)NPC.velocity.X < -0.5)
            NPC.spriteDirection = 1;
        if (!Main.dedServ) {
            if (!TextureAssets.Npc[Type].IsLoaded)
                return;

            num = TextureAssets.Npc[Type].Height() / Main.npcFrameCount[Type];
        }
        if (state != SPAWN) {
            if (NPC.velocity.X == 0f && NPC.IsGrounded()) {
                NPC.frame.Y = 0;
                NPC.frameCounter = 0.0;
                return;
            }
            NPC.frameCounter += 1.0;
            if (NPC.frameCounter > 4.0) {
                NPC.frameCounter = 0.0;
                NPC.frame.Y += num;
            }
            if (NPC.frame.Y > num * 4 || NPC.frame.Y == 0)
                NPC.frame.Y = num;
            return;
        }
        NPC.frame.Y = 0;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Texture2D texture = NPC.GetTexture();
        if (NPC.IsABestiaryIconDummy) {
            NPC.Opacity = 1f;
        }
        spriteBatch.Draw(texture, NPC.position - screenPos + new Vector2(NPC.width, NPC.height) / 2, NPC.frame, drawColor * (1f - NPC.alpha / 255f), NPC.rotation, new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) / 2, NPC.scale, NPC.velocity.X > 0f ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        texture = _glowTexture.Value;
        spriteBatch.Draw(texture, NPC.position - screenPos + new Vector2(NPC.width, NPC.height) / 2, NPC.frame, new Color(200, 200, 200, 100) * (1f - NPC.alpha / 255f), NPC.rotation, new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) / 2, NPC.scale, NPC.velocity.X > 0f ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        return false;
    }
}