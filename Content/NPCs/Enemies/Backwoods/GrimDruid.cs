using Microsoft.Xna.Framework;

using RoA.Common;
using RoA.Common.BackwoodsSystems;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Dusts;
using RoA.Content.Items.Placeable.Banners;
using RoA.Content.NPCs.Enemies.Bosses.Lothor.Summon;
using RoA.Content.Projectiles.Enemies;
using RoA.Core.Utility;
using RoA.Utilities;

using System;

using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class GrimDruid : DruidNPC {
    protected override Color MagicCastColor => new(234, 15, 35, 0);

    public override void SetStaticDefaults() {
		Main.npcFrameCount[Type] = 19;
    }

	public override void SetDefaults() {
        base.SetDefaults();

        NPC.lifeMax = 280;
        NPC.damage = 30;
        NPC.defense = 5;
        NPC.knockBackResist = 0.35f;

        int width = 28; int height = 48;
        NPC.Size = new Vector2(width, height);

        NPC.aiStyle = -1;

        NPC.npcSlots = 1.1f;
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

    public override void HitEffect(NPC.HitInfo hit) {
        if (NPC.life <= 0 && NPC.downedBoss2 && Main.rand.NextBool(5)) {
            if (NPC.Center.Y / 16 < BackwoodsVars.FirstTileYAtCenter + 25) {
                if (Main.netMode != NetmodeID.MultiplayerClient) {
                    int npc = NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<DruidSoul>());
                    if (Main.netMode == NetmodeID.Server && npc < Main.maxNPCs) {
                        NetMessage.SendData(MessageID.SyncNPC, number: npc);
                    }
                }
            }
        }

        if (Main.netMode == NetmodeID.Server) {
            return;
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

        Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, "VilestDruidGore2".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "VilestDruidGore1".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, "VilestDruidGore3".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "VilestDruidGore4".GetGoreType(), Scale: NPC.scale);
        Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, "VilestDruidGore4".GetGoreType(), Scale: NPC.scale);
    }

    protected override float TimeToChangeState() => 2f;
    protected override float TimeToRecoveryAfterGettingHit() => 1f;
    protected override (Func<bool>, float) ShouldBeAttacking() => (() => true, 450f);

    protected override void Walking() {
        NPC.ApplyFighterAI(true, (npc) => {
            float num87 = 1f * 0.8f;
            float num88 = 0.07f * 0.8f;
            //num87 += (1f - (float)life / (float)lifeMax) * 1.5f;
            //num88 += (1f - (float)life / (float)lifeMax) * 0.15f;
            if (npc.velocity.X < 0f - num87 || npc.velocity.X > num87) {
                if (npc.velocity.Y == 0f)
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
        });
    }

    protected override void Attacking() {
        if (Math.Abs(NPC.velocity.Y) <= NPC.gravity) {
            NPC.ResetAIStyle();
            Player player = Main.player[NPC.target];
            Vector2 position = new(player.Center.X, player.Center.Y + 32);
            NPC.direction = player.Center.DirectionFrom(NPC.Center).X.GetDirection();
            NPC.velocity.X *= 0.8f;
            if (AttackEndTimer >= 0f) {
                StateTimer += TimeSystem.LogicDeltaTime;
                if (StateTimer >= 0.17f) {
                    Attack = true;
                    NPC.netUpdate = true;
                }
                if (StateTimer >= 0.25f) {
                    AttackEffects(position);
                }
                if (StateTimer >= 1.75f) {
                    GrimDruidAttack(position);
                    AttackType = Main.rand.Next(0, 2);
                    AttackTimer = -TimeToChangeState();
                    AttackEndTimer = -0.3f;
                }
                NPC.netUpdate = true;
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
        if (Main.netMode != NetmodeID.Server) {
            SoundEngine.PlaySound(SoundID.Item8, new Vector2(NPC.position.X, NPC.position.Y));
            for (int i = 0; i < 15; i++) {
                int dust = Dust.NewDust(NPC.position + NPC.velocity + new Vector2(-2f, 8f), NPC.width + 4, NPC.height - 16, dustType, 0f, -2f, 255, new Color(255, 0, 25), Main.rand.NextFloat(0.8f, 1.2f));
                Main.dust[dust].velocity.Y *= 0.4f;
                Main.dust[dust].velocity.X *= 0.1f;
            }
        }
        if (Main.netMode == NetmodeID.MultiplayerClient) {
            return;
        }
        if (AttackType == 0) {
            if (Main.netMode != NetmodeID.Server) {
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            }
            Vector2 directionNormalized = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center);
            Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.Center.X + 18 * NPC.direction, NPC.Center.Y), new Vector2(directionNormalized.X * 5, directionNormalized.Y * 5), ModContent.ProjectileType<GrimBranch>(), NPC.damage / 2, 0.3f, Main.myPlayer, 100, 0f);
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
        if (Main.netMode != NetmodeID.Server) {
            SoundEngine.PlaySound(SoundID.Item76, position);
        }
        Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(positionX * 16f + 8, positionY * 16f - 30), Vector2.Zero, ModContent.ProjectileType<VileSpike>(), NPC.damage / 2, 0.2f, Main.myPlayer);
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
                NPC.netUpdate = true;
            }
        }
        ushort dustType = (ushort)ModContent.DustType<GrimDruidDust>();
        if (Main.netMode != NetmodeID.Server) {
            if (StateTimer >= 0.55f) {
                if (Main.rand.NextChance(StateTimer - 0.55f + 0.25f)) {
                    if (Main.rand.NextBool()) {
                        int dust = Dust.NewDust(new Vector2(NPC.Center.X + 19 * NPC.direction - 4, NPC.Center.Y - 2), 4, 4, dustType, 0f, 0f, 255, Scale: 0.9f);
                        Main.dust[dust].velocity *= 0.1f;
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