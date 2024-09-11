using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;

using System;
using System.IO;

using Terraria;
using Terraria.GameContent;
using Terraria.ID;
namespace RoA.Content.NPCs.Enemies.Backwoods;

abstract class DruidNPC : RoANPC {
    protected float AttackTimer { get; set; }

    protected ref float CastTimer => ref NPC.localAI[1];
    protected ref float CastFrame => ref NPC.localAI[2];

    public enum States {
        Walking,
        Attacking
    }

    public override void SetDefaults() {
        base.SetDefaults();

        NPC.HitSound = SoundID.NPCHit19;
        NPC.DeathSound = SoundID.NPCDeath30;
    }

    protected abstract float TimeToChangeState();
    protected abstract float TimeToRecoveryAfterGettingHit();
    protected abstract void Walking();
    protected abstract void Attacking();
    protected abstract (Func<bool>, float) ShouldBeAttacking();

    public override void SendExtraAI(BinaryWriter writer) {
        writer.Write(AttackTimer);
    }

    public override void ReceiveExtraAI(BinaryReader reader) {
        AttackTimer = reader.ReadSingle();
    }

    public sealed override void AI() {
        Player player;
        switch (State) {
            case (float)States.Walking:
                NPC.TargetClosest();
                player = Main.player[NPC.target];
                if (AttackTimer < -TimeToChangeState() + 0.2f && NPC.velocity.Y < 0f) {
                    NPC.velocity.Y = 0f;
                }
                AttackTimer += TimeSystem.LogicDeltaTime;
                bool canHit = Collision.CanHit(NPC.Center, 4, 4, player.Center, 4, 4);
                if (NPC.justHit) {
                    AttackTimer = -TimeToRecoveryAfterGettingHit();
                }
                if (ShouldBeAttacking().Item1() && NPC.Distance(player.Center) < ShouldBeAttacking().Item2 && !player.dead && AttackTimer >= 0f && canHit) {
                    if (NPC.velocity.Y == 0f) {
                        AttackTimer = 0f;

                        ChangeState((int)States.Attacking);
                    }
                    return;
                }
                Walking();
                break;
            case (float)States.Attacking:
                //if (NPC.velocity.Y < 0f) {
                //    ChangeState((int)States.Walking);
                //    return;
                //}
                player = Main.player[NPC.target];
                bool inRange = NPC.Distance(player.Center) >= ShouldBeAttacking().Item2;
                //if (NPC.justHit && !Attack) {
                //    StateTimer = -TimeToRecoveryAfterGettingHit();
                //}
                if ((inRange || player.dead || AttackTimer < 0f) && !Attack) {
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