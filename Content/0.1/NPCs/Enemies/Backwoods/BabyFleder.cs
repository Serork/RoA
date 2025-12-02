using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Content.Biomes.Backwoods;
using RoA.Content.Buffs;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class BabyFleder : ModNPC {
    private enum State {
        Normal,
        Sitting
    }

    private State _state = State.Sitting;
    private bool _alwaysAngry;
    private float _tireTimer, _tireTimer2;

    private bool HasParent => ParentIndex > -1;

    private bool IsSitting => !HasParent && _state == State.Sitting;

    public int ParentIndex {
        get => (int)NPC.ai[2] - 1;
        set => NPC.ai[2] = value + 1;
    }

    private ref float StateTimer => ref NPC.ai[0];
    private ref float AITimer => ref NPC.ai[3];

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write((int)_state);
        writer.Write(_alwaysAngry);
        writer.Write(_tireTimer);
        writer.Write(_tireTimer2);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _state = (State)reader.ReadInt32();
        _alwaysAngry = reader.ReadBoolean();
        _tireTimer = reader.ReadSingle();
        _tireTimer2 = reader.ReadSingle();
    }

    public override void HitEffect(NPC.HitInfo hit) {
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
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "BabyFlederGore2".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X + 14f, NPC.position.Y), NPC.velocity, "BabyFlederGore1".GetGoreType());
        }
    }

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(0f, -12f),
            PortraitPositionXOverride = 1f,
            PortraitPositionYOverride = -33f,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.BabyFleder")
        ]);
    }

    public override void SetDefaults() {
        NPC.lifeMax = 45;
        NPC.damage = 20;
        NPC.defense = 5;
        NPC.knockBackResist = 1.1f;

        int width = 25; int height = 25;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.npcSlots = 0.8f;
        NPC.value = Item.buyPrice(0, 0, 0, 95);

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<BabyFlederBanner>();
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
        if (NPC.downedBoss2) {
            target.AddBuff(ModContent.BuffType<BeastPoison>(), 60);
        }
    }

    public override void OnSpawn(IEntitySource source) {
        if (!HasParent) {
            if (Main.rand.NextBool(10)) {
                _alwaysAngry = true;
                _state = State.Normal;
            }
            else if (Main.rand.NextBool(4)) {
                _state = State.Normal;
            }

            NPC.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 0.75f;
            NPC.netUpdate = true;
        }
    }

    public override bool? CanFallThroughPlatforms() => !IsSitting;

    public override void FindFrame(int frameHeight) {
        int currentFrame = (int)NPC.localAI[0];
        if (NPC.IsABestiaryIconDummy) {
            if (++NPC.frameCounter >= 6.0) {
                NPC.frameCounter = 0.0;
                NPC.localAI[0]++;
                if (NPC.localAI[0] >= 4) {
                    NPC.localAI[0] = 0;
                }
                NPC.frame.Y = currentFrame * frameHeight;
            }

            return;
        }

        NPC.spriteDirection = NPC.direction;
        if (IsSitting && Math.Abs(NPC.velocity.Y) < 0.1f) {
            NPC.frame.Y = 2 * frameHeight;
            return;
        }
        if (++NPC.frameCounter >= (double)Math.Max(10f - Math.Abs(NPC.velocity.Y) * 3.5f, 6f) * 0.75) {
            NPC.frameCounter = 0;
            NPC.localAI[0]++;
            if (NPC.localAI[0] >= 4) {
                NPC.localAI[0] = 0;
            }
        }
        NPC.frame.Y = currentFrame * frameHeight;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        Texture2D texture = NPC.GetTexture();
        spriteBatch.Draw(texture, NPC.position - screenPos + new Vector2(0f, IsSitting ? 3f : 0f) + new Vector2(NPC.width, NPC.height) / 2,
            NPC.frame, NPC.GetNPCColorTintedByBuffs(drawColor), NPC.rotation, new Vector2(texture.Width, texture.Height / Main.npcFrameCount[Type]) / 2, NPC.scale, NPC.spriteDirection != 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        return false;
    }

    private void SitIfTileBelow() {
        if (WorldGen.SolidTile((int)NPC.Bottom.X / 16, (int)NPC.Bottom.Y / 16 + 1) || NPC.collideY) {
            AITimer = 0f;
            _state = State.Sitting;

            NPC.netUpdate = true;
        }
    }

    public override void AI() {
        NPC.noGravity = true;

        bool hasAggro = Main.player[NPC.target].npcTypeNoAggro[Type] && NPC.life >= NPC.lifeMax;
        if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || hasAggro || !Main.player[NPC.target].active) {
            NPC.TargetClosest();
        }
        int target = NPC.target;
        int direction = NPC.direction;
        Player player = Main.player[target];
        NPC parent = HasParent ? Main.npc[ParentIndex] : new NPC();
        bool hasParent = HasParent && parent.active;
        Vector2 position = hasParent ? parent.position : player.position;
        Vector2 center = hasParent ? parent.Center : player.Center;
        if (NPC.velocity.X >= 5f) {
            NPC.velocity.X = 5f;
        }
        else if (NPC.velocity.X <= -5f) {
            NPC.velocity.X = -5f;
        }
        if (hasParent && parent.As<Fleder>().IsAttacking) {
            ResetParentState();
        }
        if (hasParent && !Main.player[parent.target].InModBiome<BackwoodsBiome>()) {
            ResetParentState();
        }

        if (NPC.localAI[1]++ % 200 == 1 && Main.rand.NextBool(2) && NPC.Distance(player.Center) < 150f) SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "PipistrelleScream" + (Main.rand.NextBool(2) ? 1 : 2)) { Volume = 0.8f, PitchVariance = 0.2f, MaxInstances = 5 }, NPC.Center);

        void move(bool flag) {
            if (!HasParent) {
                if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || hasAggro || !Main.player[NPC.target].active) {
                    ApplyGravity();

                    SitIfTileBelow();

                    AITimer = 0f;

                    NPC.netUpdate = true;
                }
                if (!_alwaysAngry) {
                    AITimer += 1f;
                    if (AITimer >= 15f && Main.rand.NextChance(AITimer / 200f)) {
                        ApplyGravity();

                        SitIfTileBelow();
                        NPC.netUpdate = true;
                    }
                }
            }
            hasParent = flag;

            float accelerationX = 0.05f;
            float accelerationY = 0.02f;
            float maxAcceleration = 2f;
            float maxSpeed = 2f;
            float distance = 40f;
            float maxDistance = 350f;
            float distanceBetween = Math.Abs(NPC.position.X + NPC.width / 2 - (position.X + player.width / 2));
            float currentMovement = center.Y - NPC.height / 2;
            bool flag2 = ParentIndex != -1f && HasParent && hasParent;
            NPC.noTileCollide = false;
            if (flag2) {
                NPC.SlightlyMoveTo(center, 5f, 30f * (Utils.GetLerpValue(0f, 100f, NPC.Distance(center), true) + 1f));
                if (NPC.target == 255 || player.dead || hasAggro || Collision.CanHit(NPC.Center, 1, 1, center, 1, 1)) {
                    StateTimer -= 1f;
                    NPC.TargetClosest(false);

                    NPC.netUpdate = true;
                }
                else if (StateTimer > 0f && StateTimer != 90f) {
                    StateTimer = 90f;
                    NPC.TargetClosest(false);

                    NPC.netUpdate = true;
                }
                if (distanceBetween > maxDistance) {
                    NPC.SlightlyMoveTo(position, 25f, 20f);
                    NPC.noTileCollide = true;
                }
                if (StateTimer <= 0f) {
                    SitIfTileBelow();

                    currentMovement = NPC.Center.Y + NPC.directionY * 1000;
                    if (NPC.velocity.X < 0f) {
                        NPC.direction = -1;
                    }
                    else if (NPC.velocity.X > 0f || NPC.direction == 0) {
                        NPC.direction = 1;
                    }
                    if (NPC.position.Y < currentMovement) {
                        NPC.velocity.Y += accelerationY;
                        if (NPC.velocity.Y < 0f) {
                            NPC.velocity.Y += accelerationY;
                        }
                    }
                    else {
                        NPC.velocity.Y -= accelerationY;
                        if (NPC.velocity.Y > 0f) {
                            NPC.velocity.Y -= accelerationY;
                        }
                    }
                }
            }
            currentMovement -= maxDistance / 2f;
            if (NPC.direction == -1 && NPC.velocity.X > 0f - maxAcceleration) {
                NPC.velocity.X -= accelerationX;
                if (NPC.velocity.X > maxAcceleration) {
                    NPC.velocity.X -= accelerationX;
                }
                else if (NPC.velocity.X > 0f) {
                    NPC.velocity.X -= accelerationX / 2f;
                }
                if (NPC.velocity.X < 0f - maxAcceleration) {
                    NPC.velocity.X = 0f - maxAcceleration;
                }
            }
            else if (NPC.direction == 1 && NPC.velocity.X < maxAcceleration) {
                NPC.velocity.X += accelerationX;
                if (NPC.velocity.X < 0f - maxAcceleration) {
                    NPC.velocity.X += accelerationX;
                }
                else if (NPC.velocity.X < 0f) {
                    NPC.velocity.X += accelerationX / 2f;
                }
                if (NPC.velocity.X > maxAcceleration) {
                    NPC.velocity.X = maxAcceleration;
                }
            }
            if (flag2 && !(distanceBetween > maxDistance || distanceBetween < distance)) {
                Vector2 position2 = Vector2.Normalize(center - NPC.Center - NPC.velocity) * 5f;
                float speed2 = 0.1f;
                float maxSpeed2 = 1.5f;
                if (NPC.velocity.X < position2.X) {
                    if (NPC.velocity.X > maxSpeed2) {
                        NPC.velocity.X = maxSpeed2;
                    }
                    NPC.velocity.X += speed2;
                    if (NPC.velocity.X < 0f && position2.X > 0f) {
                        NPC.velocity.X += speed2;
                    }
                }
                else if (NPC.velocity.X > position2.X) {
                    if (NPC.velocity.X < -maxSpeed2) {
                        NPC.velocity.X = -maxSpeed2;
                    }
                    NPC.velocity.X -= speed2;
                    if (NPC.velocity.X > 0f && position2.X < 0f) {
                        NPC.velocity.X -= speed2;
                    }
                }
                else if (NPC.velocity.X > 0f) {
                    NPC.velocity.X -= speed2;
                }
                else if (NPC.velocity.X < 0f) {
                    NPC.velocity.X += speed2;
                }
                else {
                    NPC.velocity.X = 0f;

                }
                speed2 += 0.025f;
                if (NPC.velocity.Y < position2.Y) {
                    if (NPC.velocity.Y > maxSpeed2) {
                        NPC.velocity.Y = maxSpeed2;
                    }
                    NPC.velocity.Y += speed2;
                    if (NPC.velocity.Y < 0f && position2.Y > 0f) {
                        NPC.velocity.Y += speed2;
                    }
                }
                else if (NPC.velocity.Y > position2.Y) {
                    if (NPC.velocity.Y < -maxSpeed2) {
                        NPC.velocity.Y = -maxSpeed2;
                    }
                    NPC.velocity.Y -= speed2;
                    if (NPC.velocity.Y > 0f && position2.Y < 0f) {
                        NPC.velocity.Y -= speed2;
                    }
                }
                else if (NPC.velocity.Y > 0f) {
                    NPC.velocity.Y -= speed2;
                }
                else if (NPC.velocity.Y < 0f) {
                    NPC.velocity.Y += speed2;
                }
                else {
                    NPC.velocity.Y = 0f;
                }
            }
            if (NPC.position.Y < currentMovement) {
                NPC.velocity.Y += accelerationY;
                if (NPC.velocity.Y < 0f) {
                    NPC.velocity.Y += accelerationY;
                }
            }
            else {
                NPC.velocity.Y -= accelerationY;
                if (NPC.velocity.Y > 0f) {
                    NPC.velocity.Y -= accelerationY;
                }
            }
            if (NPC.velocity.Y < 0f - maxSpeed) {
                NPC.velocity.Y = 0f - maxSpeed;
            }
            if (NPC.velocity.Y > maxSpeed) {
                NPC.velocity.Y = maxSpeed;
            }
        }
        if (Math.Abs(NPC.velocity.X) > 0.025f) {
            NPC.rotation = NPC.velocity.X * 0.1f;
        }
        else {
            NPC.rotation = 0f;
        }
        NPC.OffsetTheSameNPC(0.1f);
        NPC.noGravity = true;
        bool flag4 = NPC.Distance(player.Center) < 150f;
        if (!hasParent) {
            ResetParentState();
            if (IsSitting) {
                NPC.velocity *= 0.9f;
                NPC.TargetClosest(false);
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    Rectangle playerRect = new((int)player.position.X, (int)player.position.Y, player.width, player.height);
                    Rectangle npcRect = new((int)NPC.position.X - 200, (int)NPC.position.Y - 300, NPC.width + 400, NPC.height + 600);
                    bool flag5 = hasAggro;
                    if (!flag5 && (flag4 || ((npcRect.Intersects(playerRect) && Collision.CanHit(NPC.Center, 1, 1, center, 1, 1)) || NPC.life < NPC.lifeMax))) {
                        _state = State.Normal;
                        AITimer = 0f;
                        NPC.ai[1] = 17f + Main.rand.NextFloatRange(2f);

                        NPC.netUpdate = true;
                    }
                }
            }
            else {
                hasParent = false;
                move(hasParent);

                NPC.BasicFlier(0.4f * 0.4f, 0.2f * 0.4f, 4f * 0.4f, 2f * 0.4f);

                if (Main.player[NPC.target].InModBiome<BackwoodsBiome>()) {
                    NPC.TargetClosest();
                }

                int num201 = 1;
                int num202 = 1;
                if (NPC.velocity.X < 0f)
                    num201 = -1;

                if (NPC.velocity.Y < 0f)
                    num202 = -1;

                if (!Collision.CanHit(NPC, Main.player[target])) {
                    //NPC.direction = num201;
                    NPC.directionY = num202;
                }

                if (NPC.collideX) {
                    NPC.velocity.X = -NPC.oldVelocity.X * 0.75f;
                    if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < 2f)
                        NPC.velocity.X = 2f;

                    if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -2f)
                        NPC.velocity.X = -2f;
                }

                if (NPC.collideY) {
                    NPC.velocity.Y = -NPC.oldVelocity.Y * 0.75f;
                    if (NPC.velocity.Y > 0f && NPC.velocity.Y < 2f)
                        NPC.velocity.Y = 2f;

                    if (NPC.velocity.Y < 0f && NPC.velocity.Y > -2f)
                        NPC.velocity.Y = -2f;
                }

                if (direction == -1 && NPC.velocity.X > -4f) {
                    NPC.velocity.X -= 0.1f;
                    if (NPC.velocity.X > 4f)
                        NPC.velocity.X -= 0.1f;
                    else if (NPC.velocity.X > 0f)
                        NPC.velocity.X += 0.05f;

                    if (NPC.velocity.X < -4f)
                        NPC.velocity.X = -4f;
                }
                else if (direction == 1 && NPC.velocity.X < 4f) {
                    NPC.velocity.X += 0.1f;
                    if (NPC.velocity.X < -4f)
                        NPC.velocity.X += 0.1f;
                    else if (NPC.velocity.X < 0f)
                        NPC.velocity.X -= 0.05f;

                    if (NPC.velocity.X > 4f)
                        NPC.velocity.X = 4f;
                }

                if (NPC.directionY == -1 && (double)NPC.velocity.Y > -0.65f) {
                    NPC.velocity.Y -= 0.04f;
                    if ((double)NPC.velocity.Y > 0.65f)
                        NPC.velocity.Y -= 0.05f;
                    else if (NPC.velocity.Y > 0f)
                        NPC.velocity.Y += 0.03f;

                    if ((double)NPC.velocity.Y < -0.65f)
                        NPC.velocity.Y = -0.65f;
                }
                else if (NPC.directionY == 1 && (double)NPC.velocity.Y < 1.5) {
                    NPC.velocity.Y += 0.04f;
                    if ((double)NPC.velocity.Y < -1.5)
                        NPC.velocity.Y += 0.05f;
                    else if (NPC.velocity.Y < 0f)
                        NPC.velocity.Y -= 0.03f;

                    if ((double)NPC.velocity.Y > 1.5)
                        NPC.velocity.Y = 1.5f;
                }

                _tireTimer += 1f;
                if (_tireTimer > 200f) {
                    if (Collision.CanHit(NPC, Main.player[NPC.target]))
                        _tireTimer = 0f;

                    if (_tireTimer >= 460f && !Collision.CanHit(NPC, Main.player[NPC.target])) {
                        ApplyGravity();

                        SitIfTileBelow();
                    }
                    if (_tireTimer > 500f) {
                        if (!Collision.CanHit(NPC, Main.player[NPC.target])) {
                            _alwaysAngry = false;
                        }
                        _tireTimer = 0f;
                        NPC.netUpdate = true;
                    }

                    //_tireTimer2 += 1f;
                    //if (_tireTimer2 > 0f) {
                    //    if (NPC.velocity.Y < num214)
                    //        NPC.velocity.Y += num212;
                    //}
                    //else if (NPC.velocity.Y > 0f - num214) {
                    //    NPC.velocity.Y -= num212 / 2f;
                    //}

                    //if (_tireTimer2 < -150f || _tireTimer2 > 150f) {
                    //    if (NPC.velocity.X < num213)
                    //        NPC.velocity.X += num211;
                    //}
                    //else if (NPC.velocity.X > 0f - num213) {
                    //    NPC.velocity.X -= num211;
                    //}

                    //if (_tireTimer >= 260f && !Collision.CanHit(NPC, Main.player[target])) {
                    //    ApplyGravity();

                    //    SitIfTileBelow();
                    //}
                    //if (_tireTimer2 > 300f) {
                    //    //if (!Collision.CanHit(NPC, Main.player[target])) {
                    //    //    _alwaysAngry = false;
                    //    //    NPC.netUpdate = true;
                    //    //}
                    //    _tireTimer2 = -300f;
                    //    NPC.netUpdate = true;
                    //}
                }
            }
        }
        else {
            if (_state != State.Normal) {
                _state = State.Normal;

                NPC.netUpdate = true;
            }
            move(true);
        }

        if (IsSitting) {
            int x = (int)(NPC.Center.X / 16f);
            int y = (int)(NPC.Center.Y / 16f);
            Rectangle tileRect2 = new(x * 16, y * 16, 16, 16);
            if (WorldGenHelper.GetTileSafely(x, y).HasTile && NPC.frame.Intersects(tileRect2)) {
                NPC.position.Y -= 0.2f;
            }
            ApplyGravity();

            if (Math.Abs(NPC.velocity.Y) < 0.1f) {
                NPC.velocity.X *= 0.9f;
            }
        }
        else {
            if (NPC.ai[1] > 0f) {
                NPC.ai[1] -= 1f;
                NPC.velocity.Y -= 0.8f * (NPC.ai[1] / 25f);
            }
        }

        NPC.velocity *= 0.9625f;
    }

    private void ApplyGravity() {
        NPC.velocity.Y += 0.15f;
        NPC.velocity.Y = Math.Min(5f, NPC.velocity.Y);
    }

    private void ResetParentState() {
        if (HasParent) {
            ParentIndex = -1;

            NPC.netUpdate = true;
        }
    }
}