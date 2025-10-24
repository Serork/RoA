using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;

using System;
using System.IO;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace RoA.Content.NPCs.Enemies.Backwoods;

abstract class DruidNPC : RoANPC {
    protected Vector2 PlayersOldPosition;

    protected float AttackTimer { get; set; }
    protected int AttackType { get; set; }
    protected float AttackEndTimer { get; set; }

    protected ref float CastTimer => ref NPC.localAI[1];
    protected ref float CastFrame => ref NPC.localAI[2];

    protected virtual Color MagicCastColor => Color.White;

    protected virtual byte MaxFrame => 19 - 1;

    private float _magicCastOpacity;

    public enum States {
        Walking,
        Attacking
    }

    public override void SetDefaults() {
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath30;

        SpawnModBiomes = [ModContent.GetInstance<BackwoodsBiome>().Type];
    }

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(AttackTimer);
        writer.Write(AttackType);
        writer.WriteVector2(PlayersOldPosition);
        writer.Write(AttackEndTimer);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        AttackTimer = reader.ReadSingle();
        AttackType = reader.ReadInt32();
        PlayersOldPosition = reader.ReadVector2();
        AttackEndTimer = reader.ReadSingle();
    }

    public override bool? CanFallThroughPlatforms() {
        if (Main.player[NPC.target].dead) {
            return true;
        }
        else {
            return Main.player[NPC.target].position.Y > NPC.position.Y + NPC.height;
        }
    }

    protected abstract float TimeToChangeState();
    protected abstract float TimeToRecoveryAfterGettingHit();
    protected abstract void Walking();
    protected abstract void Attacking();
    protected abstract (Func<bool>, float) ShouldBeAttacking();

    protected virtual void ChangeToAttackState() { }

    protected virtual void AttackAnimation() {
        double walkingCounter = 4.0;
        if (Attack) {
            double maxTime = walkingCounter * 10.0;
            if (NPC.frameCounter < 80) {
                NPC.frameCounter += 1;
            }
            float factor = Helper.EaseInOut2((float)(NPC.frameCounter / maxTime)) * 3f;
            bool flag = NPC.frameCounter <= maxTime;
            //if (NPC.velocity.Y > 0f || NPC.velocity.Y <= -0.25f) {
            //}
            if (flag) {
                CurrentFrame = 19 - 3 + (int)factor;
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
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        //if (CurrentFrame >= 19 - 3)
            {
            _magicCastOpacity = MathHelper.Lerp(_magicCastOpacity, (float)(NPC.frameCounter / 80.0), 0.25f);
            DrawMagicCast(spriteBatch, MagicCastColor, (int)CastFrame, Helper.EaseInOut2(_magicCastOpacity));
        }
    }

    public override void FindFrame(int frameHeight) {
        double walkingCounter = 4.0;
        int currentFrame = Math.Min((int)CurrentFrame, MaxFrame);
        if (NPC.IsABestiaryIconDummy) {
            if (++NPC.frameCounter > walkingCounter) {
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
                ChangeFrame((currentFrame, frameHeight));
            }

            return;
        }

        NPC.spriteDirection = NPC.direction;
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
                AttackAnimation();
                break;
        }
        if (NPC.velocity.Y != 0f) {
            CurrentFrame = 1;
        }
        ChangeFrame((currentFrame, frameHeight));
    }

    protected virtual bool ShouldAttack() => (Main.player[NPC.target].InModBiome<BackwoodsBiome>() || NPC.life < (int)(NPC.lifeMax * 0.8f)) && NPC.LookingAt(Main.player[NPC.target].Center) && ShouldBeAttacking().Item1() && NPC.Distance(Main.player[NPC.target].Center) < ShouldBeAttacking().Item2 && Collision.CanHit(NPC, Main.player[NPC.target]);

    public sealed override void AI() {
        if (NPC.localAI[3] == 0f) {
            NPC.localAI[3] = 1f;
            //NPC.TargetClosest();
            AttackTimer = -TimeToChangeState();
            NPC.netUpdate = true;
        }

        Player player;
        switch (State) {
            case (float)States.Walking:
                //NPC.TargetClosest();
                Walking();
                if (NPC.HasPlayerTarget) {
                    player = Main.player[NPC.target];
                    //if (AttackTimer < -TimeToChangeState() + 0.2f && NPC.velocity.Y < 0f) {
                    //    NPC.velocity.Y = 0f;
                    //}
                    if (ShouldAttack()) {
                        AttackTimer += TimeSystem.LogicDeltaTime;
                    }
                    if (NPC.justHit && Main.expertMode) {
                        AttackTimer += TimeToRecoveryAfterGettingHit() * 0.2f;
                    }
                    //if (NPC.justHit && AttackTimer > -TimeToRecoveryAfterGettingHit()) {
                    //    AttackTimer -= -TimeToRecoveryAfterGettingHit() * 0.25f;
                    //}
                    if (NPC.velocity.Y == 0f) {
                        if (!player.dead && AttackTimer >= 0f) {
                            AttackTimer = 0f;
                            if (Main.netMode != NetmodeID.MultiplayerClient) {
                                AttackType = Main.rand.Next(0, 2);
                                NPC.netUpdate = true;
                            }
                            ChangeToAttackState();
                            ChangeState((int)States.Attacking);
                            return;
                        }
                    }
                }
                break;
            case (float)States.Attacking:
                //if (NPC.velocity.Y < 0f) {
                //    ChangeState((int)States.Walking);
                //    return;
                //}
                player = Main.player[NPC.target];
                //bool inRange = NPC.Distance(player.Center) >= ShouldBeAttacking().Item2;
                //if (NPC.justHit && !Attack) {
                //    StateTimer = -TimeToRecoveryAfterGettingHit();
                //}
                //if (NPC.velocity.Y > 0f) {
                //    ChangeState((int)States.Walking);
                //    return;
                //}
                NPC.knockBackResist = 0f;
                if ((player.dead || AttackTimer < 0f) && !Attack) {
                    AttackTimer = -TimeToChangeState();
                    ChangeState((int)States.Walking);
                    return;
                }
                Attacking();
                break;
        }
    }

    protected void DrawMagicCast(SpriteBatch spriteBatch, Color color, int frame, float alpha = 1f) {
        if (frame < 0) {
            frame = (int)TimeSystem.GlobalTime % 48 / 12;
        }
        Texture2D textureCasting = TextureAssets.Extra[51].Value;
        Vector2 origin = NPC.Bottom + new Vector2(0f, NPC.gfxOffY + 6f);
        Rectangle rectangle = textureCasting.Frame(1, 4, 0, Math.Max(0, Math.Min(3, frame)));
        Vector2 origin2 = rectangle.Size() * new Vector2(0.5f, 1f);
        spriteBatch.Draw(textureCasting, new Vector2((int)(origin.X - Main.screenPosition.X), (int)(origin.Y - Main.screenPosition.Y)), new Rectangle?(rectangle), color * alpha * 1.25f, 0f, origin2, 1f, SpriteEffects.FlipHorizontally, 0f);
    }
}