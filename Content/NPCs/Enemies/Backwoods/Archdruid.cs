using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Dusts;
using RoA.Content.Projectiles.Enemies;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods;

sealed class Archdruid : DruidNPC {
    private bool _comboAttack, _comboAttack2, _entAttack3;
    private byte _attackCounter;
    private bool _entAttack, _entAttack2;

    protected override Color MagicCastColor => new(50, 193, 97, 0);

    protected override byte MaxFrame => (byte)(_entAttack ? Main.npcFrameCount[Type] : (19 - 1));

    public override void SetStaticDefaults() {
		Main.npcFrameCount[Type] = 30;
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

        NPC.npcSlots = 3f;
        NPC.value = Item.buyPrice(0, 4, 10, 0);
    }

    protected override float TimeToChangeState() => 2f;
    protected override float TimeToRecoveryAfterGettingHit() => 1f;
    protected override (Func<bool>, float) ShouldBeAttacking() => (() => true, 550f);

    protected override bool ShouldAttack() => ShouldBeAttacking().Item1() && ((NPC.Distance(Main.player[NPC.target].Center) < ShouldBeAttacking().Item2 && Collision.CanHit(NPC.Center, 4, 4, Main.player[NPC.target].Center, 4, 4) && !_entAttack) || _entAttack);

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
        NPC.PseudoGolemAI(0.4f);
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
            NPC.velocity.X *= 0.8f;
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
                    NPC.netUpdate = true;
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
                        }
                        AttackTimer = -TimeToChangeState();
                        AttackEndTimer = _entAttack ? -0.05f : -0.3f;
                        //ChangeState((int)States.Walking);
                    }
                    NPC.netUpdate = true;
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

            NPC.netUpdate = true;

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
            Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.Center.X + 18 * NPC.direction, NPC.Center.Y), new Vector2(directionNormalized.X * 5, directionNormalized.Y * 5), ModContent.ProjectileType<ArchBranch>(), NPC.damage / 2, 0.3f, Main.myPlayer, 100, 0f);
            return;
        }
        int positionX = (int)PlayersOldPosition.X / 16;
        int positionY = (int)PlayersOldPosition.Y / 16;
        while (!Framing.GetTileSafely(positionX, positionY - 1).HasTile || !WorldGen.SolidTile2(positionX, positionY - 1)) {
            positionY++;
        }
        if (Main.netMode != NetmodeID.Server) {
            SoundEngine.PlaySound(SoundID.Item76, position);
        }
        Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(positionX * 16f + 8, positionY * 16f - 30), Vector2.Zero, ModContent.ProjectileType<ArchVileSpike>(), NPC.damage / 2, 0.2f, Main.myPlayer);
    }

    private void AttackEffects(Vector2 position) {
        if (AttackEndTimer < 0f) {
            return;
        }

        if (_entAttack) {
            return;
        }

        if (AttackType == 1) {
            NPC.localAI[3] += TimeSystem.LogicDeltaTime;
            if (NPC.localAI[3] > 0.1f) {
                PlayersOldPosition = position;
                NPC.localAI[3] = 0f;
                NPC.netUpdate = true;
            }
        }
        ushort dustType = (ushort)ModContent.DustType<ArchdruidDust>();
        if (Main.netMode != NetmodeID.Server) {
            if (StateTimer >= 0.55f || _comboAttack2) {
                if (Main.rand.NextChance(!_comboAttack2 ? (StateTimer - 0.55f + 0.25f) : (StateTimer + 0.25f))) {
                    if (Main.rand.NextBool()) {
                        int dust = Dust.NewDust(new Vector2(NPC.Center.X + 21 * NPC.direction - 4, NPC.Center.Y - 4), 4, 4, dustType, 0f, 0f, 255, Scale: 0.9f);
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

    public override void SendExtraAI(BinaryWriter writer) {
        base.SendExtraAI(writer);

        writer.Write(_comboAttack);
        writer.Write(_comboAttack2);
        writer.Write(_entAttack);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        base.ReceiveExtraAI(reader);

        _comboAttack = reader.ReadBoolean();
        _comboAttack2 = reader.ReadBoolean();
        _entAttack = reader.ReadBoolean();
    }
}