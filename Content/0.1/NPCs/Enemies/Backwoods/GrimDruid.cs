using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.BackwoodsSystems;
using RoA.Common.WorldEvents;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Dusts;
using RoA.Content.Items.Equipables.Accessories;
using RoA.Content.Items.Placeable.Banners;
using RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;
using RoA.Content.Projectiles.Enemies;
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

sealed class GrimDruid : DruidNPC {
    protected override Color MagicCastColor => new(234, 15, 35, 0);

    private float _timer, _timer2;

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

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 19;
    }

    public override void SetDefaults() {
        base.SetDefaults();

        NPC.lifeMax = 180;
        NPC.damage = 25;
        NPC.defense = 10;

        int width = 28; int height = 48;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.npcSlots = 1.25f;
        NPC.value = Item.buyPrice(0, 0, 3, 0);

        DrawOffsetY = -2;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        Banner = Type;
        BannerItem = ModContent.ItemType<GrimdruidBanner>();
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Grimdruid")
        ]);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BandOfNature>(), 20));
    }

    public override void HitEffect(NPC.HitInfo hit) {
        int type = ModContent.NPCType<DruidSoul>();
        if (NPC.life <= 0 && NPC.downedBoss2 && Main.rand.NextBool(5) && !NPC.AnyNPCs(type)) {
            if (NPC.Center.Y / 16 < BackwoodsVars.FirstTileYAtCenter + 50) {
                bool flag6 = LothorSummoningHandler.PreArrivedLothorBoss.Item1 || LothorSummoningHandler.PreArrivedLothorBoss.Item2;
                if (!flag6 && Main.netMode != NetmodeID.MultiplayerClient) {
                    int npc = NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, type);
                    if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
                        NetMessage.SendData(MessageID.SyncNPC, number: npc);
                    }
                }
            }
        }

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
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "VilestDruidGore2".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "VilestDruidGore1".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "VilestDruidGore3".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "VilestDruidGore4".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "VilestDruidGore4".GetGoreType(), Scale: NPC.scale);
        }
    }

    protected override float TimeToChangeState() => 2f;
    protected override float TimeToRecoveryAfterGettingHit() => 1f;
    protected override (Func<bool>, float) ShouldBeAttacking() => (() => true, 450f);

    protected override void Walking() {
        NPC.knockBackResist = 0.35f;

        NPC.aiStyle = NPC.ModNPC.AIType = -1;

        NPC npc = NPC;
        if (Main.player[npc.target].position.Y + (float)Main.player[npc.target].height == npc.position.Y + (float)npc.height)
            npc.directionY = -1;

        bool targetPlayer = true;
        bool shouldTargetPlayer = Terraria.NPC.DespawnEncouragement_AIStyle3_Fighters_NotDiscouraged(npc.type, npc.position, npc);

        shouldTargetPlayer = npc.life < (int)(npc.lifeMax * 0.8f) || (Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].InModBiome<BackwoodsBiome>() && targetPlayer);

        bool flag = false;
        bool canOpenDoor2 = false;
        bool flag6 = false;
        if (npc.velocity.X == 0f)
            flag6 = true;

        if (npc.justHit)
            flag6 = false;

        flag6 = false;

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

        float num87 = 1f * 0.8f;
        float num88 = 0.07f * 0.8f;
        //num87 += (1f - (float)life / (float)lifeMax) * 1.5f;
        //num88 += (1f - (float)life / (float)lifeMax) * 0.15f;
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
        //npc.directionY = 1;
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
        int tileX = (int)((NPC.position.X + NPC.width / 2 + 15 * NPC.direction) / 16f);
        int tileY = (int)((NPC.position.Y + NPC.height - 15f) / 16f);
        int num194 = tileX;
        int num195 = tileY;
        if (tileChecks && !Main.tile[(int)(NPC.Center.X) / 16, (int)(NPC.Center.Y - 15f) / 16 - 1].HasUnactuatedTile) {

            {
                if (NPC.velocity.X < 0f && NPC.spriteDirection == -1 || NPC.velocity.X > 0f && NPC.spriteDirection == 1) {
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
        //Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
    }

    protected override void Attacking() {
        if (Math.Abs(NPC.velocity.Y) <= NPC.gravity) {
            NPC.ResetAIStyle();
            Player player = Main.player[NPC.target];
            Vector2 position = new(player.Center.X, player.Center.Y + 32);
            NPC.direction = player.Center.DirectionFrom(NPC.Center).X.GetDirection();
            if (NPC.IsGrounded()) {
                NPC.velocity.X *= 0.8f;
            }
            if (AttackEndTimer >= 0f) {
                StateTimer += TimeSystem.LogicDeltaTime;
                if (StateTimer >= 0.17f) {
                    Attack = true;
                    //NPC.netUpdate = true;
                }
                if (StateTimer >= 0.25f) {
                    AttackEffects(position);
                }
                if (StateTimer >= 1.75f) {
                    GrimDruidAttack(position);
                    if (Main.netMode != NetmodeID.MultiplayerClient) {
                        AttackType = Main.rand.Next(0, 2);
                        NPC.netUpdate = true;
                    }
                    AttackTimer = -TimeToChangeState();
                    AttackEndTimer = -0.3f;
                }
                //NPC.netUpdate = true;
            }
            else {
                AttackEffects(position);
                StateTimer = 1.75f;
                AttackEndTimer += TimeSystem.LogicDeltaTime;
                if (AttackEndTimer >= -0.05f) {
                    StateTimer = 0f;
                    AttackEndTimer = 0f;
                    Attack = false;
                    ChangeState((int)States.Walking);
                }
            }
        }
    }

    private void GrimDruidAttack(Vector2 position) {
        ushort dustType = (ushort)ModContent.DustType<GrimDruidDust>();
        SoundEngine.PlaySound(SoundID.Item8, new Vector2(NPC.position.X, NPC.position.Y));
        for (int i = 0; i < 15; i++) {
            int dust = Dust.NewDust(NPC.position + NPC.velocity + new Vector2(-2f, 8f), NPC.width + 4, NPC.height - 16, dustType, 0f, -2f, 255, new Color(255, 0, 25), Main.rand.NextFloat(0.8f, 1.2f));
            Main.dust[dust].velocity.Y *= 0.4f;
            Main.dust[dust].velocity.X *= 0.1f;
        }
        if (AttackType == 0) {
            SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            Vector2 directionNormalized = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center);
            if (Main.netMode != NetmodeID.MultiplayerClient) {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.Center.X + 18 * NPC.direction, NPC.Center.Y), new Vector2(directionNormalized.X * 5, directionNormalized.Y * 5), ModContent.ProjectileType<GrimBranch>(),
                    35 / 2, 1f, Main.myPlayer, 100, 0f);
            }
            return;
        }
        int positionX = (int)PlayersOldPosition.X / 16;
        int positionY = (int)PlayersOldPosition.Y / 16;
        Point point = new(positionX, positionY);
        Point point2 = point;
        while (!WorldGen.SolidTile(point2)) {
            if (TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(point2.X, point2.Y).TileType]) {
                break;
            }

            point2.Y++;
        }
        positionX = point2.X;
        positionY = point2.Y;
        while (!Framing.GetTileSafely(positionX, positionY - 1).HasTile || !WorldGen.SolidTile2(positionX, positionY - 1)) {
            positionY++;
        }
        SoundEngine.PlaySound(SoundID.Item76, position);
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(positionX * 16f + 8, positionY * 16f - 30), Vector2.Zero, ModContent.ProjectileType<VileSpike>(),
                45 / 2, 2f, Main.myPlayer);
        }
    }

    private void AttackEffects(Vector2 position) {
        if (AttackEndTimer < 0f) {
            return;
        }

        if (AttackType == 1) {
            NPC.localAI[3] += TimeSystem.LogicDeltaTime;
            if (NPC.localAI[3] > 0.05f) {
                PlayersOldPosition = position;
                NPC.localAI[3] = 0f;
                //NPC.netUpdate = true;
            }
        }
        ushort dustType = (ushort)ModContent.DustType<GrimDruidDust>();
        if (Main.netMode != NetmodeID.Server) {
            if (StateTimer >= 0.55f) {
                if (Main.rand.NextChance(StateTimer - 0.55f + 0.25f)) {
                    if (Main.rand.NextBool()) {
                        int dust = Dust.NewDust(new Vector2(NPC.Center.X + 19 * NPC.direction - 4, NPC.Center.Y - 2), 4, 4, dustType, 0f, 0f, 255, Scale: 0.9f);
                        Main.dust[dust].velocity *= 0.1f;
                        Main.dust[dust].customData = 1f;
                    }
                }
            }
            if (AttackType == 0 || PlayersOldPosition == Vector2.Zero) {
                return;
            }
            int positionX = (int)PlayersOldPosition.X / 16;
            int positionY = (int)PlayersOldPosition.Y / 16;
            while (!Framing.GetTileSafely(positionX, positionY - 1).HasTile || !WorldGen.SolidTile2(positionX, positionY - 1)) {
                positionY++;
            }
            float stateTimer = StateTimer - 0.25f;
            if (Main.rand.NextChance(Math.Min(1f, stateTimer))) {
                int dust = Dust.NewDust(new Vector2(positionX * 16f + Main.rand.Next(-32, 32), positionY * 16f - 26 + 8f), 8, 8, dustType, 0f, Main.rand.NextFloat(-2.5f, -0.5f), 255, Scale: 0.9f + Main.rand.NextFloat(0f, 0.4f));
                Main.dust[dust].velocity *= 0.5f;
            }
        }
    }
}