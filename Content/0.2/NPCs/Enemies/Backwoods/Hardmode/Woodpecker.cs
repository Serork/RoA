using Microsoft.Xna.Framework;

using RoA.Common.Projectiles;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Tiles.Ambient.LargeTrees;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

[NPCTracked]
sealed class Woodpecker : ModNPC {
    private static byte FRAMECOUNT => 16;
    private static float PECKINGCHECKTIME => 20f;
    private static float TONGUEATTACKCHECKTIME => 60f;
    private static ushort TRIGGERAREASIZE => 1000;

    private ref struct WoodpeckerValues(NPC npc) {
        public enum AIState : byte {
            Idle,
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
            TongueAttack1,
            TongueAttack2,
            TongueAttack3,
            TongueAttack4,
            Jump,
            Count
        }

        public ref float CanBeBusyWithActionTimer = ref npc.localAI[0];
        public ref float EncouragementTimer = ref npc.localAI[1];
        public ref float FrameValue = ref npc.localAI[2];
        public ref float CanGoToTreeAgainTimer = ref npc.localAI[3];

        public ref float StateValue = ref npc.ai[0];
        public ref float TongueAttackTimer = ref npc.ai[1];
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

        public readonly bool IsPecking => State == AIState.GoingToTree || State == AIState.Pecking || (State == AIState.Idle && ShouldBePeckingTimer > 0f);

        public void ResetAllTimers() => TongueAttackTimer = CanBeBusyWithActionTimer = CanGoToTreeAgainTimer = EncouragementTimer = TargetClosestTimer = 0f;
    }

    public Vector2 GoToTreePosition;
    public Point16 TreePosition;
    public float VelocityXFactor = 1f;
    public HashSet<Vector2> TreePositionsTaken = [];

    public int DirectionToTree => GoToTreePosition == Vector2.Zero ? 0 : (TreePosition.ToWorldCoordinates().X - NPC.Center.X).GetDirection();
    public ushort TreeDustType => WorldGenHelper.GetTileSafely(TreePosition).TileType == ModContent.TileType<BackwoodsBigTree>() ? (ushort)TileLoader.GetTile(ModContent.TileType<BackwoodsBigTree>()).DustType : TileHelper.GetTreeKillDustType(TreePosition);

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(28, 48);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;
    }

    public override void AI() {
        Player closestPlayer = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];
        Rectangle playerRect;
        Rectangle npcRect = new((int)NPC.position.X - TRIGGERAREASIZE / 2, (int)NPC.position.Y - TRIGGERAREASIZE / 2, NPC.width + TRIGGERAREASIZE, NPC.height + TRIGGERAREASIZE);
        bool isTriggeredBy(Player player) {
            playerRect = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
            if (player.npcTypeNoAggro[Type] && NPC.life >= NPC.lifeMax) {
                return false;
            }
            return (npcRect.Intersects(playerRect) || NPC.life < NPC.lifeMax) && !player.dead && player.active;
        }
        bool hasClosePlayer = false;
        if (isTriggeredBy(closestPlayer)) {
            NPC.target = closestPlayer.whoAmI;
            hasClosePlayer = true;
        }
        bool shouldTargetPlayer = hasClosePlayer && (NPC.life < (int)(NPC.lifeMax * 0.8f) || closestPlayer.InModBiome<BackwoodsBiome>());
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
            float distance = NPC.Center.DistanceX(GoToTreePosition);
            if (distance < stoppingDistance * 2f) {
                float velocityXFactor = distance / (stoppingDistance * 2f);
                VelocityXFactor = Helper.Approach(VelocityXFactor, velocityXFactor, 0.025f);
                NPC.velocity.X *= VelocityXFactor;
                if (NPC.SpeedX() < 0.1f) {
                    NPC.velocity.X = 0f;
                }
                NPC.position.X = Helper.Approach(NPC.position.X, GoToTreePosition.X - NPC.width / 2f, 0.5f * (1f - VelocityXFactor));
            }
            NPC.DirectTo(GoToTreePosition, updateSpriteDirection: false);
            TreePositionsTaken.Add(GoToTreePosition);
            NPC.spriteDirection = DirectionToTree;
        }
        void resetTreeInfo() {
            GoToTreePosition = Vector2.Zero;
            TreePosition = Point16.Zero;
            VelocityXFactor = 1f;
        }
        void handleGoingToTreeState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (woodpeckerValues.State != WoodpeckerValues.AIState.GoingToTree) {
                return;
            }

            if (IsTooFarFromTree()) {
                woodpeckerValues.CanGoToTreeAgainTimer = 60f;
                woodpeckerValues.State = WoodpeckerValues.AIState.Walking;
            }

            directToTree();

            if (shouldTargetPlayer || !NoOtherWoodpeckerNearby(GoToTreePosition) || IsTreeNotDestroyed()) {
                woodpeckerValues.ShouldBePeckingTimer = 0f;
                woodpeckerValues.State = WoodpeckerValues.AIState.Walking;
                return;
            }

            bool shouldBePecking = woodpeckerValues.ShouldBePeckingTimer++ >= PECKINGCHECKTIME;
            if (IsNearTree() && shouldBePecking) {
                woodpeckerValues.ShouldBePeckingTimer = 0f;
                woodpeckerValues.State = WoodpeckerValues.AIState.Pecking;
            }
        }
        void handleIdleState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (woodpeckerValues.State != WoodpeckerValues.AIState.Idle) {
                return;
            }

            if (!shouldTargetPlayer && HaveFreeTreeNearby(out Vector2 goToTreePosition, out Point16 treePosition)) {
                GoToTreePosition = goToTreePosition;
                TreePosition = treePosition;
                woodpeckerValues.State = WoodpeckerValues.AIState.GoingToTree;
                woodpeckerValues.ShouldBePeckingTimer = PECKINGCHECKTIME;
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

            resetTreeInfo();

            NPCExtensions.FighterAI.ApplyFighterAI(NPC, ref woodpeckerValues.CanBeBusyWithActionTimer,
                                                        ref woodpeckerValues.EncouragementTimer, 
                                                        ref woodpeckerValues.TargetClosestTimer,
                                                        shouldTargetPlayer: shouldTargetPlayer,
                                                        xMovement: handleXMovement);

            bool shouldDoTongueAttack = woodpeckerValues.TongueAttackTimer++ > TONGUEATTACKCHECKTIME;
            if (shouldTargetPlayer && shouldDoTongueAttack) {

            }

            bool canStartPeckingAgain = woodpeckerValues.CanGoToTreeAgainTimer-- <= 0f;
            if (HaveFreeTreeNearby(out _, out _) && !shouldTargetPlayer && canStartPeckingAgain) {
                woodpeckerValues.ResetAllTimers();
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
            if (ShouldGoToIdle()) {
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
                case WoodpeckerValues.AIState.Idle:
                    woodpeckerValues.Frame = NPC.IsGrounded() ? WoodpeckerValues.AnimationFrame.Idle : WoodpeckerValues.AnimationFrame.Jump;
                    break;
                case WoodpeckerValues.AIState.Walking:
                case WoodpeckerValues.AIState.GoingToTree:
                    if (NPC.IsGrounded()) {
                        if (NPC.SpeedX() < 0.5f) {
                            woodpeckerValues.Frame = WoodpeckerValues.AnimationFrame.Idle;
                            break;
                        }
                        byte walkingAnimationSpeed = 15;
                        double additionalCounter = MathF.Max(1f, NPC.velocity.Length());
                        woodpeckerValues.Frame = (WoodpeckerValues.AnimationFrame)NPC.AnimateFrame((byte)woodpeckerValues.Frame, (byte)WoodpeckerValues.AnimationFrame.Walking1, (byte)WoodpeckerValues.AnimationFrame.Walking6, walkingAnimationSpeed, (ushort)frameHeight, additionalCounter);
                    }
                    else {
                        woodpeckerValues.Frame = WoodpeckerValues.AnimationFrame.Jump;
                    }
                    break;
                case WoodpeckerValues.AIState.Pecking:
                    byte peckingAnimationSpeed = 6;
                    woodpeckerValues.Frame = (WoodpeckerValues.AnimationFrame)NPC.AnimateFrame((byte)woodpeckerValues.Frame, (byte)WoodpeckerValues.AnimationFrame.Idle, (byte)WoodpeckerValues.AnimationFrame.Pecking4, peckingAnimationSpeed, (ushort)frameHeight);
                    if (woodpeckerValues.Frame == WoodpeckerValues.AnimationFrame.Pecking1) {
                        woodpeckerValues.StartedPecking = true;
                    }
                    if (woodpeckerValues.Frame == WoodpeckerValues.AnimationFrame.Pecking4 && NPC.HasJustChangedFrame()) {
                        HitTree();
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

    private bool IsTooFarFromTree() => Vector2.Distance(GoToTreePosition, NPC.Center) > TileHelper.TileSize * 5;
    private bool IsTreeNotDestroyed() => !WorldGenHelper.ActiveTile(TreePosition - new Point16(0, 4));
    private bool IsNearTree() => NPC.Center.X == GoToTreePosition.X;

    private void HitTree() {
        SoundEngine.PlaySound(SoundID.Dig, NPC.Center);

        byte dustCount = 4;
        float baseAngle = MathHelper.PiOver4 / 2f;
        for (float i = -baseAngle; i < baseAngle; i += baseAngle / dustCount) {
            Vector2 dustPosition = NPC.Top + new Vector2((NPC.width - 2f) * NPC.spriteDirection, -2f);
            ushort dustType = TreeDustType;
            Vector2 dustVelocity = (dustPosition - NPC.Center).SafeNormalize().RotatedBy(MathHelper.Pi + baseAngle + i * Main.rand.NextFloat(0.5f, 1f) - baseAngle * NPC.direction) - Vector2.UnitY * Main.rand.NextFloat(1f, 3f) * Main.rand.NextFloat(0.5f, 1f);
            dustVelocity *= Main.rand.NextFloat();
            Dust dust = Dust.NewDustPerfect(dustPosition, dustType, dustVelocity);
            dust.scale = Main.rand.NextFloat(0.75f, 1f);
        }
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

    private bool NoOtherWoodpeckerNearby(Vector2 goToTreePosition) {
        bool noOtherWoodpeckerNearby = true;
        foreach (NPC woodpeckerCheckNPC in TrackedEntitiesSystem.GetTrackedNPC<Woodpecker>()) {
            if (woodpeckerCheckNPC.SameAs(NPC)) {
                continue;
            }

            if (new WoodpeckerValues(woodpeckerCheckNPC).IsPecking &&
                woodpeckerCheckNPC.Center.DistanceX(goToTreePosition) < TileHelper.TileSize * 2) {
                noOtherWoodpeckerNearby = false;
                break;
            }
        }
        return noOtherWoodpeckerNearby;
    }

    private bool HaveFreeTreeNearby(out Vector2 goToTreePosition, out Point16 treePosition) {
        bool result = false;
        bool haveTreeNearby = false;
        int checkXDistance = 7;
        goToTreePosition = NPC.Center;
        treePosition = Point16.Zero;
        bool anyTree(int checkX, out Vector2 goToTreePosition, out Point16 treePosition) {
            bool result = false;
            goToTreePosition = NPC.Center;

            treePosition = Point16.Zero;
            int x = (int)NPC.Center.X / 16 + checkX, y = (int)NPC.Center.Y / 16;
            Tile checkTile = WorldGenHelper.GetTileSafely(x, y);
            bool isBigTree = false;
            bool isTrunk = (checkTile.ActiveTile(TileID.Trees) || checkTile.ActiveTile(TileID.PalmTree)) &&
                           !(checkTile.TileFrameX >= 1 * 22 && checkTile.TileFrameX <= 2 * 22 &&
                             checkTile.TileFrameY >= 6 * 22 && checkTile.TileFrameY <= 8 * 22) &&
                           !(checkTile.TileFrameX == 3 * 22 &&
                             checkTile.TileFrameY >= 0 && checkTile.TileFrameY <= 2 * 22) &&
                           !(checkTile.TileFrameX == 4 * 22 &&
                             checkTile.TileFrameY >= 3 * 22 && checkTile.TileFrameY <= 5 * 22) &&
                           checkTile.TileFrameY <= 198;
            if (!isTrunk) {
                ushort bigTreeType = (ushort)ModContent.TileType<BackwoodsBigTree>();
                Tile checkTileRight = WorldGenHelper.GetTileSafely(x - 1, y),
                     checkTileLeft = WorldGenHelper.GetTileSafely(x + 1, y);
                isTrunk = checkTile.ActiveTile(bigTreeType) && BackwoodsBigTree.IsTrunk(x, y) &&
                          ((NPC.IsFacingRight() && checkTileRight.ActiveTile(bigTreeType) && BackwoodsBigTree.IsTrunk(x - 1, y)) ||
                           (NPC.IsFacingLeft() && checkTileLeft.ActiveTile(bigTreeType) && BackwoodsBigTree.IsTrunk(x + 1, y)));
                isBigTree = isTrunk;
            }
            if (isTrunk) {
                result = true;
                treePosition = new Point16(x, y);
                goToTreePosition = treePosition.ToWorldCoordinates();
                int directionToTree = (goToTreePosition.X - NPC.Center.X).GetDirection();
                float treeOffsetFactor = isBigTree ? (directionToTree == -1 ? 1.3f : 1.875f) : 1.3f;
                goToTreePosition += -Vector2.UnitX * NPC.width * directionToTree * treeOffsetFactor;
                Point16 goToTreePositionInTiles = goToTreePosition.ToTileCoordinates16();
                if (WorldGen.SolidOrSlopedTile(goToTreePositionInTiles.X, goToTreePositionInTiles.Y)) {
                    result = false;
                }
            }
            if (TreePositionsTaken.Contains(goToTreePosition)) {
                result = false;
            }
            return result;
        }
        if (NPC.IsFacingRight()) {
            for (int checkX = checkXDistance; checkX >= -checkXDistance; checkX--) {
                if (anyTree(checkX, out goToTreePosition, out treePosition)) {
                    haveTreeNearby = true;
                    break;
                }
            }
        }
        else {
            for (int checkX = -checkXDistance; checkX <= -checkXDistance; checkX++) {
                if (anyTree(checkX, out goToTreePosition, out treePosition)) {
                    haveTreeNearby = true;
                    break;
                }
            }
        }
        bool noOtherWoodpeckerNearby = NoOtherWoodpeckerNearby(goToTreePosition);
        if (haveTreeNearby && noOtherWoodpeckerNearby) {
            result = true;
        }
        if (!result) {
            goToTreePosition = NPC.Center;
            treePosition = Point16.Zero;
        }
        if (!NPC.IsGrounded()) {
            result = false;
        }
        return result;
    }
}
