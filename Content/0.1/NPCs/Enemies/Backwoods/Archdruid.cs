using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Dusts;
using RoA.Content.Items.Equipables.Miscellaneous;
using RoA.Content.Items.Placeable.Banners;
using RoA.Content.Items.Weapons.Nature.PreHardmode;
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

sealed class Archdruid : DruidNPC {
    private bool _comboAttack, _comboAttack2, _entAttack3;
    private byte _attackCounter;
    private bool _entAttack, _entAttack2;

    private float _timer, _timer2;

    protected override Color MagicCastColor => new(50, 193, 97, 0);

    protected override byte MaxFrame => (byte)(_entAttack ? Main.npcFrameCount[Type] : (19 - 1));

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
            Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "ArchdruidGore2".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "ArchdruidGore1".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "ArchdruidGore3".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "ArchdruidGore4".GetGoreType(), Scale: NPC.scale);
            Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "ArchdruidGore4".GetGoreType(), Scale: NPC.scale);
        }
    }

    public override void SetStaticDefaults() {
        Main.npcFrameCount[Type] = 30;

        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot) {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SacrificialSickleOfTheMoon>(), 13));
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeerSkull>(), 7));
    }

    public override void SetDefaults() {
        base.SetDefaults();

        NPC.lifeMax = 500;
        NPC.damage = 40;
        NPC.defense = 16;
        NPC.knockBackResist = 0f;

        int width = 28; int height = 48;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.npcSlots = 2f;
        NPC.value = Item.buyPrice(0, 1, 0, 0);

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];

        NPC.rarity = 1;

        Banner = Type;
        BannerItem = ModContent.ItemType<ArchdruidBanner>();
        ItemID.Sets.KillsToBanner[BannerItem] = 25;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement("Mods.RoA.Bestiary.Archdruid")
        ]);
    }

    protected override float TimeToChangeState() => 2f;
    protected override float TimeToRecoveryAfterGettingHit() => 1f;
    protected override (Func<bool>, float) ShouldBeAttacking() => (() => true, 550f);

    protected override bool ShouldAttack() => (Main.player[NPC.target].InModBiome<BackwoodsBiome>() || NPC.life < (int)(NPC.lifeMax * 0.8f)) && ShouldBeAttacking().Item1() && ((NPC.Distance(Main.player[NPC.target].Center) < ShouldBeAttacking().Item2 && Collision.CanHit(NPC.Center, 4, 4, Main.player[NPC.target].Center, 4, 4) && !_entAttack) || _entAttack);

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!_entAttack) {
            base.PostDraw(spriteBatch, screenPos, drawColor);
        }
    }

    protected override void AttackAnimation() {
        if (!_entAttack) {
            base.AttackAnimation();

            return;
        }

        NPC.frameCounter++;
        if (NPC.velocity.Y > 0f || NPC.velocity.Y <= -0.25f) {
        }
        else {
            if (NPC.frameCounter >= 0.0 && NPC.frameCounter < 10) {
                CurrentFrame = 19;
            }
            else if (NPC.frameCounter % 5 == 0 && (NPC.frameCounter <= 30 || (NPC.frameCounter > 55 && NPC.frameCounter <= 70) || (NPC.frameCounter > 95 && NPC.frameCounter <= 105))) {
                CurrentFrame++;
            }
        }
        if (++CastTimer > 4) {
            CastFrame++;
            CastTimer = 0;
        }
        if (CastFrame >= 4) {
            CastFrame = 0;
        }
    }

    protected override void Walking() {
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

        float num87 = 1f * 0.9f;
        float num88 = 0.07f * 0.9f;
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

                    //if (num85 <= num86) {
                    //    NPC.gfxOffY += NPC.position.Y + NPC.height - num84;
                    //    NPC.position.Y = num84 - NPC.height;
                    //    if (num85 < 9f) {
                    //        NPC.stepSpeed = 1f;
                    //    }
                    //    else {
                    //        NPC.stepSpeed = 2f;
                    //    }
                    //}
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
        Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
    }

    protected override void ChangeToAttackState() {
        if (Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool()) {
            _comboAttack = true;

            NPC.netUpdate = true;
        }
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
            if (_entAttack) {
                if (NPC.ai[2] >= 65 && !_entAttack3) {
                    SoundEngine.PlaySound(SoundID.Item29, NPC.position);
                    _entAttack3 = true;
                }
                if (NPC.frameCounter >= 70 && !_entAttack2) {
                    _entAttack2 = true;

                    for (int i = 0; i < 10; i++) {
                        int dust7 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y + NPC.height), NPC.width + 20, 4, DustID.Smoke, 0f, 0f, 80, default(Color), 1.5f);
                        Main.dust[dust7].velocity *= 0.6f;
                    }
                    for (int i = 0; i < 30; i++) {
                        int dust6 = Dust.NewDust(new Vector2(NPC.Center.X - 40, NPC.Center.Y + 25), NPC.width * 3, 8, DustID.Clentaminator_Green, 0f, -2f, 40, new Color(0, 255, 25), Main.rand.NextFloat(0.8f, 1.6f));
                        Main.dust[dust6].velocity.X *= 0.2f;
                        Main.dust[dust6].velocity.Y *= 0.5f;
                    }
                    SoundEngine.PlaySound(SoundID.Item14, NPC.position);
                    if (NPC.CountNPCS(ModContent.NPCType<Ent>()) < 2) {
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            int npcSlot = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<EntLegs>());
                            NPC npc = Main.npc[npcSlot];
                            npc.ai[3] = Main.rand.Next(1, 4);
                            npc.lifeMax = 500 - (int)(50 * npc.ai[3]);
                            npc.life = npc.lifeMax;
                            npc.SpawnedFromStatue = true;
                            npc.value = 0f;
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcSlot);
                        }
                    }
                    else {
                        int healAmount = (int)(NPC.lifeMax * 0.25f);
                        NPC.HealEffect(healAmount);
                        NPC.life += healAmount;
                        if (NPC.life > NPC.lifeMax) {
                            NPC.life = NPC.lifeMax;
                        }
                        SoundEngine.PlaySound(SoundID.Item2, NPC.position);
                    }
                }
            }
            if (AttackEndTimer >= 0f) {
                bool flag2 = StateTimer < 0.55f;
                StateTimer += _entAttack && flag2 ? TimeSystem.LogicDeltaTime * 1.25f : flag2 ? TimeSystem.LogicDeltaTime : (TimeSystem.LogicDeltaTime * 0.75f);
                if (StateTimer >= 0.17f) {
                    Attack = true;
                    //NPC.netUpdate = true;
                }
                if (StateTimer >= 0.25f) {
                    AttackEffects(position);
                }
                if (StateTimer >= 1.75f) {
                    GrimDruidAttack(position);
                    if (_comboAttack && !_entAttack) {
                        StateTimer = 0.55f;
                        bool flag = AttackType == 0;
                        AttackType = flag ? 1 : 0;
                        _comboAttack = false;
                        _comboAttack2 = true;
                    }
                    else {
                        _comboAttack2 = false;
                        //StateTimer = 0f;
                        //Attack = false;
                        if (Main.netMode != NetmodeID.MultiplayerClient) {
                            AttackType = Main.rand.Next(0, 2);
                            NPC.netUpdate = true;
                        }
                        AttackTimer = -TimeToChangeState();
                        AttackEndTimer = _entAttack ? -0.05f : -0.3f;
                        //ChangeState((int)States.Walking);
                    }
                    //NPC.netUpdate = true;
                }
            }
            else {
                AttackEffects(position);
                StateTimer = 1.75f;
                AttackEndTimer += TimeSystem.LogicDeltaTime;
                if (AttackEndTimer >= -0.05f) {
                    StateTimer = 0f;
                    AttackEndTimer = 0f;
                    Attack = false;
                    _entAttack = _entAttack2 = _entAttack3 = false;

                    ActivateEntAttackIfCan();

                    ChangeState((int)States.Walking);
                }
            }
        }
    }

    private bool ActivateEntAttackIfCan() {
        if (_attackCounter >= 4) {
            _attackCounter = 0;
            _entAttack = true;
            _comboAttack = false;

            NPC.frameCounter = 0;

            //NPC.netUpdate = true;

            return true;
        }

        return false;
    }

    private void GrimDruidAttack(Vector2 position) {
        _attackCounter++;

        if (_entAttack) {
            return;
        }

        ushort dustType = (ushort)ModContent.DustType<ArchdruidDust>();
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
                Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.Center.X + 18 * NPC.direction, NPC.Center.Y), new Vector2(directionNormalized.X * 5, directionNormalized.Y * 5), ModContent.ProjectileType<ArchBranch>(),
                    60 / 2, 1.5f, Main.myPlayer, 100, 0f);
            }
            return;
        }
        int positionX = (int)PlayersOldPosition.X / 16;
        int positionY = (int)PlayersOldPosition.Y / 16;
        while (!Framing.GetTileSafely(positionX, positionY - 1).HasTile || !WorldGen.SolidTile2(positionX, positionY - 1)) {
            positionY++;
        }
        SoundEngine.PlaySound(SoundID.Item76, position);
        if (Main.netMode != NetmodeID.MultiplayerClient) {
            Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(positionX * 16f + 8, positionY * 16f - 30), Vector2.Zero, ModContent.ProjectileType<ArchVileSpike>(),
                50 / 2, 2.5f, Main.myPlayer);
        }
    }

    private void AttackEffects(Vector2 position) {
        if (AttackEndTimer < 0f) {
            return;
        }

        if (_entAttack) {
            return;
        }

        if (AttackType == 1) {
            PlayersOldPosition = position;
        }
        ushort dustType = (ushort)ModContent.DustType<ArchdruidDust>();
        if (Main.netMode != NetmodeID.Server) {
            if (StateTimer >= 0.55f || _comboAttack2) {
                if (Main.rand.NextChance(!_comboAttack2 ? (StateTimer - 0.55f + 0.25f) : (StateTimer + 0.25f))) {
                    if (Main.rand.NextBool()) {
                        int dust = Dust.NewDust(new Vector2(NPC.Center.X + 21 * NPC.direction - 4, NPC.Center.Y - 4), 4, 4, dustType, 0f, 0f, 255, Scale: 0.9f);
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
            Point point = new(positionX, positionY);
            Point point2 = point;
            while (!WorldGen.SolidTile(point2)) {
                if (TileID.Sets.Platforms[WorldGenHelper.GetTileSafely(point2.X, point2.Y).TileType]) {
                    break;
                }

                point2.Y++;
            }
            point2.Y += 1;
            positionX = point2.X;
            positionY = point2.Y;
            float stateTimer = StateTimer - 0.25f;
            if (Main.rand.NextChance(Math.Min(1f, stateTimer))) {
                int dust = Dust.NewDust(new Vector2(positionX * 16f + Main.rand.Next(-32, 32), positionY * 16f - 26 + 8f), 8, 8, dustType, 0f, Main.rand.NextFloat(-2.5f, -0.5f), 255, Scale: 0.9f + Main.rand.NextFloat(0f, 0.4f));
                Main.dust[dust].velocity *= 0.5f;
            }
        }
    }

    public override void SendExtraAI(BinaryWriter writer) {
        base.SendExtraAI(writer);

        writer.Write(_comboAttack);
        writer.Write(_comboAttack2);
        writer.Write(_entAttack);
        writer.Write(_timer);
        writer.Write(_timer2);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        base.ReceiveExtraAI(reader);

        _comboAttack = reader.ReadBoolean();
        _comboAttack2 = reader.ReadBoolean();
        _entAttack = reader.ReadBoolean();
        _timer = reader.ReadSingle();
        _timer2 = reader.ReadSingle();
    }
}