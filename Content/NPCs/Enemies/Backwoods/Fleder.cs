using Microsoft.Xna.Framework;

using RoA.Common.BackwoodsSystems;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Buffs;
using RoA.Content.Items.Placeable.Banners;
using RoA.Content.Tiles.Platforms;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class Fleder : ModNPC {
    private enum State {
        Normal,
        Sitting,
        Attacking
    }

    private State _state = State.Normal;
    private Vector2 _sittingPosition = Vector2.Zero;

    private bool IsSittingOnBranch => _state == State.Sitting && _sittingPosition != Vector2.Zero;
    public bool IsAttacking => _state == State.Attacking;

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write((int)_state);
        writer.WriteVector2(_sittingPosition);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        _state = (State)reader.ReadInt32();
        _sittingPosition = reader.ReadVector2();
    }

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 4;

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;

        var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers() {
            Position = new Vector2(0f, 17f)
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);
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
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "FlederGore2".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "FlederGore1".GetGoreType());
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "FlederGore1".GetGoreType());
        }
    }

    public override void SetDefaults() {
        NPC.lifeMax = 95;
        NPC.damage = 40;
        NPC.defense = 7;
        NPC.knockBackResist = 0.5f;

        int width = 30; int height = 30;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.value = Item.buyPrice(0, 0, 2, 0);

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;

        NPC.noGravity = true;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<FlederBanner>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Fleder")
        ]);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) => target.AddBuff(ModContent.BuffType<BeastPoison>(), 90);

    public override bool? CanFallThroughPlatforms() => true;

    public override void OnSpawn(IEntitySource source) {
        NPC.velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 0.75f;
        NPC.netUpdate = true;
    }

    public override void AI() {
        if (!IsSittingOnBranch) {
            NPC.OffsetTheSameNPC();
        }

        if (!IsSittingOnBranch) {
            float rotation = NPC.rotation;
            if (NPC.velocity.Y != 0f && Math.Abs(NPC.velocity.X) > 0.05f) {
                rotation = NPC.velocity.X * (MathHelper.PiOver2 * 0.135f);
            }
            rotation = (float)Math.Sin(rotation) * (float)Math.PI * 0.12f;
            NPC.rotation = rotation;
        }
        else {
            NPC.rotation = Helper.SmoothAngleLerp(NPC.rotation, 0f, 0.25f);
        }

        Rectangle playerRect;
        Rectangle npcRect = new((int)NPC.position.X - 250, (int)NPC.position.Y - 350, NPC.width + 500, NPC.height + 700);
        bool hasAggro = Main.player[NPC.target].npcTypeNoAggro[Type] && NPC.life >= NPC.lifeMax;
        bool isTriggeredBy(Player player) {
            playerRect = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
            if (player.npcTypeNoAggro[Type] && NPC.life >= NPC.lifeMax) {
                return false;
            }
            return (npcRect.Intersects(playerRect) || NPC.life < NPC.lifeMax) && !player.dead && player.active && player.InModBiome<BackwoodsBiome>();
        }
        void flyAway() {
            float maxSpeed = 4.5f;
            if (NPC.velocity.Y < -maxSpeed) {
                NPC.velocity.Y = -maxSpeed;
            }
            float speedY = 0.075f * 0.6f;
            NPC.velocity.Y -= speedY + speedY / 2f * Main.rand.NextFloat();
            float speedX = NPC.direction * 0.1f * 0.6f;
            NPC.velocity.X += speedX + speedX / 2f * Main.rand.NextFloat();
            if (NPC.velocity.X < -maxSpeed) {
                NPC.velocity.X = -maxSpeed;
            }
            if (NPC.velocity.X > maxSpeed) {
                NPC.velocity.X = maxSpeed;
            }
        }
        if (IsSittingOnBranch) {
            //Helper.InertiaMoveTowards(ref NPC.velocity, NPC.Center, _sittingPosition, minDistance: 5f);
            NPC.Center = Vector2.SmoothStep(NPC.Center, _sittingPosition, 0.4f);
            //NPC.velocity = Vector2.Zero;

            if (Main.netMode != NetmodeID.MultiplayerClient) {
                foreach (Player activePlayer in Main.ActivePlayers) {
                    if (isTriggeredBy(activePlayer)) {
                        NPC.target = activePlayer.whoAmI;
                        _state = State.Attacking;

                        NPC.velocity.Y -= 5f;

                        NPC.netUpdate = true;
                    }
                }
            }

            return;
        }

        Player player = Main.player[NPC.target];
        playerRect = new((int)player.position.X, (int)player.position.Y, player.width, player.height);
        if (!IsAttacking) {
            List<NPC> others = Main.npc.Where(npc => npc.active && npc.type == NPC.type && npc.whoAmI != NPC.whoAmI).ToList();
            Tile centerTile = WorldGenHelper.GetTileSafely((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16);
            Point tile;
            Point? treeBranch = null;
            float nearestTile = 0f;
            bool flag2 = (int)(NPC.Center.Y / 16) > BackwoodsVars.FirstTileYAtCenter + 15;
            if ((!centerTile.AnyWall() && flag2) || !flag2) {
                nearestTile = NPC.SearchForNearestTile<TreeBranch>(out tile, out treeBranch, (tilePosition) => {
                    if (others.Any(npc => npc.As<Fleder>()._state == State.Sitting && npc.WithinRange(tilePosition.ToWorldCoordinates(), 20f))) {
                        return false;
                    }

                    return true;
                }, 30);
            }
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                foreach (Player activePlayer in Main.ActivePlayers) {
                    if (isTriggeredBy(activePlayer)) {
                        NPC.target = activePlayer.whoAmI;
                        _state = State.Attacking;

                        NPC.netUpdate = true;
                    }
                }
            }
            if (nearestTile >= 25 * 25 && treeBranch != null && NPC.life >= NPC.lifeMax * 0.5f) {
                NPC.noTileCollide = true;

                NPC.localAI[1] = 0f;

                NPC.localAI[2] += 1f * Main.rand.NextFloat();
                Vector2 destination = treeBranch.Value.ToWorldCoordinates();
                if (Collision.CanHitLine(NPC.Center, 0, 0, destination, 0, 0)) {
                    Helper.InertiaMoveTowards(ref NPC.velocity, NPC.Center, destination, minDistance: 25f, max: true);
                }
                else if (NPC.localAI[2] >= 15f) {
                    NPC.localAI[2] = 0f;
                    if (NPC.velocity.LengthSquared() < 1f) {
                        NPC.velocity = new Vector2(1.2f, 0f).RotatedByRandom(MathHelper.TwoPi);
                    }
                    else {
                        NPC.velocity = NPC.velocity.RotatedBy(Main.rand.Next(-1, 2) * 0.2f);
                    }
                    NPC.netUpdate = true;
                }

                if (Vector2.DistanceSquared(NPC.Center + NPC.velocity, destination) <= 15f * 15f && Math.Abs(NPC.Center.X + NPC.velocity.X - destination.X) <= 15f) {
                    //NPC.Center = destination;
                    NPC.velocity = Vector2.Zero;
                    //NPC._rotation = 0f;

                    _sittingPosition = destination;
                    _state = State.Sitting;

                    NPC.netUpdate = true;
                }

                if (Math.Abs(NPC.velocity.X) > 0.05f) {
                    NPC.direction = NPC.velocity.X.GetDirection();
                }

                return;
            }
            else if (player.dead || hasAggro || !player.active || !player.InModBiome<BackwoodsBiome>()) {
                int y = (int)NPC.Center.Y / 16;
                centerTile = WorldGenHelper.GetTileSafely((int)NPC.Center.X / 16, y);
                if (!centerTile.AnyWall() && y < BackwoodsVars.FirstTileYAtCenter + 15) {
                    flyAway();
                }
            }
        }

        NPC.localAI[1] += 1f;
        int y2 = (int)NPC.Center.Y / 16;
        Tile centerTile2 = WorldGenHelper.GetTileSafely((int)NPC.Center.X / 16, y2);
        bool flag = !centerTile2.AnyWall() && y2 < BackwoodsVars.FirstTileYAtCenter + 15;
        if (player.dead || hasAggro || !player.active || !player.InModBiome<BackwoodsBiome>()) {
            NPC.TargetClosest();
            if (player.dead || hasAggro || !player.active || !player.InModBiome<BackwoodsBiome>()) {
                if (flag && _state != State.Normal) {
                    _state = State.Normal;

                    NPC.netUpdate = true;
                }
            }
        }

        if (NPC.localAI[1] % 200 == 1 && Main.rand.NextBool(2) && NPC.Distance(player.Center) < 150f) SoundEngine.PlaySound(new SoundStyle(ResourceManager.NPCSounds + "PipistrelleScream" + (Main.rand.NextBool(2) ? 1 : 2)) { PitchVariance = 0.2f, MaxInstances = 5 }, NPC.Center);

        NPC.noTileCollide = false;

        if (!flag || (!player.dead && !hasAggro && player.InModBiome<BackwoodsBiome>())) {
            if (NPC.localAI[1] > 100f) {
                _state = State.Attacking;

                NPC.netUpdate = true;
            }

            NPC.TargetClosest();

            npcRect = new((int)NPC.position.X, (int)NPC.position.Y, NPC.width, NPC.height);
            if (npcRect.Intersects(playerRect) || NPC.collideX) {
                float maxSpeedAfterCollideX = 4f;
                float speedAfterCollideX = 2.5f;
                NPC.velocity.X = NPC.oldVelocity.X * -0.5f;
                if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < maxSpeedAfterCollideX) {
                    NPC.velocity.X = speedAfterCollideX;
                }
                if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -maxSpeedAfterCollideX) {
                    NPC.velocity.X = -speedAfterCollideX;
                }
            }
            if (npcRect.Intersects(playerRect) || NPC.collideY) {
                float maxSpeedAfterCollideY = 5f;
                float speedAfterCollideY = 3f;
                NPC.velocity.Y = NPC.oldVelocity.Y * -0.5f;
                if (NPC.velocity.Y > 0f && NPC.velocity.Y < maxSpeedAfterCollideY) {
                    NPC.velocity.Y = speedAfterCollideY;
                }
                if (NPC.velocity.Y < 0f && NPC.velocity.Y > -maxSpeedAfterCollideY) {
                    NPC.velocity.Y = -speedAfterCollideY;
                }
            }

            NPC.TargetClosest(true);
            float speedX = 0.15f;
            float maxSpeedX = 5f;
            if (NPC.direction == -1 && NPC.velocity.X > -maxSpeedX) {
                NPC.velocity.X -= speedX;
                if (NPC.velocity.X > maxSpeedX) {
                    NPC.velocity.X -= 1f;
                }
                else if (NPC.velocity.X > 0f) {
                    NPC.velocity.X -= speedX / 5f;
                }
                if (NPC.velocity.X < -maxSpeedX) {
                    NPC.velocity.X = -maxSpeedX;
                }
            }
            else if (NPC.direction == 1 && NPC.velocity.X < maxSpeedX) {
                NPC.velocity.X += speedX;
                if (NPC.velocity.X < -maxSpeedX) {
                    NPC.velocity.X += speedX;
                }
                else if (NPC.velocity.X < 0f) {
                    NPC.velocity.X += speedX / 5f;
                }
                if (NPC.velocity.X > maxSpeedX) {
                    NPC.velocity.X = maxSpeedX;
                }
            }
            if (!player.dead && !hasAggro) {
                float distX = Math.Abs((float)(NPC.position.X + (double)(NPC.width / 2) - ((player.position.X - player.oldVelocity.Y) + (double)(player.width / 2))));
                float distY = player.position.Y - NPC.height / 2;
                float minX = 50f;
                float upY = 200f;
                if (distX > minX && player.FindBuffIndex(ModContent.BuffType<BeastPoison>()) == -1) {
                    distY -= upY;
                }
                float speedY = 0.15f;
                if (NPC.position.Y < distY) {
                    NPC.velocity.Y += speedY;
                    if (NPC.velocity.Y < 0f) {
                        NPC.velocity.Y += speedY / 5f;
                    }
                }
                else {
                    NPC.velocity.Y -= speedY;
                    if (NPC.velocity.Y > 0f) {
                        NPC.velocity.Y -= speedY / 5f;
                    }
                }
                float maxSpeedY = 5f;
                if (NPC.velocity.Y < -maxSpeedY) {
                    NPC.velocity.Y = -maxSpeedY;
                }
                if (NPC.velocity.Y > maxSpeedY) {
                    NPC.velocity.Y = maxSpeedY;
                }
            }

            if (NPC.collideX) {
                NPC.velocity.X = -NPC.oldVelocity.X;
                if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < 2f)
                    NPC.velocity.X = 2f;

                if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -2f)
                    NPC.velocity.X = -2f;
            }

            if (NPC.collideY) {
                NPC.velocity.Y = -NPC.oldVelocity.Y;
                if (NPC.velocity.Y > 0f && NPC.velocity.Y < 1f)
                    NPC.velocity.Y = 1f;

                if (NPC.velocity.Y < 0f && NPC.velocity.Y > -1f)
                    NPC.velocity.Y = -1f;
            }
        }
        else {
            flyAway();
        }
    }

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = NPC.direction;
        if (NPC.IsABestiaryIconDummy) {
            if (++NPC.frameCounter >= 6.0) {
                NPC.frameCounter = 0.0;
                NPC.localAI[0]++;
                if (NPC.localAI[0] >= 4) {
                    NPC.localAI[0] = 0;
                }
                int currentFrame = (int)NPC.localAI[0];
                NPC.frame.Y = currentFrame * frameHeight;
            }

            return;
        }

        if (_state == State.Sitting) {
            if (NPC.WithinRange(_sittingPosition, 2f)) {
                NPC.frame.Y = 2 * frameHeight;

                return;
            }
        }

        if (++NPC.frameCounter >= (double)Math.Max(10f - Math.Abs(NPC.velocity.Y) * 2f, 4f)) {
            NPC.frameCounter = 0.0;
            NPC.localAI[0]++;
            if (NPC.localAI[0] >= 4) {
                NPC.localAI[0] = 0;
            }
            int currentFrame = (int)NPC.localAI[0];
            NPC.frame.Y = currentFrame * frameHeight;
        }
    }
}
