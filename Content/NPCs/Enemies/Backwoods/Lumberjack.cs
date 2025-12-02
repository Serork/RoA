using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Placeable.Banners;
using RoA.Core;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class Lumberjack : RoANPC {
    private const float MAXSPEED = 1.65f;

    private float _timer, _timer2;

    private enum States {
        Spawned,
        Walking,
        Attacking
    }

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 21;
    }

    public override void SetDefaults() {
        NPC.lifeMax = 110;
        NPC.damage = 30;
        NPC.defense = 12;
        NPC.knockBackResist = 0.25f;

        int width = 28; int height = 48;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.npcSlots = 1.25f;
        NPC.value = Item.buyPrice(0, 0, 15, 0);

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<LumberjackBanner>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Lumberjack"),
            new BestiaryPortraitBackgroundProviderPreferenceInfoElement(ModContent.GetInstance<BackwoodsBiome>().ModBiomeBestiaryInfoElement)
        ]);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Coffin>(), 50));
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
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "LumberHead".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "LumberAxe".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "LumberArm".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "LumberLeg".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "LumberLeg".GetGoreType(), Scale: NPC.scale);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }

    public override void OnKill() {
        if (NPC.downedBoss2) {
            if (Main.netMode == NetmodeID.MultiplayerClient) {
                return;
            }

            int npc = NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.PantlessSkeleton);
            if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
                NetMessage.SendData(MessageID.SyncNPC, number: npc);
            }
        }
    }

    public override void FindFrame(int frameHeight) {
        double walkingCounter = 4.0;
        int currentFrame = Math.Min((int)CurrentFrame, Main.npcFrameCount[Type] - 1);
        if (NPC.IsABestiaryIconDummy) {
            NPC.frameCounter += 0.75f;
            if (NPC.frameCounter > walkingCounter) {
                int lastWalkingFrame = 7;
                if (++CurrentFrame >= lastWalkingFrame) {
                    CurrentFrame = 0;
                }
                NPC.frameCounter = 0.0;
                ChangeFrame((currentFrame, frameHeight));
            }

            return;
        }

        NPC.spriteDirection = NPC.direction;
        double attackCounter = walkingCounter + 50.0;
        switch (State) {
            case (float)States.Walking:
                bool isDead = Main.player[NPC.target].dead;
                if (NPC.velocity.Y > 0f || NPC.velocity.Y <= -0.25f) {
                    CurrentFrame = 3;
                }
                else if (NPC.velocity.X == 0f) {
                    CurrentFrame = 0;
                }
                else {
                    NPC.frameCounter += 1f * Math.Abs(NPC.velocity.X / MAXSPEED);
                    if (NPC.frameCounter > walkingCounter) {
                        int lastWalkingFrame = 7;
                        if (++CurrentFrame >= lastWalkingFrame + (isDead ? 6 : 0)) {
                            CurrentFrame = isDead ? 7 : 0;
                        }
                        NPC.frameCounter = 0.0;
                    }
                }
                break;
            case (float)States.Attacking:
                if (NPC.velocity.Y != 0f) {
                    CurrentFrame = 3;
                }
                else {
                    NPC.frameCounter = 0.0;
                    double progress = (double)Helper.EaseInOut3(StateTimer);
                    CurrentFrame = 13 + (int)(8.0 * progress);
                }
                break;
        }
        ChangeFrame((currentFrame, frameHeight));
    }

    public override void SendExtraAI(BinaryWriter writer) {
        base.SendExtraAI(writer);

        writer.Write(_timer);
        writer.Write(_timer2);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        base.ReceiveExtraAI(reader);

        _timer = reader.ReadSingle();
        _timer2 = reader.ReadSingle();
    }

    public override void AI() {
        Player player;
        float closeRange = 65f;
        switch (State) {
            case (float)States.Spawned:
                NPC.TargetClosest();
                player = Main.player[NPC.target];
                if (NPC.position.ToTileCoordinates().Y < Main.worldSurface) {
                    if (NPC.Distance(player.Center) > closeRange && ((NPC.position.X > player.position.X && player.direction == 1) || (NPC.position.X < player.position.X && player.direction == -1))) {
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            NPC.KillNPC();
                            return;
                        }
                    }
                }
                StateTimer = 0.2f;
                ChangeState((int)States.Walking);
                break;
            case (float)States.Walking:
                if (Attack) {
                    Attack = false;
                    NPC.netUpdate = true;
                }
                if (StateTimer > 0f) {
                    StateTimer -= TimeSystem.LogicDeltaTime;
                    //if (NPC.velocity.Y < 0f) {
                    //    NPC.velocity.Y = 0f;
                    //}
                }

                NPC.aiStyle = NPC.ModNPC.AIType = -1;

                NPC npc = NPC;
                if (Main.player[npc.target].position.Y + (float)Main.player[npc.target].height == npc.position.Y + (float)npc.height)
                    npc.directionY = -1;

                bool flag = false;
                bool canOpenDoor2 = false;
                bool flag6 = false;
                if (npc.velocity.X == 0f)
                    flag6 = true;

                if (npc.justHit)
                    flag6 = false;

                flag6 = false;

                bool targetPlayer = !Main.IsItDay();
                int num56 = 60;

                bool flag7 = false;
                bool canOpenDoor = true;

                bool flag9 = false;

                bool flag10 = true;

                if (!flag9 && flag10) {
                    if (NPC.IsGrounded() && ((npc.velocity.X > 0f && npc.direction < 0) || (npc.velocity.X < 0f && npc.direction > 0)))
                        flag7 = true;

                    if (npc.position.X == npc.oldPosition.X || npc.ai[3] >= (float)num56 || flag7)
                        npc.ai[3] += 1f;
                    else if ((double)Math.Abs(npc.velocity.X) > 0.9 && npc.ai[3] > 0f)
                        npc.ai[3] -= 1f;

                    if (npc.ai[3] > (float)(num56 * 10))
                        npc.ai[3] = 0f;

                    if (npc.justHit)
                        npc.ai[3] = 0f;

                    if (npc.ai[3] == (float)num56)
                        npc.netUpdate = true;

                    if (Main.player[npc.target].Hitbox.Intersects(npc.Hitbox))
                        npc.ai[3] = 0f;
                }

                bool shouldTargetPlayer = Terraria.NPC.DespawnEncouragement_AIStyle3_Fighters_NotDiscouraged(npc.type, npc.position, npc);

                shouldTargetPlayer = npc.life < (int)(npc.lifeMax * 0.8f) || (Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].InModBiome<BackwoodsBiome>() && targetPlayer);
                if (npc.ai[3] < (float)num56 && shouldTargetPlayer) {
                    npc.TargetClosest();
                    if (npc.directionY > 0 && Main.player[npc.target].Center.Y <= npc.Bottom.Y)
                        npc.directionY = -1;
                }
                else if (!(_timer > 0f) || !Terraria.NPC.DespawnEncouragement_AIStyle3_Fighters_CanBeBusyWithAction(npc.type)) {
                    bool flag12 = targetPlayer/*Main.player[npc.target].InModBiome<BackwoodsBiome>()*/;
                    if (!flag12 && (double)(npc.position.Y / 16f) < Main.worldSurface/* && npc.type != 624 && npc.type != 631*/) {
                        npc.EncourageDespawn(10);
                    }

                    if (npc.velocity.X == 0f) {
                        if (NPC.IsGrounded()) {
                            _timer2 += 1f;
                            if (_timer2 >= 2f) {
                                npc.direction *= -1;
                                npc.spriteDirection = npc.direction;
                                _timer2 = 0f;
                            }
                        }
                    }
                    else {
                        _timer2 = 0f;
                    }

                    if (npc.direction == 0)
                        npc.direction = 1;
                }

                float num87 = 1f * 1.25f;
                float num88 = 0.07f * 1.25f;
                if (npc.velocity.X < 0f - num87 || npc.velocity.X > num87) {
                    if (NPC.IsGrounded())
                        npc.velocity *= 0.7f;
                }
                else if (npc.velocity.X < num87 && npc.direction == 1) {
                    npc.velocity.X += num88;
                    if (npc.velocity.X > num87)
                        npc.velocity.X = num87;
                }
                else if (npc.velocity.X > 0f - num87 && npc.direction == -1) {
                    npc.velocity.X -= num88;
                    if (npc.velocity.X < 0f - num87)
                        npc.velocity.X = 0f - num87;
                }

                bool tileChecks = false;
                if (NPC.IsGrounded()) {
                    int num77 = (int)(NPC.position.Y + NPC.height + 7f) / 16;
                    int num189 = (int)NPC.position.X / 16;
                    int num79 = (int)(NPC.position.X + NPC.width) / 16;
                    for (int num80 = num189; num80 <= num79; num80++) {
                        if (Main.tile[num80, num77].HasUnactuatedTile && Main.tileSolid[Main.tile[num80, num77].TileType]) {
                            tileChecks = true;
                            break;
                        }
                    }
                }
                if (NPC.velocity.Y >= 0f) {
                    int direction = Math.Sign(NPC.velocity.X);

                    Vector2 position3 = NPC.position;
                    position3.X += NPC.velocity.X;
                    int num82 = (int)((position3.X + NPC.width / 2 + (NPC.width / 2 + 1) * direction) / 16f);
                    int num83 = (int)((position3.Y + NPC.height - 1f) / 16f);
                    if (num82 * 16 < position3.X + NPC.width && num82 * 16 + 16 > position3.X && (Main.tile[num82, num83].HasUnactuatedTile && !Main.tile[num82, num83].TopSlope && !Main.tile[num82, num83 - 1].TopSlope && Main.tileSolid[Main.tile[num82, num83].TileType] && !Main.tileSolidTop[Main.tile[num82, num83].TileType] || Main.tile[num82, num83 - 1].IsHalfBlock && Main.tile[num82, num83 - 1].HasUnactuatedTile) && (!Main.tile[num82, num83 - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 1].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 1].TileType] || Main.tile[num82, num83 - 1].IsHalfBlock && (!Main.tile[num82, num83 - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 4].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 4].TileType])) && (!Main.tile[num82, num83 - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 2].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 2].TileType]) && (!Main.tile[num82, num83 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82, num83 - 3].TileType] || Main.tileSolidTop[Main.tile[num82, num83 - 3].TileType]) && (!Main.tile[num82 - direction, num83 - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[num82 - direction, num83 - 3].TileType])) {
                        float num84 = num83 * 16;
                        if (Main.tile[num82, num83].IsHalfBlock) {
                            num84 += 8f;
                        }

                        if (Main.tile[num82, num83 - 1].IsHalfBlock) {
                            num84 -= 8f;
                        }
                        if (num84 < position3.Y + NPC.height) {
                            float num85 = position3.Y + NPC.height - num84;
                            float num86 = 16.1f;
                            if (NPC.type == NPCID.BlackRecluse || NPC.type == NPCID.WallCreeper || NPC.type == NPCID.JungleCreeper || NPC.type == NPCID.BloodCrawler || NPC.type == NPCID.DesertScorpionWalk) {
                                num86 += 8f;
                            }

                            if (num85 <= num86) {
                                NPC.gfxOffY += NPC.position.Y + NPC.height - num84;
                                NPC.position.Y = num84 - NPC.height;
                                if (num85 < 9f) {
                                    NPC.stepSpeed = 1f;
                                }
                                else {
                                    NPC.stepSpeed = 2f;
                                }
                            }
                        }
                    }
                }
                if (tileChecks && !Main.tile[(int)(NPC.Center.X) / 16, (int)(NPC.Center.Y - 15f) / 16 - 1].HasUnactuatedTile) {
                    int tileX = (int)((NPC.position.X + NPC.width / 2 + 15 * NPC.direction) / 16f);
                    int tileY = (int)((NPC.position.Y + NPC.height - 15f) / 16f);

                    {
                        if (NPC.velocity.X < 0f && NPC.direction == -1 || NPC.velocity.X > 0f && NPC.direction == 1) {
                            void jumpIfPlayerAboveAndClose() {
                                if (NPC.IsGrounded() && Main.expertMode && Main.player[npc.target].Bottom.Y < npc.Top.Y && Math.Abs(npc.Center.X - Main.player[npc.target].Center.X) < (float)(Main.player[npc.target].width * 3) && Collision.CanHit(npc, Main.player[npc.target])) {
                                    if (NPC.IsGrounded()) {
                                        int num200 = 6;
                                        if (Main.player[npc.target].Bottom.Y > npc.Top.Y - (float)(num200 * 16)) {
                                            npc.velocity.Y = -7.9f;
                                        }
                                        else {
                                            int num201 = (int)(npc.Center.X / 16f);
                                            int num202 = (int)(npc.Bottom.Y / 16f) - 1;
                                            for (int num203 = num202; num203 > num202 - num200; num203--) {
                                                if (Main.tile[num201, num203].HasUnactuatedTile && TileID.Sets.Platforms[Main.tile[num201, num203].TileType]) {
                                                    npc.velocity.Y = -7.9f;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            jumpIfPlayerAboveAndClose();

                            bool JumpCheck(int tileX, int tileY) {
                                if (NPC.height >= 32 && Main.tile[tileX, tileY - 2].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 2].TileType]) {
                                    if (Main.tile[tileX, tileY - 3].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 3].TileType]) {
                                        NPC.velocity.Y = -8f;
                                        NPC.netUpdate = true;
                                    }
                                    else {
                                        NPC.velocity.Y = -7f;
                                        NPC.netUpdate = true;
                                    }
                                    return true;
                                }
                                else if (Main.tile[tileX, tileY - 1].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY - 1].TileType]) {
                                    NPC.velocity.Y = -6f;
                                    NPC.netUpdate = true;
                                    return true;
                                }
                                else if (NPC.position.Y + NPC.height - tileY * 16 > 20f && Main.tile[tileX, tileY].HasUnactuatedTile && !Main.tile[tileX, tileY].TopSlope && Main.tileSolid[Main.tile[tileX, tileY].TileType]) {
                                    NPC.velocity.Y = -5f;
                                    NPC.netUpdate = true;
                                    return true;
                                }
                                else if (NPC.directionY < 0 && (!Main.tile[tileX, tileY + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY + 1].TileType]) && (!Main.tile[tileX + NPC.direction, tileY + 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX + NPC.direction, tileY + 1].TileType])) {
                                    NPC.velocity.Y = -8f;
                                    NPC.velocity.X *= 1.5f;
                                    NPC.netUpdate = true;
                                    return true;
                                }
                                return false;
                            }
                            if (!JumpCheck(tileX, tileY)) {
                            }
                        }
                    }
                }
                if (npc.IsGrounded()) {
                    Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
                }

                player = Main.player[NPC.target];
                if (shouldTargetPlayer && Collision.CanHit(NPC, player)) {
                    if (NPC.Distance(player.Center) < closeRange && !player.dead) {
                        StateTimer = 0.2f;
                        ChangeState((int)States.Attacking);
                    }
                }
                break;
            case (float)States.Attacking:
                player = Main.player[NPC.target];
                bool inRange = NPC.Distance(player.Center) >= closeRange;
                if ((inRange || player.dead) && StateTimer <= 0.2f) {
                    StateTimer = 0.1f;
                    ChangeState((int)States.Walking);
                    NPC.TargetClosest();
                    return;
                }
                if (NPC.IsGrounded()) {
                    NPC.ResetAIStyle();
                    if (NPC.IsGrounded()) {
                        NPC.velocity.X *= 0.8f;
                    }
                    StateTimer += TimeSystem.LogicDeltaTime / 2f;
                    StateTimer *= 1.05f;
                    if (StateTimer >= 0.6f) {
                        if (!Attack) {
                            Attack = true;
                            SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                            if (Main.netMode != NetmodeID.MultiplayerClient) {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(NPC.width / 2 * NPC.direction + 10, 0f), Vector2.Zero, ModContent.ProjectileType<LumberjackAxeSlash>(),
                                    60 / 2, 3f, Main.myPlayer);
                            }
                            NPC.netUpdate = true;
                        }
                        if (StateTimer >= 1f) {
                            StateTimer = 0f;
                            Attack = false;
                            NPC.netUpdate = true;
                        }
                    }
                }
                break;
        }
    }

    private class LumberjackAxeSlash : ModProjectile {
        public override string Texture => ResourceManager.EmptyTexture;

        public override bool PreDraw(ref Color lightColor) => false;

        public override void SetDefaults() {
            int width = 120; int height = 60;
            Projectile.Size = new Vector2(width, height);

            Projectile.friendly = false;
            Projectile.hostile = true;

            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 10;

            Projectile.tileCollide = false;

            Projectile.alpha = byte.MaxValue;
        }
    }
}