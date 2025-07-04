using Microsoft.Xna.Framework;

using RoA.Common.Projectiles;
using RoA.Content.Biomes.Backwoods;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
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
            GoingToTree,
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

    public Vector2 TreePosition;
    public int DirectionToTree;
    public float VelocityXFactor = 1f;

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
        void directToTree() {
            handleXMovement();
            float stoppingDistance = 10f;
            float distance = MathF.Abs(NPC.Center.X - TreePosition.X);
            if (distance < stoppingDistance * 2f) {
                float velocityXFactor = distance / (stoppingDistance * 2f);
                VelocityXFactor = Helper.Approach(VelocityXFactor, velocityXFactor, 0.025f);
                NPC.velocity.X *= VelocityXFactor;
                if (MathF.Abs(NPC.velocity.X) < 0.1f) {
                    NPC.velocity.X = 0f;
                }
            }

            NPC.DirectTo(TreePosition, updateSpriteDirection: false);
            NPC.spriteDirection = DirectionToTree;
        }
        void handleGoingToTreeState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (woodpeckerValues.State != WoodpeckerValues.AIState.GoingToTree) {
                return;
            }

            directToTree();

            if (Helper.SinglePlayerOrServer && woodpeckerValues.ShouldBePeckingTimer++ >= PECKINGCHECKTIME) {
                woodpeckerValues.ShouldBePeckingTimer = 0f;
                woodpeckerValues.State = WoodpeckerValues.AIState.Pecking;

                NPC.netUpdate = true;
            }
        }
        void handleIdleState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (woodpeckerValues.State != WoodpeckerValues.AIState.Idle) {
                return;
            }

            if (HaveFreeTreeNearby(out Vector2 treePosition, out int directionToTree) && !shouldTargetPlayer) {
                TreePosition = treePosition;
                DirectionToTree = directionToTree;
                woodpeckerValues.State = WoodpeckerValues.AIState.GoingToTree;
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

            NPCExtensions.FighterAI.ApplyFighterAI(NPC, ref woodpeckerValues.CanBeBusyWithActionTimer,
                                                        ref woodpeckerValues.EncouragementTimer, 
                                                        ref woodpeckerValues.TargetClosestTimer,
                                                        shouldTargetPlayer: shouldTargetPlayer,
                                                        xMovement: handleXMovement);
            if (HaveFreeTreeNearby(out _, out _)) {
                woodpeckerValues.CanBeBusyWithActionTimer = woodpeckerValues.EncouragementTimer = woodpeckerValues.TargetClosestTimer = 0f;
                woodpeckerValues.State = WoodpeckerValues.AIState.Idle;
            }
        }
        void handlePeckingState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (woodpeckerValues.State != WoodpeckerValues.AIState.Pecking) {
                return;
            }

            directToTree();
            if (woodpeckerValues.StartedPecking && woodpeckerValues.Frame == WoodpeckerValues.AnimationFrame.Idle) {
                woodpeckerValues.State = WoodpeckerValues.AIState.GoingToTree;
            }
        }
        void handleAirborneState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (!NPC.IsGrounded()) {
                woodpeckerValues.State = WoodpeckerValues.AIState.Jump;
            }
            else if (ShouldGoToIdle()) {
                woodpeckerValues.State = WoodpeckerValues.AIState.Idle;
            }
        }

        handleGoingToTreeState();
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
                case WoodpeckerValues.AIState.GoingToTree:
                    bool goingToTree = woodpeckerValues.State == WoodpeckerValues.AIState.GoingToTree;
                    if ((goingToTree && MathF.Abs(NPC.velocity.X) > 0f) || !goingToTree) {
                        byte walkingAnimationSpeed = 15;
                        double additionalCounter = Math.Abs(NPC.velocity.Length());
                        woodpeckerValues.Frame = (WoodpeckerValues.AnimationFrame)NPC.AnimateFrame((byte)woodpeckerValues.Frame, (byte)WoodpeckerValues.AnimationFrame.Walking1, (byte)WoodpeckerValues.AnimationFrame.Walking6, walkingAnimationSpeed, (ushort)frameHeight, additionalCounter);
                    }
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
            if (ShouldUpdateDirection()) {
                NPC.UpdateDirectionBasedOnVelocity();
            }
        }

        setDirection();
        animatePerState();
    }

    private bool ShouldGoToIdle() {
        WoodpeckerValues woodpeckerValues = new(NPC);
        return woodpeckerValues.State != WoodpeckerValues.AIState.Walking &&
               woodpeckerValues.State != WoodpeckerValues.AIState.Pecking &&
               woodpeckerValues.State != WoodpeckerValues.AIState.GoingToTree;
    }

    private bool ShouldUpdateDirection() {
        WoodpeckerValues woodpeckerValues = new(NPC);
        return woodpeckerValues.State != WoodpeckerValues.AIState.GoingToTree &&
               woodpeckerValues.State != WoodpeckerValues.AIState.Pecking;
    }

    private bool HaveFreeTreeNearby(out Vector2 treePosition, out int directionToTree) {
        bool result = false;
        bool haveTreeNearby = false;
        int checkXDistance = 5;
        treePosition = NPC.Center;
        directionToTree = NPC.direction;
        bool anyTree(int checkX, out Vector2 treePosition, out int directionToTree) {
            bool result = false;
            treePosition = NPC.Center;
            directionToTree = NPC.direction;
            int x = (int)NPC.Center.X / 16 + checkX, y = (int)NPC.Center.Y / 16;
            if (WorldGenHelper.GetTileSafely(x, y).ActiveTile(TileID.Trees) &&
                WorldGenHelper.GetTileSafely(x, y + 1).ActiveTile(TileID.Trees)) {
                result = true;
                treePosition = new Point16(x, y).ToWorldCoordinates();
                directionToTree = (treePosition.X - NPC.Center.X).GetDirection();
                treePosition += -Vector2.UnitX * NPC.width * directionToTree * 1.35f;
            }
            return result;
        }
        if (NPC.IsFacingRight()) {
            for (int checkX = checkXDistance; checkX >= -checkXDistance; checkX--) {
                if (anyTree(checkX, out treePosition, out directionToTree)) {
                    haveTreeNearby = true;
                    break;
                }
            }
        }
        else {
            for (int checkX = -checkXDistance; checkX <= -checkXDistance; checkX++) {
                if (anyTree(checkX, out treePosition, out directionToTree)) {
                    haveTreeNearby = true;
                    break;
                }
            }
        }
        bool noOtherWoodpeckerNearby = true;
        foreach (NPC woodpeckerCheckNPC in TrackedEntitiesSystem.GetTrackedNPC<Woodpecker>()) {
            if (woodpeckerCheckNPC.SameAs(NPC)) {
                continue;
            }

            if (new WoodpeckerValues(woodpeckerCheckNPC).IsPecking && 
                MathF.Abs(woodpeckerCheckNPC.Center.X - NPC.Center.X) < TileHelper.TileSize * 4) {
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
