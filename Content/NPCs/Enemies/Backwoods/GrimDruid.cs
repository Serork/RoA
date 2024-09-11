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

sealed class GrimDruid : DruidNPC {
    private Vector2 _playersOldPosition;
    private float _reset;

    public int AttackType { get; private set; }
    
    public override void SetStaticDefaults() {
		Main.npcFrameCount[Type] = 19;
	}

	public override void SetDefaults() {
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
    }

    public override void FindFrame(int frameHeight) {
        NPC.spriteDirection = NPC.direction;
        double walkingCounter = 4.0;
        switch (State) {
            case (float)States.Walking:
                bool dead = Main.player[NPC.target].dead;
                if (NPC.velocity.Y > 0f || NPC.velocity.Y <= -0.25f) {
                }
                else if (NPC.velocity.X == 0f) {
                    CurrentFrame = 0;
                }
                else if (++NPC.frameCounter > walkingCounter) {
                    int firstWalkingFrame = 2;
                    int lastWalkingFrame = 16;
                    CurrentFrame++;
                    if (CurrentFrame <= 1) {
                        CurrentFrame = firstWalkingFrame;
                    }
                    else if (CurrentFrame >= lastWalkingFrame) {
                        CurrentFrame = firstWalkingFrame;
                    }
                    NPC.frameCounter = 0.0;
                }
                break;
            case (float)States.Attacking:
                if (Attack) {
                    double maxTime = walkingCounter * 10.0;
                    NPC.frameCounter += 1;
                    float factor = Helper.EaseInOut2((float)(NPC.frameCounter / maxTime)) * 3f;
                    bool flag = NPC.frameCounter <= maxTime;
                    if (NPC.velocity.Y > 0f || NPC.velocity.Y <= -0.25f) {
                    }
                    else if (flag) {
                        CurrentFrame = Main.npcFrameCount[Type] - 3 + (int)factor;
                    }
                    if (++CastTimer > 4) {
                        CastFrame++;
                        CastTimer = 0;
                    }
                    if (CastFrame >= 4) {
                        CastFrame = 0;
                    }
                }
                else {
                    CurrentFrame = 0;
                }
                break;
        }
        if (NPC.velocity.Y > 0f || NPC.velocity.Y <= -0.25f) {
            CurrentFrame = 1;
        }
        int currentFrame = Math.Min((int)CurrentFrame, Main.npcFrameCount[Type] - 1);
        ChangeFrame((currentFrame, frameHeight));
    }

    public override void SendExtraAI(BinaryWriter writer) {
        base.SendExtraAI(writer);

        writer.Write(AttackType);
        writer.WriteVector2(_playersOldPosition);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        base.ReceiveExtraAI(reader);

        AttackType = reader.ReadInt32();
        _playersOldPosition = reader.ReadVector2(); 
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (CurrentFrame >= Main.npcFrameCount[Type] - 2) {
            DrawMagicCast(spriteBatch, new Color(234, 15, 35, 0), (int)CastFrame, Helper.EaseInOut2((float)(NPC.frameCounter / 80.0)));
        }
    }

    protected override float TimeToChangeState() => 2f;
    protected override float TimeToRecoveryAfterGettingHit() => 1f;

    protected override (Func<bool>, float) ShouldBeAttacking() => (() => true, 450f);

    protected override void Walking() {
        NPC.direction = Main.player[NPC.target].Center.DirectionFrom(NPC.Center).X.GetDirection();
        NPC.aiStyle = 3;
        AIType = 580;
        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
        if (Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) <= 20f && Math.Abs(NPC.velocity.Y) <= NPC.gravity) {
            NPC.velocity.X *= 0.99f;
            NPC.velocity.X += (float)NPC.direction * 0.025f;
        }
        float maxSpeed = 1.1f;
        NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);
    }

    protected override void Attacking() {
        if (Math.Abs(NPC.velocity.Y) <= NPC.gravity) {
            NPC.aiStyle = 0;
            AIType = -1;
            Player player = Main.player[NPC.target];
            NPC.direction = player.Center.DirectionFrom(NPC.Center).X.GetDirection();
            NPC.velocity.X *= 0.8f;
            StateTimer += TimeSystem.LogicDeltaTime;
            Vector2 position = new(player.Center.X, player.Center.Y + 32);
            if (StateTimer >= 0.17f) {
                Attack = true;
                NPC.netUpdate = true;
            }
            if (StateTimer >= 0.25f) {
                Dusts(position);
            }
            if (StateTimer >= 1.75f) {
                Attack = false;
                GrimDruidAttack(position);
                AttackType = Main.rand.Next(0, 2);
                StateTimer = 0f;
                AttackTimer = -TimeToChangeState();
                ChangeState((int)States.Walking);
                NPC.netUpdate = true;
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
            int projectile = Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(NPC.Center.X + 18 * NPC.direction, NPC.Center.Y), new Vector2(directionNormalized.X * 5, directionNormalized.Y * 5), ModContent.ProjectileType<GrimBranch>(), NPC.damage / 2, 0.3f, Main.myPlayer, 100, 0f);
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, projectile);
            return;
        }
        int positionX = (int)_playersOldPosition.X / 16;
        int positionY = (int)_playersOldPosition.Y / 16;
        while (!Framing.GetTileSafely(positionX, positionY - 1).HasTile || !WorldGen.SolidTile2(positionX, positionY - 1)) {
            positionY++;
        }
        if (Main.netMode != NetmodeID.Server) {
            SoundEngine.PlaySound(SoundID.Item76, position);
        }
        Projectile.NewProjectile(NPC.GetSource_FromAI(), new Vector2(positionX * 16f + 8, positionY * 16f - 30), Vector2.Zero, ModContent.ProjectileType<VileSpike>(), NPC.damage, 0.2f, Main.myPlayer);
    }

    private void Dusts(Vector2 position) {
        if (AttackType == 1) {
            NPC.localAI[3] += TimeSystem.LogicDeltaTime;
            if (NPC.localAI[3] > 0.1f) {
                _playersOldPosition = position;
                NPC.localAI[3] = 0f;
                NPC.netUpdate = true;
            }
        }
        if (Main.netMode == NetmodeID.Server) {
            return;
        }
        ushort dustType = (ushort)ModContent.DustType<GrimDruidDust>();
        if (StateTimer >= 0.65f) {
            if (Main.rand.NextChance(StateTimer - 0.75f + 0.25f)) {
                if (Main.rand.NextBool()) {
                    int dust = Dust.NewDust(new Vector2(NPC.Center.X + 19 * NPC.direction - 2, NPC.Center.Y - 2), 4, 4, dustType, 0f, 0f, 255, Scale: 0.9f);
                    Main.dust[dust].velocity *= 0.1f;
                }
            }
        }
        if (AttackType == 0) {
            return;
        }
        int positionX = (int)_playersOldPosition.X / 16;
        int positionY = (int)_playersOldPosition.Y / 16;
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