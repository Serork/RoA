using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Content.Biomes.Backwoods;
using RoA.Content.Tiles.Ambient.LargeTrees;
using RoA.Core;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.IO;

using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.NPCs.Enemies.Backwoods.Hardmode;

[Tracked]
sealed class Woodpecker : ModNPC {
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
        bestiaryEntry.Info.AddRange([
            new FlavorTextBestiaryInfoElement($"Mods.RoA.Bestiary.{nameof(Woodpecker)}")
        ]);
    }

    private static byte FRAMECOUNT => 16;
    private static float PECKINGCHECKTIME => 20f;
    private static float TONGUEATTACKCHECKTIME => 120f;
    private static ushort TRIGGERAREASIZE => 1000;
    private static float MINDISTANCEREQUIREDTOSTARTONGUEATTACK => 600f;

    public ref struct WoodpeckerValues(NPC npc) {
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
        public ref float TongueSpawnedValue = ref npc.ai[2];
        public ref float ShouldBePeckingTimer = ref npc.ai[3];
        public ref float StartedPeckingValue = ref npc.ai[3];
        public ref float TargetClosestTimer = ref npc.ai[3];

        public AIState State {
            readonly get => (AIState)StateValue;
            set => StateValue = Utils.Clamp((byte)value, (byte)AIState.Idle, (byte)AIState.Count);
        }

        public AnimationFrame Frame {
            readonly get => (AnimationFrame)FrameValue;
            set {
                byte frameToSet = Utils.Clamp((byte)value, (byte)AnimationFrame.Idle, (byte)AnimationFrame.Count);
                FrameValue = frameToSet;
            }
        }

        public bool StartedPecking {
            readonly get => StartedPeckingValue == 1f;
            set => StartedPeckingValue = value.ToInt();
        }

        public bool ShouldTongueBeSpawned {
            readonly get => TongueSpawnedValue == 1f;
            set => TongueSpawnedValue = value.ToInt();
        }

        public readonly bool IsPecking => State == AIState.GoingToTree || State == AIState.Pecking || (State == AIState.Idle && ShouldBePeckingTimer > 0f);

        public void ResetAllTimers() => TongueAttackTimer = CanBeBusyWithActionTimer = CanGoToTreeAgainTimer = EncouragementTimer = TargetClosestTimer = 0f;
    }

    public Vector2 ChoosenTreeWorldPosition;
    public Point16 ChoosenTreeTilePosition;
    public float VelocityXFactor = 1f;
    public HashSet<Vector2> TreePositionsTaken = [];
    public NPC? Tongue;
    public float TimeToAttack;

    public int DirectionToTree => ChoosenTreeWorldPosition == Vector2.Zero ? 0 : ChoosenTreeTilePosition.ToWorldCoordinates().GetDirectionTo(NPC.Center);

    public bool HasTongue => Tongue != null && Tongue.active;
    public bool IsTongueNotActive {
        get {
            if (Tongue == null) {
                return false;
            }
            WoodpeckerTongue.WoodpeckerTongueValues woodpeckerTongueValues = new(Tongue);
            return !Tongue.active || woodpeckerTongueValues.TongueAttackProgress > 0.5f && woodpeckerTongueValues.Progress < 0.625f;
        }
    }

    public Vector2 TonguePosition => NPC.Top + new Vector2(0f, -1f);

    public override void SetStaticDefaults() {
        NPC.SetFrameCount(FRAMECOUNT);
    }

    public override void SetDefaults() {
        NPC.SetSizeValues(34, 64);
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
        float maxSpeed = 2f + NPC.GetRemainingHealthPercentage() * 1.5f,
              acceleration = 0.07f,
              deceleration = 0.8f;
        void handleXMovement() {
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
        void goToChosenTree() {
            handleXMovement();
            float stoppingDistance = 10f;
            float distance = NPC.Center.DistanceX(ChoosenTreeWorldPosition);
            if (distance < stoppingDistance * 2f) {
                float velocityXFactor = distance / (stoppingDistance * 2f);
                VelocityXFactor = Helper.Approach(VelocityXFactor, velocityXFactor, 0.025f);
                NPC.velocity.X *= VelocityXFactor;
                if (NPC.SpeedX() < 0.1f) {
                    NPC.velocity.X = 0f;
                }
                NPC.position.X = Helper.Approach(NPC.position.X, ChoosenTreeWorldPosition.X - NPC.width / 2f, 0.5f * (1f - VelocityXFactor));
            }
            NPC.DirectTo(ChoosenTreeWorldPosition, updateSpriteDirection: false);
            TreePositionsTaken.Add(ChoosenTreeWorldPosition);
            NPC.spriteDirection = DirectionToTree;
            NPC.StepUp();
        }
        void resetChosenTreeStoredData() {
            ChoosenTreeWorldPosition = Vector2.Zero;
            ChoosenTreeTilePosition = Point16.Zero;
            VelocityXFactor = 1f;
        }
        void handleTongueAttackState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (woodpeckerValues.State != WoodpeckerValues.AIState.TongueAttack) {
                return;
            }

            if (IsTongueNotActive) {
                if (woodpeckerValues.Frame == WoodpeckerValues.AnimationFrame.TongueAttack1 && !woodpeckerValues.ShouldTongueBeSpawned) {
                    Tongue = null;

                    woodpeckerValues.ResetAllTimers();
                    woodpeckerValues.State = WoodpeckerValues.AIState.Idle;
                }
            }
            else {
                if (woodpeckerValues.Frame == WoodpeckerValues.AnimationFrame.TongueAttack2 && !woodpeckerValues.ShouldTongueBeSpawned) {
                    woodpeckerValues.ShouldTongueBeSpawned = true;
                }
            }

            if (!NPC.HasValidTarget) {
                woodpeckerValues.State = WoodpeckerValues.AIState.Idle;
            }
            else if (Helper.SinglePlayerOrServer && woodpeckerValues.Frame == WoodpeckerValues.AnimationFrame.TongueAttack3 && woodpeckerValues.ShouldTongueBeSpawned) {
                Tongue = NPC.NewNPCDirect(NPC.GetSource_FromAI(), NPC.Center, (ushort)ModContent.NPCType<WoodpeckerTongue>(), ai0: NPC.whoAmI);
                woodpeckerValues.ShouldTongueBeSpawned = false;
            }

            NPC.velocity.X *= deceleration;
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

            goToChosenTree();

            if (shouldTargetPlayer || !NoOtherWoodpeckerNearby(ChoosenTreeWorldPosition) || IsTreeNotDestroyed()) {
                woodpeckerValues.ShouldBePeckingTimer = 0f;
                woodpeckerValues.State = WoodpeckerValues.AIState.Walking;
                return;
            }

            bool shouldBePecking = woodpeckerValues.ShouldBePeckingTimer++ >= PECKINGCHECKTIME;
            if (ReachedTree() && shouldBePecking) {
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
                ChoosenTreeWorldPosition = goToTreePosition;
                ChoosenTreeTilePosition = treePosition;
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

            resetChosenTreeStoredData();

            NPCExtensions.FighterAI.ApplyFighterAI(NPC, ref woodpeckerValues.CanBeBusyWithActionTimer,
                                                        ref woodpeckerValues.EncouragementTimer, 
                                                        ref woodpeckerValues.TargetClosestTimer,
                                                        shouldTargetPlayer: shouldTargetPlayer,
                                                        xMovement: handleXMovement,
                                                        shouldBeBored: (npc) => npc.SpeedX() < 1f && npc.Center.DistanceX(npc.GetTargetPlayer().Center) < npc.width);

            if (NPC.IsGrounded()) {
                if (Helper.SinglePlayerOrServer && TimeToAttack <= 0f) {
                    TimeToAttack = Main.rand.NextFloat(TONGUEATTACKCHECKTIME * 0.75f, TONGUEATTACKCHECKTIME * 1.5f);
                    if (Main.expertMode) {
                        TimeToAttack = MathHelper.Lerp(TimeToAttack, TimeToAttack * 0.5f, NPC.GetRemainingHealthPercentage());
                    }
                    NPC.netUpdate = true;
                }

                if (shouldTargetPlayer && NPC.GetTargetPlayer().Distance(NPC.Center) < MINDISTANCEREQUIREDTOSTARTONGUEATTACK && woodpeckerValues.TongueAttackTimer++ > TimeToAttack) {
                    woodpeckerValues.ResetAllTimers();
                    woodpeckerValues.State = WoodpeckerValues.AIState.TongueAttack;

                    TimeToAttack = 0f;
                }

                if (HaveFreeTreeNearby(out _, out _) && !shouldTargetPlayer && woodpeckerValues.CanGoToTreeAgainTimer-- <= 0f) {
                    woodpeckerValues.ResetAllTimers();
                    woodpeckerValues.State = WoodpeckerValues.AIState.Idle;
                }
            }
        }
        void handlePeckingState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (woodpeckerValues.State != WoodpeckerValues.AIState.Pecking) {
                return;
            }

            goToChosenTree();
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

        handleTongueAttackState();
        handleGoingToTreeState();
        handleIdleState();
        handleFighterState();
        handlePeckingState();
        handleAirborneState();
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
        if (!HasTongue) {
            goto End;
        }

        Tongue!.As<WoodpeckerTongue>().DrawSelf(spriteBatch);

        End:
        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }

    public override void FindFrame(int frameHeight) {
        void walkingAnimation() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            if (NPC.IsGrounded()) {
                if (NPC.SpeedX() < 0.1f) {
                    woodpeckerValues.Frame = WoodpeckerValues.AnimationFrame.Idle;
                    return;
                }
                byte walkingAnimationSpeed = 15;
                double additionalCounter = MathF.Max(1f, NPC.velocity.Length());
                woodpeckerValues.Frame = (WoodpeckerValues.AnimationFrame)NPC.AnimateFrame((byte)woodpeckerValues.Frame, (byte)WoodpeckerValues.AnimationFrame.Walking1, (byte)WoodpeckerValues.AnimationFrame.Walking6, walkingAnimationSpeed, (ushort)frameHeight, additionalCounter);
            }
            else if (NPC.velocity.Y < 0f || NPC.velocity.Y > 1f) {
                woodpeckerValues.Frame = WoodpeckerValues.AnimationFrame.Jump;
            }
        }
        void animatePerState() {
            WoodpeckerValues woodpeckerValues = new(NPC);
            switch (woodpeckerValues.State) {
                case WoodpeckerValues.AIState.Idle:
                    woodpeckerValues.Frame = NPC.IsGrounded() ? WoodpeckerValues.AnimationFrame.Idle : WoodpeckerValues.AnimationFrame.Jump;
                    break;
                case WoodpeckerValues.AIState.Walking:
                case WoodpeckerValues.AIState.GoingToTree:
                    walkingAnimation();
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
                case WoodpeckerValues.AIState.TongueAttack:
                    if (NPC.SpeedX() < 0.5f) {
                        byte tongueAttackAnimationSpeed = 10;
                        if (!woodpeckerValues.ShouldTongueBeSpawned && IsTongueNotActive) {
                            woodpeckerValues.Frame = (WoodpeckerValues.AnimationFrame)NPC.AnimateFrame((byte)woodpeckerValues.Frame, (byte)WoodpeckerValues.AnimationFrame.TongueAttack4, (byte)WoodpeckerValues.AnimationFrame.TongueAttack1, tongueAttackAnimationSpeed, (ushort)frameHeight,
                                resetAnimation: false);
                        }
                        else {
                            woodpeckerValues.Frame = (WoodpeckerValues.AnimationFrame)NPC.AnimateFrame((byte)woodpeckerValues.Frame, (byte)WoodpeckerValues.AnimationFrame.TongueAttack1, (byte)WoodpeckerValues.AnimationFrame.TongueAttack4, tongueAttackAnimationSpeed, (ushort)frameHeight,
                                resetAnimation: false);
                        }
                    }
                    else {
                        walkingAnimation();
                    }
                    break;
            }
        }
        void setDirection() {
            if (new WoodpeckerValues(NPC).State == WoodpeckerValues.AIState.TongueAttack) {
                NPC.DirectTo(Main.player[NPC.target].Center);
            }
            if (ShouldUpdateDirection()) {
                NPC.UpdateDirectionBasedOnVelocity();
            }
        }

        setDirection();
        animatePerState();
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(TimeToAttack);
    public override void ReceiveExtraAI(BinaryReader reader) => TimeToAttack = reader.ReadSingle();

    private bool IsTooFarFromTree() => Vector2.Distance(ChoosenTreeWorldPosition, NPC.Center) > TileHelper.TileSize * 5;
    private bool IsTreeNotDestroyed() => !WorldGenHelper.ActiveTile(ChoosenTreeTilePosition - new Point16(0, 4));
    private bool ReachedTree() => NPC.Center.X == ChoosenTreeWorldPosition.X;

    private void HitTree() {
        SoundEngine.PlaySound(SoundID.Dig, NPC.Center);

        byte dustCount = 4;
        float baseAngle = MathHelper.PiOver4 / 2f;
        for (float i = -baseAngle; i < baseAngle; i += baseAngle / dustCount) {
            Vector2 dustPosition = NPC.Center + new Vector2((NPC.width - 4f) * NPC.spriteDirection, -NPC.height / 3f);
            ushort dustType = TileHelper.GetTreeDustType(ChoosenTreeTilePosition);
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
               woodpeckerValues.State != WoodpeckerValues.AIState.GoingToTree &&
               woodpeckerValues.State != WoodpeckerValues.AIState.TongueAttack;
    }
    private bool ShouldUpdateDirection() {
        WoodpeckerValues woodpeckerValues = new(NPC);
        return woodpeckerValues.State != WoodpeckerValues.AIState.GoingToTree &&
               woodpeckerValues.State != WoodpeckerValues.AIState.Pecking &&
               woodpeckerValues.State != WoodpeckerValues.AIState.TongueAttack;
    }

    private bool NoOtherWoodpeckerNearby(Vector2 goToTreePosition) {
        bool noOtherWoodpeckerNearby = true;
        foreach (NPC woodpeckerCheckNPC in TrackedEntitiesSystem.GetTrackedNPC<Woodpecker>((checkNPC) => checkNPC.SameAs(NPC))) {
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
            bool isTrunk = TileHelper.IsTreeTrunk(checkTile);
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
                int directionToTree = goToTreePosition.GetDirectionTo(NPC.Center);
                float treeOffsetFactor = isBigTree ? (directionToTree == -1 ? 1.3f : 1.875f) : 1.3f;
                treeOffsetFactor *= 0.8f;
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
            for (int checkX = -checkXDistance; checkX <= checkXDistance; checkX++) {
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

// rags are taken from calamity fables
sealed class WoodpeckerTongue : ModNPC {
    public ref struct WoodpeckerTongueValues(NPC npc) {
        public ref float InitOnSpawnValue = ref npc.localAI[0];

        public ref float WoodpeckerWhoAmIValue = ref npc.ai[0];
        public ref float RagsSine = ref npc.ai[1];
        public ref float AttackTimer = ref npc.ai[2];
        public ref float ProgressValue = ref npc.ai[3];

        public bool Init {
            readonly get => InitOnSpawnValue == 1f;
            set => InitOnSpawnValue = value.ToInt();
        }

        public float Progress {
            readonly get => ProgressValue;
            set => ProgressValue = MathUtils.Clamp01(value);
        }

        public readonly int WoodpeckerWhoAmIThatIBelong => (int)WoodpeckerWhoAmIValue;
        public readonly NPC? WoodpeckerThatIBelong => WoodpeckerWhoAmIThatIBelong >= 0 ? Main.npc[WoodpeckerWhoAmIThatIBelong] : null;

        public readonly float TongueAttackProgress => Utils.Remap(AttackTimer, TimeToAttack / 3f, TimeToAttack, 0f, 1f) * Utils.Remap(AttackTimer, TimeToAttack * 1.115f, TimeToAttack * 1.445f, 1f, 0f);

        private readonly float TimeToAttack => npc.As<WoodpeckerTongue>().TimeToAttack;
    }

    public VerletNet? Rags;
    public float TimeToAttack;

    public override void SetDefaults() {
        NPC.SetSizeValues(40);
        NPC.DefaultToEnemy(new NPCExtensions.NPCHitInfo(500, 40, 16, 0f));

        NPC.aiStyle = -1;

        NPC.immortal = true;
        NPC.HideStrikeDamage = true;
    }

    public override void AI() {
        void init() {
            WoodpeckerTongueValues woodpeckerTongueValues = new(NPC);
            if (!woodpeckerTongueValues.Init) {
                woodpeckerTongueValues.Init = true;
                NPC woodpeckerThatIBelong = woodpeckerTongueValues.WoodpeckerThatIBelong!;
                NPC.defense = woodpeckerThatIBelong.defense / 2;
                NPC.velocity = Vector2.One.RotatedBy(woodpeckerThatIBelong.direction == -1 ? MathHelper.Pi : -MathHelper.PiOver2) * 40f;
            }
        }
        void checkActive() {
            WoodpeckerTongueValues woodpeckerTongueValues = new(NPC);
            NPC? woodpeckerThatIBelong = woodpeckerTongueValues.WoodpeckerThatIBelong;
            if (woodpeckerThatIBelong == null || !woodpeckerThatIBelong.active) {
                NPC.KillNPC();
                return;
            }

            if (woodpeckerTongueValues.AttackTimer <= TimeToAttack) {
                woodpeckerTongueValues.Progress = Helper.Approach(woodpeckerTongueValues.Progress, 1f, 0.05f);
                if (woodpeckerTongueValues.Progress >= 1f) {
                    woodpeckerTongueValues.AttackTimer += 1f;
                }
            }
            else {
                woodpeckerTongueValues.Progress = Helper.Approach(woodpeckerTongueValues.Progress, 0f, 0.05f);
                if (woodpeckerTongueValues.Progress <= 0f) {
                    NPC.KillNPC();
                }
            }

            List<Vector2> ragPoints = GetRagPoints();
            Vector2 lastPoint = ragPoints[ragPoints.Count - 1];
            if (!Utils.HasNaNs(lastPoint)) {
                NPC.Center = lastPoint - NPC.velocity;
                return;
            }

            NPC.Center = woodpeckerThatIBelong.As<Woodpecker>().TonguePosition;
        }
        void setAttackTime() {
            WoodpeckerTongueValues woodpeckerTongueValues = new(NPC);
            if (Helper.SinglePlayerOrServer && TimeToAttack <= 0f) {
                TimeToAttack = Main.rand.NextFloat(160f, 240f);
                if (Main.expertMode) {
                    TimeToAttack = MathHelper.Lerp(TimeToAttack, TimeToAttack * 0.5f, woodpeckerTongueValues.WoodpeckerThatIBelong!.GetRemainingHealthPercentage());
                }
                woodpeckerTongueValues.RagsSine = 30;
                NPC.netUpdate = true;
            }
        }
        void updateRags() {
            if (Rags == null) {
                Rags = new VerletNet();
                InitializeRags();
            }
            else {
                SimulateRags();
            }
        }
        init();
        updateRags();
        checkActive();
        setAttackTime();
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(TimeToAttack);
    public override void ReceiveExtraAI(BinaryReader reader) => TimeToAttack = reader.ReadSingle();

    public override void HitEffect(NPC.HitInfo hit) {
        WoodpeckerTongueValues woodpeckerTongueValues = new(NPC);
        NPC woodpeckerThatIBelong = woodpeckerTongueValues.WoodpeckerThatIBelong!;
        hit.Damage = (int)(hit.Damage * 1.5f);
        woodpeckerThatIBelong.StrikeNPC(hit);
        if (Main.netMode != NetmodeID.SinglePlayer) {
            NetMessage.SendStrikeNPC(woodpeckerThatIBelong, hit);
        }
    }

    public void DrawSelf(SpriteBatch spriteBatch) {
        Texture2D texture = TextureAssets.Npc[Type].Value;
        WoodpeckerTongueValues woodpeckerTongueValues = new(NPC);
        NPC woodpeckerThatIBelong = woodpeckerTongueValues.WoodpeckerThatIBelong!;
        List<Vector2> drawPoints = GetRagPoints();
        for (int index = 2; index < drawPoints.Count - 1; index++) {
            Vector2 point = drawPoints[index];
            Rectangle sourceRectangle = GetClipPerSegment(index, drawPoints.Count - 1);
            if (index == 2) {
                sourceRectangle = GetClipPerSegment(0, drawPoints.Count - 1);
            }
            Vector2 nextPosition = drawPoints[Math.Min(drawPoints.Count - 1, index + 1)];
            Vector2 position = point;
            float rotation = position.DirectionTo(nextPosition).ToRotation() + MathHelper.PiOver2;
            Vector2 origin = new(sourceRectangle.Width / 2f, sourceRectangle.Height);
            SpriteEffects flip = woodpeckerThatIBelong.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color color = Lighting.GetColor(position.ToTileCoordinates());
            spriteBatch.Draw(texture, position, DrawInfo.Default with {
                Clip = sourceRectangle,
                Origin = origin,
                Rotation = rotation,
                ImageFlip = flip,
                Color = color
            });
        }
    }

    private List<Vector2> GetRagPoints() {
        List<Vector2> result = [];
        if (Rags == null) {
            return result;
        }

        WoodpeckerTongueValues woodpeckerTongueValues = new(NPC);
        Vector2 previousPosition = Vector2.Zero;
        int count = Rags.Points.Count;
        for (int index = 0; index < count; index++) {
            Vector2 point = Rags.Points[index];
            int nextIndex = Math.Min(count - 1, index + 1);
            Vector2 nextPosition = Rags.Points[nextIndex];
            Rectangle sourceRectangle = GetClipPerSegment(index, count);
            float heightProgress = Utils.Remap(woodpeckerTongueValues.Progress / (index - 2), 1f / count, 1f / Math.Min(count - 1, index - 2 + 1), 0f, 1f);
            if (index == count - 2) {
                heightProgress = 1f;
            }
            sourceRectangle.Height = (int)(sourceRectangle.Height * heightProgress);
            Vector2 velocity = point.DirectionTo(nextPosition) * (sourceRectangle.Height * 1f);
            if (previousPosition == Vector2.Zero) {
                Vector2 tongueStartPosition = point - velocity * 0.7f;
                previousPosition = tongueStartPosition;
            }
            result.Add(previousPosition);
            previousPosition += velocity;
        }

        return result;
    }
    private Rectangle GetClipPerSegment(int index, int count) {
        Rectangle sourceRectangle;
        if (count < 2) {
            sourceRectangle = new(0, 0, 20, 24);
        }
        else if (index != 0) {
            if (index != count - 1) {
                sourceRectangle = new(6, 26, 8, 12);
            }
            else {
                sourceRectangle = new(0, 0, 20, 24);
            }
        }
        else {
            sourceRectangle = new(6, 40, 8, 14);
        }
        return sourceRectangle;
    }

    // calamity fables
    private void InitializeRags() {
        Rags!.Clear();
        WoodpeckerTongueValues woodpeckerTongueValues = new(NPC);
        NPC woodpeckerThatIBelong = woodpeckerTongueValues.WoodpeckerThatIBelong!;
        Vector2 tonguePosition = woodpeckerThatIBelong.As<Woodpecker>().TonguePosition;
        VerletPoint start = new(tonguePosition, true);
        VerletPoint end = new(tonguePosition + NPC.velocity);
        Rags.AddChain(start, end, 10, 2f, (progress) => start.Position.DirectionTo(end.Position) * (1f - progress));
    }
    private void SimulateRags() {
        WoodpeckerTongueValues woodpeckerTongueValues = new(NPC);
        NPC woodpeckerThatIBelong = woodpeckerTongueValues.WoodpeckerThatIBelong!;
        ref float RagsSine = ref woodpeckerTongueValues.RagsSine;

        Vector2 tonguePosition = woodpeckerThatIBelong.As<Woodpecker>().TonguePosition;
        Rags!.Extremities[0].Position = tonguePosition;

        if (woodpeckerTongueValues.TongueAttackProgress >= 0.5f && woodpeckerTongueValues.TongueAttackProgress < 0.75f) {
            float progress = MathUtils.Clamp01((woodpeckerTongueValues.TongueAttackProgress - 0.5f) / 0.25f);
            Rags.Extremities[1].Position = Vector2.SmoothStep(Rags.Extremities[1].Position, woodpeckerThatIBelong.GetTargetPlayer().Center, progress * 0.75f);

            var point1 = Rags.Extremities[0];
            var point2 = Rags.Extremities[1];
            int count = Rags.Segments.Count;
            int length = Math.Min(30, (int)Vector2.Distance(woodpeckerThatIBelong.Center, woodpeckerThatIBelong.GetTargetPlayer().Center) / 10);
            if (count < length) {
                Rags.Clear();
                Rags.AddChain(point1, point2, count + 1, 2f);
            }
        }

        bool needsFlip = false;
        bool falling = woodpeckerThatIBelong.velocity.Y > 5;

        int indexAlongTrail = 0;
        int ragIndex = 0;

        foreach (VerletPoint point in Rags.Points) {
            float progressAlongTrail = indexAlongTrail / ((float)Rags.Points.Count * woodpeckerTongueValues.Progress);

            //A directional push towards the player's back. More or less strong depending on how fast the player is moving
            Vector2 customGravity = Vector2.UnitX * woodpeckerThatIBelong.direction * ((needsFlip ? 1.4f : 0.3f));

            switch (ragIndex) {
                case 0:
                    customGravity.Y -= 1.1f;
                    float xFactor = (float)Math.Sin(progressAlongTrail * 4f - RagsSine * 0.10f) * (0.2f + 0.8f * (float)Math.Sin(progressAlongTrail * 2.4f));
                    customGravity.X += 2f * xFactor * woodpeckerThatIBelong.direction * woodpeckerTongueValues.Progress;
                    customGravity.Y += 0.9f * (float)Math.Sin(progressAlongTrail * 9.6f + RagsSine * 0.10f) * (0.2f + 0.8f * (float)Math.Sin(progressAlongTrail * 2.4f));

                    break;
            }

            point.CustomGravity = customGravity;

            indexAlongTrail++;
            if (indexAlongTrail == 15) {
                ragIndex++;
                indexAlongTrail = 0;
            }
        }

        int iterations = 2;

        Rags.Update(iterations, 0f, 0.7f);

        RagsSine += 1f;
        if (Main.expertMode) {
            RagsSine += 0.5f * woodpeckerTongueValues.WoodpeckerThatIBelong!.GetRemainingHealthPercentage();
        }
    }
}
