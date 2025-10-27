using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class Hog : RoANPC {
    private int _currentAI;
    private float _extraAITimer;
    private int _soundTimer;

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 16;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Hog")
        ]);
    }

    public override void SetDefaults() {
        NPC.CloneDefaults(NPCID.Bunny);

        NPC.lifeMax = 85;
        NPC.damage = 22;
        NPC.defense = 7;
        NPC.knockBackResist = 0.75f;

        NPC.aiStyle = -1;

        NPC.npcSlots = 0.9f;

        int width = 40; int height = 35;
        NPC.Size = new Vector2(width, height);

        NPC.value = Item.buyPrice(0, 0, 1, 0);

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<HogBanner>();
    }

    public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) => RageMode();
    public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) => RageMode();

    private void RageMode() {
        if (_currentAI != 2) {
            _currentAI = 2;
            NPC.TargetClosest();
            NPC.netUpdate = true;
        }
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(_currentAI);
        writer.Write(_extraAITimer);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _currentAI = reader.ReadInt32();
        _extraAITimer = reader.ReadSingle();
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => _currentAI == 2;

    public override void HitEffect(NPC.HitInfo hit) {
        _currentAI = 2;
        NPC.netUpdate = true;

        if (NPC.life > 0) {
            for (int num493 = 0; (double)num493 < hit.Damage / (double)NPC.lifeMax * 50.0; num493++) {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, hit.HitDirection, -1f);
            }

            return;
        }

        for (int num494 = 0; num494 < 20; num494++) {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, 5, 2 * hit.HitDirection, -2f);
        }

        if (!Main.dedServ) {
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "HogGore2".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "HogGore1".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "HogGore3".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "HogGore3".GetGoreType());
        }
    }

    private void AdaptedUnicornAI() {
        int num = 10;
        int num2 = 10;
        bool flag = false;
        bool flag2 = false;
        bool flag3 = false;
        if (NPC.IsGrounded() && ((NPC.velocity.X > 0f && NPC.direction < 0) || (NPC.velocity.X < 0f && NPC.direction > 0))) {
            flag2 = true;
            NPC.ai[3] += 1f;
        }
        if (NPC.position.X == NPC.oldPosition.X || NPC.ai[3] >= (float)num || flag2) {
            NPC.ai[3] += 1f;
            flag3 = true;
        }
        else if (NPC.ai[3] > 0f) {
            NPC.ai[3] -= 1f;
        }

        if (NPC.ai[3] > (float)(num * num2))
            NPC.ai[3] = 0f;

        if (NPC.justHit)
            NPC.ai[3] = 0f;

        if (NPC.ai[3] == (float)num)
            NPC.netUpdate = true;

        Vector2 vector3 = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
        float num7 = Main.player[NPC.target].position.X + (float)Main.player[NPC.target].width * 0.5f - vector3.X;
        float num8 = Main.player[NPC.target].position.Y - vector3.Y;
        float num9 = (float)Math.Sqrt(num7 * num7 + num8 * num8);
        if (num9 < 200f && !flag3)
            NPC.ai[3] = 0f;

        if (NPC.ai[3] < (float)num) {
            NPC.TargetClosest();
        }
        else {
            if (NPC.velocity.X == 0f) {
                if (NPC.IsGrounded()) {
                    NPC.ai[0] += 1f;
                    if (NPC.ai[0] >= 2f) {
                        NPC.direction *= -1;
                        NPC.spriteDirection = NPC.direction;
                        NPC.ai[0] = 0f;
                    }
                }
            }
            else {
                NPC.ai[0] = 0f;
            }

            NPC.directionY = -1;
            if (NPC.direction == 0)
                NPC.direction = 1;
        }

        float num11 = 3.5f;
        float num12 = 0.08f;
        if (!flag && (NPC.IsGrounded() || NPC.wet || (NPC.velocity.X <= 0f && NPC.direction < 0) || (NPC.velocity.X >= 0f && NPC.direction > 0))) {
            if (NPC.velocity.X < 0f - num11 || NPC.velocity.X > num11) {
                if (NPC.IsGrounded())
                    NPC.velocity *= 0.7f;
            }
            else if (NPC.velocity.X < num11 && NPC.direction == 1) {
                NPC.velocity.X += num12;
                if (NPC.velocity.X > num11)
                    NPC.velocity.X = num11;
            }
            else if (NPC.velocity.X > 0f - num11 && NPC.direction == -1) {
                NPC.velocity.X -= num12;
                if (NPC.velocity.X < 0f - num11)
                    NPC.velocity.X = 0f - num11;
            }
        }

        if (NPC.velocity.Y >= 0f) {
            int num14 = 0;
            if (NPC.velocity.X < 0f)
                num14 = -1;

            if (NPC.velocity.X > 0f)
                num14 = 1;

            Vector2 vector8 = NPC.position;
            vector8.X += NPC.velocity.X;
            int num15 = (int)((vector8.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 1) * num14)) / 16f);
            int num16 = (int)((vector8.Y + (float)NPC.height - 1f) / 16f);

            if ((float)(num15 * 16) < vector8.X + (float)NPC.width && (float)(num15 * 16 + 16) > vector8.X && ((Main.tile[num15, num16].HasUnactuatedTile && !Main.tile[num15, num16].TopSlope && !Main.tile[num15, num16 - 1].TopSlope && Main.tileSolid[Main.tile[num15, num16].TileType] && !Main.tileSolidTop[Main.tile[num15, num16].TileType]) || (Main.tile[num15, num16 - 1].IsHalfBlock && Main.tile[num15, num16 - 1].HasUnactuatedTile)) && (!Main.tile[num15, num16 - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num15, num16 - 1].TileType] || Main.tileSolidTop[Main.tile[num15, num16 - 1].TileType] || (Main.tile[num15, num16 - 1].IsHalfBlock && (!Main.tile[num15, num16 - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[num15, num16 - 4].TileType] || Main.tileSolidTop[Main.tile[num15, num16 - 4].TileType]))) && (!Main.tile[num15, num16 - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[num15, num16 - 2].TileType] || Main.tileSolidTop[Main.tile[num15, num16 - 2].TileType]) && (!Main.tile[num15, num16 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num15, num16 - 3].TileType] || Main.tileSolidTop[Main.tile[num15, num16 - 3].TileType]) && (!Main.tile[num15 - num14, num16 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num15 - num14, num16 - 3].TileType])) {
                float num17 = num16 * 16;
                if (Main.tile[num15, num16].IsHalfBlock)
                    num17 += 8f;

                if (Main.tile[num15, num16 - 1].IsHalfBlock)
                    num17 -= 8f;

                if (num17 < vector8.Y + (float)NPC.height) {
                    float num18 = vector8.Y + (float)NPC.height - num17;
                    if ((double)num18 <= 16.1) {
                        NPC.gfxOffY += NPC.position.Y + (float)NPC.height - num17;
                        NPC.position.Y = num17 - (float)NPC.height;
                        if (num18 < 9f)
                            NPC.stepSpeed = 1f;
                        else
                            NPC.stepSpeed = 2f;
                    }
                }
            }
        }

        if (NPC.IsGrounded()) {
            bool flag6 = true;
            int num19 = (int)(NPC.position.Y - 7f) / 16;
            int num20 = (int)(NPC.position.X - 7f) / 16;
            int num21 = (int)(NPC.position.X + (float)NPC.width + 7f) / 16;
            for (int m = num20; m <= num21; m++) {
                if (Main.tile[m, num19] != null && Main.tile[m, num19].HasUnactuatedTile && Main.tileSolid[Main.tile[m, num19].TileType]) {
                    flag6 = false;
                    break;
                }
            }

            if (flag6) {
                int num22 = (int)((NPC.position.X + (float)(NPC.width / 2) + (float)((NPC.width / 2 + 2) * NPC.direction) + NPC.velocity.X * 5f) / 16f);
                int num23 = (int)((NPC.position.Y + (float)NPC.height - 15f) / 16f);

                int num24 = NPC.spriteDirection;

                if ((NPC.velocity.X < 0f && num24 == -1) || (NPC.velocity.X > 0f && num24 == 1)) {
                    bool flag7 = false;
                    float num25 = 3f;
                    if (Main.tile[num22, num23 - 2].HasUnactuatedTile && Main.tileSolid[Main.tile[num22, num23 - 2].TileType]) {
                        if (Main.tile[num22, num23 - 3].HasUnactuatedTile && Main.tileSolid[Main.tile[num22, num23 - 3].TileType]) {
                            NPC.velocity.Y = -6.5f;
                            NPC.netUpdate = true;
                        }
                        else {
                            NPC.velocity.Y = -5.5f;
                            NPC.netUpdate = true;
                        }
                    }
                    else if (Main.tile[num22, num23 - 1].HasUnactuatedTile && !Main.tile[num22, num23 - 1].TopSlope && Main.tileSolid[Main.tile[num22, num23 - 1].TileType]) {
                        NPC.velocity.Y = -5f;
                        NPC.netUpdate = true;
                    }
                    else if (NPC.position.Y + (float)NPC.height - (float)(num23 * 16) > 20f && Main.tile[num22, num23].HasUnactuatedTile && !Main.tile[num22, num23].TopSlope && Main.tileSolid[Main.tile[num22, num23].TileType]) {
                        NPC.velocity.Y = -4f;
                        NPC.netUpdate = true;
                    }
                    else if ((NPC.directionY < 0 || Math.Abs(NPC.velocity.X) > num25) && (!flag7 || !Main.tile[num22, num23 + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num22, num23 + 1].TileType]) && (!Main.tile[num22, num23 + 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[num22, num23 + 2].TileType]) && (!Main.tile[num22 + NPC.direction, num23 + 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num22 + NPC.direction, num23 + 3].TileType])) {
                        NPC.velocity.Y = -6f;
                        NPC.netUpdate = true;
                    }
                }
            }
        }
    }

    public override void AI() {
        NPC.chaseable = _currentAI == 2;
        if (_currentAI == 1) {
            if (Math.Abs(NPC.velocity.Y) <= NPC.gravity) {
                NPC.velocity.X *= 0.85f;
            }
            if (++_extraAITimer >= 30f) {
                _extraAITimer = 0f;
                _currentAI = 0;
                NPC.netUpdate = true;
            }
            NPC.spriteDirection = NPC.direction = Main.player[NPC.target].Center.DirectionFrom(NPC.Center).X.GetDirection();
        }
        if (NPC.localAI[1] > 0f) {
            NPC.localAI[1]--;
        }

        if (_soundTimer++ % 150 == 0 && Main.rand.NextBool(5)) SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "Hog") { Volume = _currentAI == 2 ? 0.6f : 0.2f, Pitch = _currentAI == 2 ? 0.5f : 0f, PitchVariance = 0.2f, MaxInstances = 5 }, NPC.Center);

        int currentAI = _currentAI;
        switch (currentAI) {
            case 0:
                //NPC.aiStyle = 7;
                //AIType = 46;
                //NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -0.9f, 0.9f);

                // adapted vanilla
                // it allows the enemy to jump over 1-3 height tiles
                if (NPC.IsGrounded()) {
                    int collisionWidth = 18;
                    int collisionHeight = 40;
                    int num20 = (int)((NPC.position.X + (float)(collisionWidth / 2) + (float)(15 * NPC.direction)) / 16f);
                    int num21 = (int)((NPC.position.Y + (float)collisionHeight - 16f) / 16f);
                    Tile tileSafely3 = Framing.GetTileSafely(num20, num21);
                    Tile tileSafely4 = Framing.GetTileSafely(num20, num21 - 1);
                    Tile tileSafely5 = Framing.GetTileSafely(num20, num21 - 2);
                    bool flag21 = collisionHeight / 16 < 3;
                    if ((NPC.velocity.X < 0f && NPC.direction == -1) || (NPC.velocity.X > 0f && NPC.direction == 1)) {
                        bool flag22 = false;
                        if (tileSafely5.HasUnactuatedTile && Main.tileSolid[tileSafely5.TileType] && !Main.tileSolidTop[tileSafely5.TileType] && (!flag21 || (tileSafely4.HasUnactuatedTile && Main.tileSolid[tileSafely4.TileType] && !Main.tileSolidTop[tileSafely4.TileType]))) {
                            if (!Collision.SolidTilesVersatile(num20 - NPC.direction * 2, num20 - NPC.direction, num21 - 5, num21 - 1) && !Collision.SolidTiles(num20, num20, num21 - 5, num21 - 3)) {
                                NPC.velocity.Y = -6f;
                                NPC.netUpdate = true;
                            }
                        }
                        else if (tileSafely4.HasUnactuatedTile && Main.tileSolid[tileSafely4.TileType] && !Main.tileSolidTop[tileSafely4.TileType]) {
                            if (!Collision.SolidTilesVersatile(num20 - NPC.direction * 2, num20 - NPC.direction, num21 - 4, num21 - 1) && !Collision.SolidTiles(num20, num20, num21 - 4, num21 - 2)) {
                                NPC.velocity.Y = -5f;
                                NPC.netUpdate = true;
                            }
                            else {
                                flag22 = true;
                            }
                        }
                        else if (NPC.position.Y + (float)collisionHeight - (float)(num21 * 16) > 20f && tileSafely3.HasUnactuatedTile && Main.tileSolid[tileSafely3.TileType] && !tileSafely3.TopSlope) {
                            if (!Collision.SolidTilesVersatile(num20 - NPC.direction * 2, num20, num21 - 3, num21 - 1)) {
                                NPC.velocity.Y = -4.4f;
                                NPC.netUpdate = true;
                            }
                            else {
                                flag22 = true;
                            }
                        }

                        if (flag22) {
                            NPC.direction *= -1;
                            NPC.velocity.X *= -1f;
                            NPC.netUpdate = true;
                        }

                        //if (NPC.velocity.Y < 0f) {
                        //    NPC.velocity.Y *= 1.2f;
                        //}
                    }
                }
                else if (NPC.direction == 0) {
                    NPC.direction = 1;
                    if (NPC.velocity.X < 0f) {
                        NPC.direction = -1;
                    }
                }
                if (NPC.ai[2] == 1f) {
                    if (NPC.direction == 0)
                        NPC.TargetClosest();

                    if (NPC.collideX)
                        NPC.direction *= -1;

                    if (NPC.localAI[3] != 0f && NPC.localAI[2] > NPC.localAI[3] * 0.75f) {
                        NPC.velocity.X *= 0.9f;
                    }
                    else {
                        if (NPC.localAI[3] > 0f) {
                            NPC.localAI[3] = 0f;
                            if (Main.rand.NextChance(0.75)) {
                                NPC.direction *= -1;
                            }
                        }
                        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
                        float acceleration = 0.05f;
                        float speed = 1f;
                        if (NPC.velocity.X < 0f - speed || NPC.velocity.X > speed) {
                            NPC.velocity *= 0.7f;
                        }
                        else if (NPC.velocity.X < speed && NPC.direction == 1) {
                            NPC.velocity.X += acceleration;
                            if (NPC.velocity.X > speed)
                                NPC.velocity.X = speed;
                        }
                        else if (NPC.velocity.X > 0f - speed && NPC.direction == -1) {
                            NPC.velocity.X -= acceleration;
                            if (NPC.velocity.X < 0f - speed)
                                NPC.velocity.X = 0f - speed;
                        }
                    }
                }
                else if (Math.Abs(NPC.velocity.Y) <= NPC.gravity) {
                    NPC.velocity.X *= 0.7f;
                }

                if (Main.netMode != 1) {
                    NPC.localAI[2] -= 1f;
                    if (NPC.localAI[2] <= 0f) {
                        if (NPC.ai[2] == 1f) {
                            NPC.ai[2] = 0f;
                            NPC.localAI[2] = NPC.localAI[3] = Main.rand.Next(300, 900) / 2;
                        }
                        else {
                            NPC.ai[2] = 1f;
                            NPC.localAI[2] = NPC.localAI[3] = Main.rand.Next(600, 1800) / 2;
                        }
                        NPC.netUpdate = true;
                    }
                }
                break;
            case 2:
                //NPC.aiStyle = 26;
                //AIType = 86;
                AdaptedUnicornAI();
                //float distance = Main.player[NPC.target].Distance(NPC.Center);
                //float extraSpeed = (distance < 250f ? 1f + (1f - distance / 250f) : 1f) * 1.25f;
                //float maxSpeed = extraSpeed * 2f;
                //NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);
                break;
        }
    }

    //public override void HitEffect(NPC.HitInfo hit) {
    //	if (Main.netMode == NetmodeID.Server) {
    //		return;
    //	}
    //	if (NPC.life <= 0) {
    //		/*for (int i = 0; i < 4; i++) {
    //			Gore.NewGore(NPC.GetSource_Death(), NPC.NPC.position, NPC.velocity, ModContent.Find<ModGore>(nameof(RiseofAges) + "/VilestDruidGore" + (i + 1).ToString()).Type, 1f);
    //		}
    //		for (int i = 0; i < 20; i++) {
    //			Dust.NewDust(NPC.NPC.position, NPC.width, NPC.height, DustID.Blood, 2.5f * hitNPC.direction, -2.5f, 0, default(DrawColor), 0.7f);
    //		}*/
    //	}
    //	else {
    //		for (int i = 0; i < damage / NPC.lifeMax * 75.0; i++) {
    //			int dust = Dust.NewDust(NPC.NPC.position, NPC.width, NPC.height, DustID.Blood, 0.5f * hitNPC.direction, -0.5f, 0, default, 1f);
    //			Main.dust[dust].NPC.velocity.X *= 0.85f;
    //		}
    //	}
    //}

    public override void EmoteBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects) {
        position.X += NPC.width / 2f * NPC.direction;
    }

    public override void PostAI() {
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            foreach (Player player in Main.ActivePlayers) {
                Item item = player.inventory[player.selectedItem];
                if (player.whoAmI != Main.myPlayer) {
                    return;
                }
                if (item.IsEmpty()) {
                    return;
                }
                if (item.type != ItemID.Acorn) {
                    return;
                }
                if (!Main.mouseRight || !Main.mouseRightRelease) {
                    return;
                }
                Rectangle mouseRect = new((int)Main.MouseWorld.X, (int)Main.MouseWorld.Y, 4, 4);
                Rectangle npcRect = new((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height);
                npcRect.Inflate(5, 5);
                if (NPC.Distance(player.Center) < NPC.width * 2 && mouseRect.Intersects(npcRect) && NPC.localAI[1] <= 0f) {
                    NPC.localAI[1] = 80f;

                    item.stack -= 1;
                    if (item.stack <= 0) {
                        item.SetDefaults();
                    }
                    _currentAI = 1;
                    NPC.target = player.whoAmI;
                    NPC.spriteDirection = NPC.direction = player.Center.DirectionFrom(NPC.Center).X.GetDirection();
                    Color[] particlesColor = {
                        new(169, 148, 91),
                        new(107, 93, 55),
                        new(162, 189, 55)
                    };
                    npcRect = new((int)(NPC.Center.X + 35f * NPC.direction), (int)NPC.Center.Y - 15, 30, 30);
                    SoundEngine.PlaySound(SoundID.Item2, npcRect.Center.ToVector2());
                    for (int i = 0; i < 10; i++) {
                        Vector2 position = npcRect.Center.ToVector2() + Main.rand.NextVector2Square(-10f, 10f);
                        if (NPC.direction == 1) {
                            position.X -= 31f;
                        }
                        Dust.NewDustPerfect(position,
                                            284,
                                            new Vector2?(1.3f * new Vector2((float)NPC.direction, -0.8f).RotatedBy(MathHelper.TwoPi * (double)Main.rand.NextFloatDirection())) * 0.5f,
                                            newColor: particlesColor[Main.rand.Next(particlesColor.Length)],
                                            Scale: ((float)(0.8 + 0.2 * (double)Main.rand.NextFloat()))).fadeIn = 0f;
                    }
                    int healAmount = NPC.lifeMax / 10 + NPC.life / 5;
                    EmoteBubble.NewBubble(0, new WorldUIAnchor(NPC), 50);
                    NPC.life += healAmount;
                    if (NPC.life > NPC.lifeMax) {
                        NPC.life = NPC.lifeMax;
                    }
                    NPC.netUpdate = true;
                }
            }
        }
    }

    public override void FindFrame(int frameHeight) {
        int currentFrame = Math.Min((int)CurrentFrame, Main.npcFrameCount[Type] - 1);
        if (NPC.IsABestiaryIconDummy) {
            if (++NPC.frameCounter >= 7.0) {
                NPC.frameCounter = 0.0;
                CurrentFrame++;
                if (CurrentFrame > 8) {
                    CurrentFrame = 1;
                }
            }

            ChangeFrame((currentFrame, frameHeight));

            return;
        }

        void slowMovementAnimation() {
            if (Math.Abs(NPC.velocity.X) > 0.1f) {
                if (++NPC.frameCounter >= 7.0) {
                    NPC.frameCounter = 0.0;
                    CurrentFrame++;
                    if (CurrentFrame > 8) {
                        CurrentFrame = 1;
                    }
                }
            }
        }

        if (NPC.velocity.Y < 0f) {
            CurrentFrame = 15;
        }
        else if (NPC.velocity.Y > 0f) {
            CurrentFrame = 13;
        }
        else if (Math.Abs(NPC.velocity.X) < 0.1f) {
            CurrentFrame = 0;
        }
        else {
            if (_currentAI == 2) {
                float speed = Math.Abs(NPC.velocity.X);
                if (speed > 2f) {
                    NPC.spriteDirection = NPC.direction = NPC.velocity.X.GetDirection();
                }
                else {
                    NPC.spriteDirection = NPC.direction;
                }
                if (speed < 3f) {
                    slowMovementAnimation();
                }
                else if (++NPC.frameCounter >= Math.Clamp((double)(3f - Math.Abs(NPC.velocity.X)) * 2.0 + 4.0, 6.0, 20.0)) {
                    NPC.frameCounter = 0.0;
                    CurrentFrame++;
                    if (CurrentFrame < 9 || CurrentFrame > 14) {
                        CurrentFrame = 9;
                    }
                }
            }
            else {
                slowMovementAnimation();
                NPC.spriteDirection = NPC.direction;
            }
        }

        ChangeFrame((currentFrame, frameHeight));
    }
}