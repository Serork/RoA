using RoA.Common.Projectiles;
using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

[NPCTracked]
sealed class Woodpecker : ModNPC {
    private static byte FRAMECOUNT => 15;
    private static float PECKINGCHECKTIME => 20f;

    private ref struct WoodpeckerValues(NPC npc) {
        public enum AIState : byte {
            Idle,
            Jump,
            Pecking,
            Walking,
            TongueAttack,
            Count
        }

        public enum AnimationFrame : byte {
            Idle,
            Pecking1,
            Pecking2,
            Pecking3,
            Pecking4,
            Walking1,
            Walking2,
            Walking3,
            Walking4,
            Walking5,
            Walking6,
            Walking7,
            Count
        }

        public ref float CanBeBusyWithActionTimer = ref npc.localAI[0];
        public ref float EncouragementTimer = ref npc.localAI[1];
        public ref float FrameValue = ref npc.localAI[2];

        public ref float StateValue = ref npc.ai[0];
        public ref float ShouldBePeckingTimer = ref npc.ai[3];
        public ref float StartedPeckingValue = ref npc.ai[3];
        public ref float TargetClosestTimer = ref npc.ai[3];

        public AIState State {
            readonly get => (AIState)StateValue;
            set => StateValue = Utils.Clamp<byte>((byte)value, (byte)AIState.Idle, (byte)AIState.Count);
        }

        public AnimationFrame Frame {
            readonly get => (AnimationFrame)FrameValue;
            set => FrameValue = Utils.Clamp<byte>((byte)value, (byte)AnimationFrame.Idle, (byte)AnimationFrame.Count);
        }

        public bool StartedPecking {
            readonly get => StartedPeckingValue == 1f;
            set => StartedPeckingValue = value.ToInt();
        }

        public readonly bool IsPecking => State == AIState.Pecking || ShouldBePeckingTimer > 0f;
    }

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(28, 40);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;
    }

    public override void AI() {
        Player closestPlayer = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];
        bool shouldTargetPlayer = !closestPlayer.dead && (NPC.life < (int)(NPC.lifeMax * 0.8f) || closestPlayer.InModBiome<BackwoodsBiome>());

        void handleIdleState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (woodpeckerValues.State != WoodpeckerValues.AIState.Idle) {
                return;
            }

            if (HaveFreeTreeNearby() && !shouldTargetPlayer) {
                NPC.velocity.X *= 0.8f;

                if (Helper.SinglePlayerOrServer && woodpeckerValues.ShouldBePeckingTimer++ >= PECKINGCHECKTIME) {
                    woodpeckerValues.ShouldBePeckingTimer = 0f;
                    woodpeckerValues.State = WoodpeckerValues.AIState.Pecking;

                    NPC.netUpdate = true;
                }
            }
            else {
                woodpeckerValues.ShouldBePeckingTimer = 0f;
                woodpeckerValues.State = WoodpeckerValues.AIState.Walking;
            }
        }
        void handleFighterState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (woodpeckerValues.State != WoodpeckerValues.AIState.Walking) {
                return;
            }

            void handleXMovement() {
                float maxSpeed = 2f + NPC.GetRemainingHealthPercentage() * 2f,
                      acceleration = 0.07f,
                      deceleration = 0.8f;
                if (NPC.velocity.X < -maxSpeed || NPC.velocity.X > maxSpeed) {
                    if (NPC.IsGrounded()) {
                        NPC.velocity *= deceleration;
                    }
                }
                else if (NPC.velocity.X < maxSpeed && NPC.IsFacingRight()) {
                    if (NPC.IsGrounded() && NPC.velocity.X < 0f) {
                        NPC.velocity.X *= 0.9f;
                    }

                    NPC.velocity.X += acceleration;
                    if (NPC.velocity.X > maxSpeed) {
                        NPC.velocity.X = maxSpeed;
                    }
                }
                else if (NPC.velocity.X > -maxSpeed && NPC.IsFacingLeft()) {
                    if (NPC.IsGrounded() && NPC.velocity.X > 0f) {
                        NPC.velocity.X *= 0.9f;
                    }

                    NPC.velocity.X -= acceleration;
                    if (NPC.velocity.X < -maxSpeed) {
                        NPC.velocity.X = -maxSpeed;
                    }
                }
            }
            NPCExtensions.FighterAI.ApplyFighterAI(NPC, ref woodpeckerValues.CanBeBusyWithActionTimer,
                                                        ref woodpeckerValues.EncouragementTimer, 
                                                        ref woodpeckerValues.TargetClosestTimer,
                                                        shouldTargetPlayer: shouldTargetPlayer,
                                                        xMovement: handleXMovement);
            if (HaveFreeTreeNearby()) {
                woodpeckerValues.CanBeBusyWithActionTimer = woodpeckerValues.EncouragementTimer = woodpeckerValues.TargetClosestTimer = 0f;
                woodpeckerValues.State = WoodpeckerValues.AIState.Idle;
            }
        }
        void handlePeckingState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (woodpeckerValues.State != WoodpeckerValues.AIState.Pecking) {
                return;
            }

            NPC.velocity.X *= 0.8f;
            if (woodpeckerValues.StartedPecking && woodpeckerValues.Frame == WoodpeckerValues.AnimationFrame.Idle) {
                woodpeckerValues.State = WoodpeckerValues.AIState.Idle;
            }
        }
        void handleAirborneState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (!NPC.IsGrounded()) {
                woodpeckerValues.State = WoodpeckerValues.AIState.Jump;
            }
            else if (woodpeckerValues.State != WoodpeckerValues.AIState.Walking && woodpeckerValues.State != WoodpeckerValues.AIState.Pecking) {
                woodpeckerValues.State = WoodpeckerValues.AIState.Idle;
            }
        }

        handleIdleState();
        handleFighterState();
        handlePeckingState();
        handleAirborneState();
    }

    public override void FindFrame(int frameHeight) {
        void animatePerState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            switch (woodpeckerValues.State) {
                case WoodpeckerValues.AIState.Jump:
                    break;
                case WoodpeckerValues.AIState.Idle:
                    woodpeckerValues.Frame = WoodpeckerValues.AnimationFrame.Idle;
                    break;
                case WoodpeckerValues.AIState.Walking:
                    byte walkingAnimationSpeed = 15;
                    double additionalCounter = Math.Abs(NPC.velocity.Length());
                    woodpeckerValues.Frame = (WoodpeckerValues.AnimationFrame)NPC.AnimateFrame((byte)woodpeckerValues.Frame, (byte)WoodpeckerValues.AnimationFrame.Walking1, (byte)WoodpeckerValues.AnimationFrame.Walking6, walkingAnimationSpeed, (ushort)frameHeight, additionalCounter);
                    break;
                case WoodpeckerValues.AIState.Pecking:
                    byte peckingAnimationSpeed = 6;
                    woodpeckerValues.Frame = (WoodpeckerValues.AnimationFrame)NPC.AnimateFrame((byte)woodpeckerValues.Frame, (byte)WoodpeckerValues.AnimationFrame.Idle, (byte)WoodpeckerValues.AnimationFrame.Pecking4, peckingAnimationSpeed, (ushort)frameHeight);
                    if (woodpeckerValues.Frame == WoodpeckerValues.AnimationFrame.Pecking1) {
                        woodpeckerValues.StartedPecking = true;
                    }
                    break;
            }
        }
        void setDirection() {
            NPC.UpdateDirectionBasedOnVelocity();
        }

        setDirection();
        animatePerState();
    }

    private bool HaveFreeTreeNearby() {
        bool result = false;
        bool haveTreeNearby = WorldGenHelper.GetTileSafely((int)(NPC.Center.X + NPC.width * NPC.direction) / 16, (int)NPC.Center.Y / 16).ActiveTile(TileID.Trees);
        bool noOtherWoodpeckerNearby = true;
        foreach (NPC woodpeckerCheckNPC in TrackedEntitiesSystem.GetTrackedNPC<Woodpecker>()) {
            if (woodpeckerCheckNPC.SameAs(NPC)) {
                continue;
            }

            if (new WoodpeckerValues(woodpeckerCheckNPC).IsPecking && 
                MathF.Abs(woodpeckerCheckNPC.Center.X - NPC.Center.X) < TileHelper.TileSize * 2) {
                noOtherWoodpeckerNearby = false;
                break;
            }
        }
        if (haveTreeNearby && noOtherWoodpeckerNearby) {
            result = true;
        }
        return result;
    }
}
