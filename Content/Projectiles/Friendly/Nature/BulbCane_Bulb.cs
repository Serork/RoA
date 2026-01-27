using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class Bulb : NatureProjectile_NoTextureLoad, IRequestAssets, IUseCustomImmunityFrames {
    // separate
    private class Bulb_DamageCounter : GlobalProjectile {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
            AcceptDamage(hit.Damage, target.Center, projectile);
        }

        public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) {
            AcceptDamage(info.Damage, target.Center, projectile);
        }

        private void AcceptDamage(int finalDamage, Vector2 damageSourcePosition, Projectile projectile) {
            if (!projectile.IsNature()) {
                return;
            }
            foreach (Projectile bulbProjectile in TrackedEntitiesSystem.GetTrackedProjectile<Bulb>(checkProjectile => !checkProjectile.SameOwnerAs(projectile))) {
                bulbProjectile.As<Bulb>().AcceptDamage(damageSourcePosition, finalDamage);
            }
        }
    }

    private static ushort TIMELEFT => MathUtils.SecondsToFrames(600);

    private static byte MAXCOUNT => 1;

    private static byte BULBFRAMECOUNT_ROW => 7;
    private static byte BULBFRAMECOUNT_COLUMN => 2;
    private static byte BULBTRANSFORMRAMECOUNT_ROW => 4;
    private static byte BULB2FRAMECOUNT_ROW => 5;

    private static byte LEAFFRAMECOUNT => 2;
    private static byte STAMENFRAMECOUNT_YELLOW => 3;
    private static byte STAMENFRAMECOUNT_GREEN => 3;
    private static byte SUMMONMOUTHFRAMECOUNT => 2;
    private static Point16 TENTACLETARGETZONESIZE => new(100, 125);

    private static float ROOTLENGTH => TileHelper.TileSize * 7;
    private static byte SUMMONMOUTHCOUNT => 6;
    private static byte SUMMONTENTACLECOUNT => 3;
    private static byte STAMENACTIVATIONSLOTCOUNT => 6;
    private static ushort DAMAGENEEDEDPERSTAMEN => 100;
    private static float TENTACLESEGMENTHEIGHT => 9;
    private static ushort SUMMONMOUTHHITBOXSIZE => 30;
    private static ushort SUMMONTENTACLEHITBOXSIZE => 8;
    private static ushort DISSAPEARTSTARTTIMELEFT => 50;
    private static ushort ACCEPTDAMAGEDISTANCE => (ushort)(TileHelper.TileSize * 20);

    public enum Bulb_RequstedTextureType : byte { 
        Bulb,
        Bulb_Back,
        Bulb2,
        Bulb2_Back,
        Stem,
        Stem2,
        Stem3,
        LeafStem,
        Leaf,
        Stamen_Yellow,
        Stamen_Green,
        SummonMouth,
        SummonTentacle,
        SummonTentacle2
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)Bulb_RequstedTextureType.Bulb, ResourceManager.NatureProjectileTextures + "Bulb"),
         ((byte)Bulb_RequstedTextureType.Bulb_Back, ResourceManager.NatureProjectileTextures + "Bulb_Back"),
         ((byte)Bulb_RequstedTextureType.Bulb2, ResourceManager.NatureProjectileTextures + "Bulb2"),
         ((byte)Bulb_RequstedTextureType.Bulb2_Back, ResourceManager.NatureProjectileTextures + "Bulb2_Back"),
         ((byte)Bulb_RequstedTextureType.Stem, ResourceManager.NatureProjectileTextures + "Bulb_Stem"),
         ((byte)Bulb_RequstedTextureType.Stem2, ResourceManager.NatureProjectileTextures + "Bulb_Stem2"),
         ((byte)Bulb_RequstedTextureType.Stem3, ResourceManager.NatureProjectileTextures + "Bulb_Stem3"),
         ((byte)Bulb_RequstedTextureType.LeafStem, ResourceManager.NatureProjectileTextures + "Bulb_LeafStem"),
         ((byte)Bulb_RequstedTextureType.Leaf, ResourceManager.NatureProjectileTextures + "Bulb_Leaf"),
         ((byte)Bulb_RequstedTextureType.Stamen_Yellow, ResourceManager.NatureProjectileTextures + "Bulb_Stamen_Yellow"),
         ((byte)Bulb_RequstedTextureType.Stamen_Green, ResourceManager.NatureProjectileTextures + "Bulb_Stamen_Green"),
         ((byte)Bulb_RequstedTextureType.SummonMouth, ResourceManager.NatureProjectileTextures + "Bulb_SummonMouth"),
         ((byte)Bulb_RequstedTextureType.SummonTentacle, ResourceManager.NatureProjectileTextures + "Bulb_SummonTentacle"),
         ((byte)Bulb_RequstedTextureType.SummonTentacle2, ResourceManager.NatureProjectileTextures + "Bulb_SummonTentacle2")];

    public record struct SummonMouthInfo(Vector2 Position, Vector2 Destination, Vector2 Velocity, float Rotation, float BaseRotation, bool FacedRight);
    public record struct SummonTentacleInfo(Vector2 RootPosition, Vector2 BaseDestination, Vector2 CurrentDestination, List<Vector2> SegmentPositions, List<float> SegmentRotations, float WaveFrequency) {
        public readonly int SegmentCount => SegmentPositions.Count;
    }
    public record struct StamenDamageInfo(int DamageDone, int DamageNeeded) {
        public readonly float Progress => MathUtils.Clamp01((float)DamageDone / DamageNeeded);
        public readonly bool Activated => Progress >= 1f;
    }
    public record struct EnergyParticleInfo(Vector2 Position, Vector2 Destination, Vector2 Velocity, bool Active);

    private SummonMouthInfo[] _summonMouthData = null!;
    private SummonTentacleInfo[] _summonTentacleData = null!;
    private StamenDamageInfo[] _stamenActivated = null!;
    private EnergyParticleInfo[] _energyParticleData = null!;
    private int _seedForRandomness;
    private bool _shouldDissapear;
    private int _nextEnergyParticleIndex;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float ShouldTransformValue => ref Projectile.localAI[1];
    public ref float TransformFactorValue => ref Projectile.localAI[2];
    public ref float RootPositionX => ref Projectile.ai[0];
    public ref float RootPositionY => ref Projectile.ai[1];
    public ref float SecondFormValue => ref Projectile.ai[2];

    public float AppearanceValue {
        get => Projectile.Opacity;
        set => Projectile.Opacity = value;
    }

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public bool ShouldTransform {
        get => ShouldTransformValue != 0f;
        set => ShouldTransformValue = value.ToInt();
    }

    public Vector2 RootPosition {
        get => new(RootPositionX, RootPositionY);
        set {
            RootPositionX = value.X;
            RootPositionY = value.Y;
        }
    }

    public bool IsSecondFormActive {
        get => SecondFormValue != 0f;
        set => SecondFormValue = value.ToInt();
    }

    public float TransformFactor => Ease.SineIn(TransformFactorValue);
    public bool IsTransforming => ShouldTransform || IsSecondFormActive;

    private ushort CustomImmunityFrameCount => (ushort)(SUMMONMOUTHCOUNT + SUMMONTENTACLECOUNT);
    private bool ShouldSummonMiseDealDamage => IsSecondFormActive;
    private bool ShouldSummonTentaclesDealDamage => IsSecondFormActive;
    private int SummonMouthCount => _summonMouthData.Length;
    private int SummonTentacleCount => _summonTentacleData.Length;

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, shouldApplyAttachedItemDamage: false);

        Projectile.SetSizeValues(10);

        Projectile.penetrate = -1;
        Projectile.friendly = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.tileCollide = false;

        Projectile.manualDirectionChange = false;
    }

    private void UpdateMaxCount(Player player) {
        IEnumerable<Projectile> list2 = TrackedEntitiesSystem.GetTrackedProjectile<Bulb>(checkProjectile => checkProjectile.SameAs(Projectile) || checkProjectile.owner != player.whoAmI);
        List<Projectile> list = [];
        foreach (Projectile projectile in list2) {
            if (projectile.timeLeft > Projectile.timeLeft) {
                list.Add(projectile);
            }
        }
        if (list.Count >= MAXCOUNT && Projectile.timeLeft > DISSAPEARTSTARTTIMELEFT) {
            Projectile.timeLeft = DISSAPEARTSTARTTIMELEFT;
        }
    }

    private void SpawnEnergyParticle(Vector2 position) {
        if (IsSecondFormActive || !Init) {
            return;
        }

        float appearanceFactor = Ease.CubeOut(AppearanceValue);
        Vector2 center = Vector2.Lerp(RootPosition, Projectile.Center, MathF.Max(0.5f, appearanceFactor));

        ref EnergyParticleInfo energyInfo = ref _energyParticleData[_nextEnergyParticleIndex++];
        energyInfo.Position = position;
        energyInfo.Active = true;
        energyInfo.Velocity = position.DirectionFrom(center).RotatedByRandom(MathHelper.Pi * Main.rand.NextFloatDirection()) * 2.5f * Main.rand.NextFloat(2.5f, 5f);
        energyInfo.Velocity = new Vector2(energyInfo.Velocity.X, MathF.Abs(energyInfo.Velocity.Y) * 1.5f);
        int direction = (AcceptedEnoughDamageProgress() > 0.5f).ToDirectionInt();
        energyInfo.Destination = center - Vector2.UnitY * 36 + Vector2.UnitX * 48f * direction + Main.rand.RandomPointInArea(10f);

        if (_nextEnergyParticleIndex > _energyParticleData.Length - 1) {
            _nextEnergyParticleIndex = 0;
        }
    }

    public override void AI() {
        UpdateMaxCount(Projectile.GetOwnerAsPlayer());
        _shouldDissapear = Projectile.timeLeft <= DISSAPEARTSTARTTIMELEFT;

        float appearanceFactor = Ease.CubeOut(AppearanceValue);
        Vector2 center = Vector2.Lerp(RootPosition, Projectile.Center, MathF.Max(0.5f, appearanceFactor));

        void init() {
            if (!Init) {
                AppearanceValue = 0f;

                float yOffset = ROOTLENGTH * Projectile.scale;
                RootPosition = Projectile.Center + Vector2.UnitY * yOffset;

                TransformFactorValue = 1f;

                if (Projectile.IsOwnerLocal()) {
                    _seedForRandomness = Main.rand.Next(100); // need sync
                }

                CustomImmunityFramesHandler.Initialize(Projectile, CustomImmunityFrameCount);

                void initEnergy() {
                    _energyParticleData = new EnergyParticleInfo[25];

                }
                void initStamens() {
                    _stamenActivated = new StamenDamageInfo[STAMENACTIVATIONSLOTCOUNT];
                    for (int i = 0; i < _stamenActivated.Length; i++) {
                        _stamenActivated[i] = new StamenDamageInfo(0, DAMAGENEEDEDPERSTAMEN);
                    }
                }
                void initSummonMise() {
                    _summonMouthData = new SummonMouthInfo[SUMMONMOUTHCOUNT];
                    int summonMouthCount = SummonMouthCount,
                        summonMouthHalfCount = summonMouthCount / 2;
                    for (int i = 0; i < summonMouthCount; i++) {
                        bool first = i < summonMouthHalfCount;
                        ref SummonMouthInfo summonMouthInfo = ref _summonMouthData[i];
                        int currentIndex = i - (!first ? summonMouthHalfCount : 0);
                        int direction = first.ToDirectionInt();
                        bool multipleOfTwo = currentIndex > 0 && currentIndex % 2 == 0;
                        float maxAngle = MathHelper.PiOver4 * 0.75f;
                        summonMouthInfo.BaseRotation = summonMouthInfo.Rotation = MathHelper.PiOver2 * direction + (currentIndex % 2 + multipleOfTwo.ToInt()) * maxAngle * direction * -multipleOfTwo.ToDirectionInt();
                        summonMouthInfo.Position = center;
                        summonMouthInfo.FacedRight = direction > 0;
                    }
                }
                void initSummonTentacles() {
                    _summonTentacleData = new SummonTentacleInfo[SUMMONTENTACLECOUNT];
                    for (int i = 0; i < SummonTentacleCount; i++) {
                        bool first = i == 0;
                        int nextIndex = i + 1;
                        ref SummonTentacleInfo summonTentacleInfo = ref _summonTentacleData[i];
                        int segmentCount = (int)((12 + 2 * nextIndex) * Projectile.scale);
                        float rootXOffset = 5f,
                              xOffset = (i % 2 == 0).ToDirectionInt() * (first ? 0f : rootXOffset),
                              yOffset = -TENTACLESEGMENTHEIGHT * Projectile.scale * segmentCount * 0.7f;
                        summonTentacleInfo.RootPosition = center + Vector2.UnitX * xOffset;
                        Vector2 destination = center + new Vector2(xOffset, yOffset);
                        summonTentacleInfo.BaseDestination = summonTentacleInfo.CurrentDestination = destination;
                        summonTentacleInfo.WaveFrequency = Main.rand.NextFloat(3f, 5f);
                        summonTentacleInfo.SegmentPositions = new List<Vector2>(segmentCount);
                        summonTentacleInfo.SegmentRotations = new List<float>(segmentCount);
                        for (int i2 = 0; i2 < segmentCount; i2++) {
                            summonTentacleInfo.SegmentPositions.Add(center);
                            summonTentacleInfo.SegmentRotations.Add(0f);
                        }
                    }
                }
                initEnergy();
                initStamens();
                initSummonMise();
                initSummonTentacles();

                Init = true;
            }
        }
        void levitate() {
            float maxOffsetY = 0.375f * Projectile.scale * 0.8f;
            float sineOffset = _seedForRandomness;
            float levitateSpeed = 2f;
            Projectile.position.Y += Helper.Wave(-maxOffsetY, maxOffsetY, levitateSpeed, sineOffset);
            float maxOffsetX = maxOffsetY / 6f;
            levitateSpeed /= 2f;
            Projectile.position.X += Helper.Wave(-maxOffsetX, maxOffsetX * 1.01f, levitateSpeed, sineOffset);
        }
        void scaleUp() {
            Projectile.scale = 1f;

            float lerpValue = 0.015f;
            float to = 1f;
            if (_shouldDissapear) {
                lerpValue *= 2f;
                to = 0f;
            }
            AppearanceValue = Helper.Approach(AppearanceValue, to, lerpValue);
        }
        void enrage() {
            bool shouldActivateEnragedState = AcceptedEnoughDamage();
            bool shouldTransformOld = ShouldTransform;
            ShouldTransform = shouldActivateEnragedState;
            if (IsSecondFormActive) {
                ShouldTransform = false;
            }
            if (shouldTransformOld != ShouldTransform) {
                Projectile.ResetFrame();
            }
            if (IsTransforming) {
                float lerpValue = TimeSystem.LogicDeltaTime;
                TransformFactorValue = Helper.Approach(TransformFactorValue, 0f, lerpValue);
            }
        }
        void animateBulb() {
            ref int frameCounter = ref Projectile.frameCounter;
            ref int frame = ref Projectile.frame;

            int frameTime = 5,
                maxFrame = ShouldTransform ? BULBTRANSFORMRAMECOUNT_ROW : IsSecondFormActive ? BULB2FRAMECOUNT_ROW : BULBFRAMECOUNT_ROW;
            if (frameCounter++ > frameTime) {
                Projectile.ResetFrameCounter();

                frame++;
            }
            if (frame >= maxFrame) {
                if (ShouldTransform) {
                    ShouldTransform = false;

                    IsSecondFormActive = true;
                }

                Projectile.ResetFrame();
            }
        }
        float dissappearanceFactor = 1f - MathUtils.Clamp01(Projectile.timeLeft / (float)DISSAPEARTSTARTTIMELEFT);
        //dissappearanceFactor = 0f;
        void processSummonMise() {
            if (!ShouldSummonMiseDealDamage) {
                return;
            }

            for (int i = 0; i < SummonMouthCount; i++) {
                ref SummonMouthInfo summonMouthInfo = ref _summonMouthData[i];
                float baseRotation = summonMouthInfo.BaseRotation;
                float scaleModifier = Projectile.scale * 0.7f;
                float offsetValue = 125f * scaleModifier;
                float centerYOffset = 50f * scaleModifier;
                Vector2 destination = center - Vector2.UnitY * centerYOffset + Vector2.UnitY.RotatedBy(baseRotation) * offsetValue + summonMouthInfo.Destination;
                float minDistance = 60f;
                if (Vector2.Distance(destination, summonMouthInfo.Position) < minDistance * scaleModifier) {
                    float maxOffsetValue = 50f * scaleModifier;
                    summonMouthInfo.Destination = Main.rand.RandomPointInArea(maxOffsetValue);
                }
                summonMouthInfo.Position += summonMouthInfo.Velocity;
                Vector2 velocity = summonMouthInfo.Velocity;
                float distanceToDestination = Vector2.Distance(Projectile.position, destination);
                float inertiaValue = 30, extraInertiaValue = inertiaValue * 5;
                float extraInertiaFactor = 1f - MathUtils.Clamp01(distanceToDestination / minDistance);
                float inertia = inertiaValue + extraInertiaValue * extraInertiaFactor;
                Helper.InertiaMoveTowards(ref velocity, summonMouthInfo.Position, destination, inertia: inertia);
                float lerpValue = 0.025f;
                float rotation = baseRotation + MathHelper.PiOver2 + summonMouthInfo.Position.DirectionTo(destination).ToRotation() * 0.375f;
                summonMouthInfo.Rotation = Utils.AngleLerp(summonMouthInfo.Rotation, rotation, lerpValue);
                summonMouthInfo.Velocity = velocity;

                //summonMouthInfo.Position = Vector2.Lerp(summonMouthInfo.Position, center, dissappearanceFactor);
            }
        }
        void processSummonTentacles() {
            //if (!IsSecondFormActive) {
            //    return;
            //}

            for (int i = 0; i < SummonTentacleCount; i++) {
                ref SummonTentacleInfo summonTentacleInfo = ref _summonTentacleData[i];

                Vector2 rootPosition = center,
                        mainDestination = summonTentacleInfo.CurrentDestination + (center - summonTentacleInfo.RootPosition);

                NPC? target = NPCUtils.FindClosestNPC(mainDestination, TENTACLETARGETZONESIZE.X);
                bool hasTarget_Inner = target is not null;
                Vector2 baseTargetCenter = hasTarget_Inner ? target!.Center : Vector2.Zero,
                        targetCenter = baseTargetCenter;
                bool hasTarget = false;
                if (hasTarget_Inner) {
                    targetCenter -= Vector2.UnitX.RotatedBy(target.AngleTo(center)) * 100f;
                    targetCenter.Y = MathF.Min(mainDestination.Y, targetCenter.Y);
                }
                bool canChase = summonTentacleInfo.CurrentDestination.Y > targetCenter.Y;
                int targetZoneXSize = TENTACLETARGETZONESIZE.X,
                    targetZoneYSize = TENTACLETARGETZONESIZE.Y;
                bool tooFarX = MathF.Abs(summonTentacleInfo.BaseDestination.X - baseTargetCenter.X) > targetZoneXSize,
                     tooFarY = MathF.Abs(summonTentacleInfo.BaseDestination.Y - baseTargetCenter.Y) > targetZoneYSize;
                if (!hasTarget_Inner) {
                    canChase = false;
                }
                if (canChase) {
                    if (tooFarX || tooFarY) {
                        canChase = false;
                    }
                }
                float tentacleChaseSpeed = TimeSystem.LogicDeltaTime * 2f;
                if (canChase) {
                    hasTarget = true;
                    summonTentacleInfo.CurrentDestination = Vector2.Lerp(summonTentacleInfo.CurrentDestination, targetCenter, tentacleChaseSpeed);
                }
                else {
                    summonTentacleInfo.CurrentDestination = Vector2.Lerp(summonTentacleInfo.CurrentDestination, summonTentacleInfo.BaseDestination, tentacleChaseSpeed);
                }

                float maxXOffset = 26f * Projectile.scale;
                float waveSinOffset = i * maxXOffset,
                      waveFrequency = summonTentacleInfo.WaveFrequency;
                float waveOffset = Helper.Wave(-maxXOffset, maxXOffset, waveFrequency, waveSinOffset);
                waveOffset *= 1f - TransformFactor;
                if (hasTarget) {
                    float rotationToTarget = Projectile.Center.AngleTo(targetCenter);
                    mainDestination += Vector2.UnitY.RotatedBy(rotationToTarget) * waveOffset;
                }
                else {
                    mainDestination.X += waveOffset;
                }

                //mainDestination = Vector2.Lerp(mainDestination, rootPosition, dissappearanceFactor);

                int segmentCount = summonTentacleInfo.SegmentCount;
                for (int i2 = 0; i2 < segmentCount; i2++) {
                    int currentSegmentIndex = i2,
                        previousSegmentIndex = Math.Max(0, currentSegmentIndex - 1),
                        nextSegmentIndex = Math.Min(segmentCount - 1, currentSegmentIndex + 1);

                    bool first = currentSegmentIndex == 0,
                         last = currentSegmentIndex == nextSegmentIndex;

                    float rotation = summonTentacleInfo.SegmentPositions[currentSegmentIndex].AngleTo(summonTentacleInfo.SegmentPositions[nextSegmentIndex]);
                    if (last) {
                        rotation = summonTentacleInfo.SegmentPositions[previousSegmentIndex].AngleTo(summonTentacleInfo.SegmentPositions[currentSegmentIndex]);
                    }
                    summonTentacleInfo.SegmentRotations[currentSegmentIndex] = rotation;

                    float dissappearanceFactor2 = MathF.Min(0.375f, dissappearanceFactor);
                    if (dissappearanceFactor > 0f) {
                        summonTentacleInfo.SegmentPositions[currentSegmentIndex] =
                            Vector2.Lerp(summonTentacleInfo.SegmentPositions[currentSegmentIndex], center, dissappearanceFactor2);
                        summonTentacleInfo.SegmentRotations[currentSegmentIndex] =
                            Utils.AngleLerp(summonTentacleInfo.SegmentRotations[currentSegmentIndex], 0f, dissappearanceFactor2);

                        continue;
                    }

                    Vector2 centerPoint(Vector2 a, Vector2 b) => new((a.X + b.X) / 2f, (a.Y + b.Y) / 2f);
                    Vector2 startPosition = summonTentacleInfo.SegmentPositions[previousSegmentIndex];
                    Vector2 destination = summonTentacleInfo.SegmentPositions[nextSegmentIndex];
                    if (previousSegmentIndex == currentSegmentIndex) {
                        startPosition = rootPosition;
                    }
                    if (last) {
                        destination = mainDestination;
                    }
                    if (first) {
                        summonTentacleInfo.SegmentPositions[currentSegmentIndex] = startPosition;
                        continue;
                    }
                    if (startPosition.Distance(destination) < 1f) {
                        continue;
                    }
                    summonTentacleInfo.SegmentPositions[currentSegmentIndex] = centerPoint(startPosition, destination);
                    float maxDistance = 100f;
                    if (summonTentacleInfo.SegmentPositions[currentSegmentIndex].Distance(destination) > maxDistance) {
                        summonTentacleInfo.SegmentPositions[currentSegmentIndex] = destination;
                    }
                }
            }
        }
        void resetDamageInfo() {
            for (int i = 0; i < CustomImmunityFrameCount; i++) {
                for (int npcId = 0; npcId < Main.npc.Length; npcId++) {
                    ref ushort immuneTime = ref CustomImmunityFramesHandler.GetImmuneTime(Projectile, (byte)i, npcId);
                    if (immuneTime > 0) {
                        immuneTime--;
                    }
                }
            }
        }
        void damageNPCs() {
            if (!Projectile.IsOwnerLocal()) {
                return;
            }

            if (ShouldSummonMiseDealDamage) {
                for (int i = 0; i < SummonMouthCount; i++) {
                    ref SummonMouthInfo summonMouthInfo = ref _summonMouthData[i];
                    Vector2 summonMouthPosition = summonMouthInfo.Position;
                    foreach (NPC npcForCollisionCheck in Main.ActiveNPCs) {
                        if (!NPCUtils.DamageNPCWithPlayerOwnedProjectile(npcForCollisionCheck, Projectile,
                                                                         ref CustomImmunityFramesHandler.GetImmuneTime(Projectile, (byte)i, npcForCollisionCheck.whoAmI),
                                                                         collided: (targetHitbox) => GeometryUtils.CenteredSquare(summonMouthPosition, SUMMONMOUTHHITBOXSIZE).Intersects(targetHitbox),
                                                                         direction: MathF.Sign(summonMouthPosition.X - npcForCollisionCheck.Center.X))) {
                            continue;
                        }
                    }
                }
            }
            if (ShouldSummonTentaclesDealDamage) {
                for (int i = SummonMouthCount; i < SummonMouthCount + SUMMONTENTACLECOUNT; i++) {
                    ref SummonTentacleInfo summonTentacleInfo = ref _summonTentacleData[i - SummonMouthCount];
                    int summonTentacleSegmentCount = summonTentacleInfo.SegmentCount - 2;
                    for (int i2 = 0; i2 < summonTentacleSegmentCount; i2++) {
                        Vector2 summongTentaclePosition = summonTentacleInfo.SegmentPositions[i2];
                        foreach (NPC npcForCollisionCheck in Main.ActiveNPCs) {
                            if (!NPCUtils.DamageNPCWithPlayerOwnedProjectile(npcForCollisionCheck, Projectile,
                                                                             ref CustomImmunityFramesHandler.GetImmuneTime(Projectile, (byte)i, npcForCollisionCheck.whoAmI),
                                                                             collided: (targetHitbox) => GeometryUtils.CenteredSquare(summongTentaclePosition, SUMMONTENTACLEHITBOXSIZE).Intersects(targetHitbox),
                                                                             direction: MathF.Sign(summongTentaclePosition.X - npcForCollisionCheck.Center.X))) {
                                continue;
                            }
                        }
                    }
                }
            }
        }
        void processEnergyParticles() {
            for (int i = 0; i < _energyParticleData.Length; i++) {
                ref EnergyParticleInfo energyInfo = ref _energyParticleData[i];
                if (!energyInfo.Active) {
                    continue;
                }

                energyInfo.Position += energyInfo.Velocity;

                Vector2 vector104 = IsSecondFormActive ? center : energyInfo.Destination;
                Vector2 value12 = vector104 - energyInfo.Position;
                if (value12.Length() < energyInfo.Velocity.Length()) {
                    energyInfo.Active = false;
                    continue;
                }

                if (energyInfo.Position.Distance(vector104) < 2f) {
                    energyInfo.Active = false;
                    continue;
                }

                value12.Normalize();
                value12 *= 15f;
                float lerpValue = 0.1f;
                energyInfo.Velocity = 
                    new Vector2(MathHelper.Lerp(energyInfo.Velocity.X, value12.X, lerpValue),
                                MathHelper.Lerp(energyInfo.Velocity.Y, value12.Y, lerpValue * 0.5f));

                float num3 = 0f;
                //float y = 0f;
                Vector2 vector6 = energyInfo.Position;
                Vector2 vector7 = energyInfo.Position - energyInfo.Velocity;
                vector7.Y -= num3 / 2f;
                vector6.Y -= num3 / 2f;
                int num5 = (int)Vector2.Distance(vector6, vector7) / 3 + 1;
                if (Vector2.Distance(vector6, vector7) % 3f != 0f)
                    num5++;

                for (float num6 = 1f; num6 <= (float)num5; num6 += 1f) {
                    if (!Main.rand.NextBool(2)) {
                        continue;
                    }
                    Dust obj = Main.dust[Dust.NewDust(Projectile.position, 0, 0, ModContent.DustType<BulbCaneGlow>(), 0f, 0f, 100, default, Main.rand.NextFloat(1.25f, 2f) * 2f)];
                    obj.position = Vector2.Lerp(vector7, vector6, num6 / (float)num5) + new Vector2(Projectile.width, Projectile.height) / 2f;
                    obj.position += Main.rand.RandomPointInArea(2f);
                    obj.noGravity = true;
                    obj.noLightEmittence = true;
                    obj.velocity *= 0.1f;
                    obj.velocity += Projectile.velocity * 0.5f;
                    obj.scale *= 0.375f;
                    Dust obj2 = obj;
                    obj2.position.X -= 2f;
                    Dust obj3 = obj;
                    obj3.position.Y += 2f;
                    Dust obj4 = obj;
                    obj4.velocity.Y -= 2f;
                }
            }
        }
        void cutTiles() {
            if (!ShouldSummonTentaclesDealDamage) {
                return;
            }

            for (int i = SummonMouthCount; i < SummonMouthCount + SUMMONTENTACLECOUNT; i++) {
                ref SummonTentacleInfo summonTentacleInfo = ref _summonTentacleData[i - SummonMouthCount];
                int summonTentacleSegmentCount = summonTentacleInfo.SegmentCount - 2;
                for (int i2 = 0; i2 < summonTentacleSegmentCount; i2++) {
                    Vector2 summongTentaclePosition = summonTentacleInfo.SegmentPositions[i2];
                    ProjectileUtils.CutTilesAt(Projectile, summongTentaclePosition, SUMMONTENTACLEHITBOXSIZE, SUMMONTENTACLEHITBOXSIZE);
                }
            }
        }

        scaleUp();
        init();
        levitate();
        enrage();
        animateBulb();
        processSummonMise();
        processSummonTentacles();
        resetDamageInfo();
        damageNPCs();
        processEnergyParticles();
        cutTiles();
    }

    public override void OnKill(int timeLeft) {

    }

    protected override void Draw(ref Color lightColor) {
        if (!Init) {
            return;
        }

        if (!AssetInitializer.TryGetRequestedTextureAssets<Bulb>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        float randomnessSeed = _seedForRandomness;

        float appearanceFactor = Ease.CubeOut(AppearanceValue),
              dissappearanceFactor = MathUtils.Clamp01(Projectile.timeLeft / (float)DISSAPEARTSTARTTIMELEFT);

        Vector2 center = Vector2.Lerp(RootPosition, Projectile.Center, appearanceFactor);
        float seedForAnimation = _seedForRandomness;

        SpriteBatch batch = Main.spriteBatch;

        Texture2D bulbTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb].Value,
                  bulbTexture_Back = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb_Back].Value,
                  bulb2Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb2].Value,
                  bulb2Texture_Back = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb2_Back].Value,
                  stem1Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem].Value,
                  stem2Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem2].Value,
                  stem3Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem3].Value,
                  leafStemTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.LeafStem].Value,
                  leafTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Leaf].Value,
                  stamenTexture_Yellow = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stamen_Yellow].Value,
                  stamenTexture_Green = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stamen_Green].Value,
                  summonMouthTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.SummonMouth].Value,
                  summonTentacleTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.SummonTentacle].Value,
                  summonTentacle2Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.SummonTentacle2].Value;

        int texturePadding = 4;
        float plantScaleFactor = Projectile.scale;
        plantScaleFactor *= appearanceFactor;
        plantScaleFactor = MathF.Max(0.1f, plantScaleFactor);
        Vector2 plantScale = Vector2.One * plantScaleFactor * Ease.QuartOut(appearanceFactor);
        plantScale.Y *= Utils.Remap(appearanceFactor * Ease.CubeOut(appearanceFactor), 0f, 1f, 2f, 1f);
        plantScale.X *= Utils.Remap(appearanceFactor * Ease.CubeIn(appearanceFactor), 0f, 1f, 0.875f, 1f);

        int bulbFrame = Projectile.frame,
            bulbFrameColumn = ShouldTransform.ToInt();

        float bulbScaleModifier = 1f;

        void getBulbPosition(out Vector2 bulbPosition_Origin, out Vector2 bulbPosition_Result) {
            Vector2 bulbOffset = new(0f, 14f);
            Vector2 bulbPosition = center;

            bulbPosition_Origin = bulbPosition;

            bulbPosition += bulbOffset;

            bulbPosition_Result = bulbPosition;
        }

        getBulbPosition(out _, out Vector2 bulbPosition_Result_ForLighting);
        bulbPosition_Result_ForLighting.Y -= 30f;
        Color baseColor = Lighting.GetColor(bulbPosition_Result_ForLighting.ToTileCoordinates()) * Utils.GetLerpValue(0f, 0.625f, appearanceFactor, true);

        // BULB
        Rectangle bulbClip = Utils.Frame(bulbTexture, BULBFRAMECOUNT_COLUMN, BULBFRAMECOUNT_ROW, frameX: bulbFrameColumn, frameY: bulbFrame);
        Vector2 bulbOrigin = bulbClip.BottomCenter();
        DrawInfo bulbDrawInfo = new() {
            Clip = bulbClip,
            Origin = bulbOrigin,

            Scale = plantScale * bulbScaleModifier,

            Color = baseColor
        };

        // BULB 2
        Rectangle bulb2Clip = Utils.Frame(bulb2Texture, 1, BULB2FRAMECOUNT_ROW, frameY: bulbFrame);
        Vector2 bulb2Origin = bulb2Clip.BottomCenter();
        DrawInfo bulb2DrawInfo = new() {
            Clip = bulb2Clip,
            Origin = bulb2Origin,

            Scale = plantScale * bulbScaleModifier,

            Color = baseColor
        };

        // STEM 1
        Rectangle stem1Clip = stem1Texture.Bounds;
        Vector2 stem1Origin = stem1Clip.BottomCenter();
        DrawInfo stem1DrawInfo = new() {
            Clip = stem1Clip,
            Origin = stem1Origin,

            Scale = plantScale,

            Color = baseColor
        };

        // STEM 2
        Rectangle stem2Clip = stem2Texture.Bounds;
        Vector2 stem2Origin = stem2Clip.BottomCenter();
        Vector2 stem2Scale = plantScale;
        stem2Scale.X *= Ease.CubeInOut(appearanceFactor);
        DrawInfo stem2DrawInfo = new() {
            Clip = stem2Clip,
            Origin = stem2Origin,

            Scale = stem2Scale,

            Color = baseColor
        };

        // STEM 3
        Rectangle stem3Clip = stem3Texture.Bounds;
        Vector2 stem3Origin = stem3Clip.BottomCenter();
        Vector2 stem3Scale = plantScale;
        stem3Scale.X *= Ease.CubeInOut(appearanceFactor);
        DrawInfo stem3DrawInfo = new() {
            Clip = stem3Clip,
            Origin = stem3Origin,

            Scale = stem3Scale,

            Color = baseColor
        };

        Vector2 leafPartScale = Vector2.One;
        leafPartScale.X = Ease.CubeOut(appearanceFactor);
        leafPartScale.Y = Ease.CubeOut(appearanceFactor);

        // LEAF STEM
        Rectangle leafStemClip = leafStemTexture.Bounds;
        Vector2 leafStemOrigin = leafStemClip.BottomCenter();
        DrawInfo leafStemDrawInfo = new() {
            Clip = leafStemClip,
            Origin = leafStemOrigin,

            Scale = leafPartScale,

            Color = baseColor
        };

        // LEAF
        Rectangle getLeafClip(int frameY = 0) => Utils.Frame(leafTexture, 1, LEAFFRAMECOUNT, frameY: frameY);
        Rectangle leafClip = getLeafClip();
        Vector2 leafOrigin = leafStemClip.BottomLeft();
        DrawInfo leafDrawInfo = new() {
            Clip = leafClip,
            Origin = leafOrigin,

            Scale = leafPartScale,

            Color = baseColor
        };

        // STAMEN YELLOW
        Rectangle getStamenClip_Yellow(int frameY = 0) => Utils.Frame(stamenTexture_Yellow, 1, STAMENFRAMECOUNT_YELLOW, frameY: frameY);
        Rectangle stamenClip_Yellow = getStamenClip_Yellow();
        Vector2 stamenOrigin_Yellow = stamenClip_Yellow.Centered();
        DrawInfo stamenDrawInfo_Yellow = new() {
            Clip = stamenClip_Yellow,
            Origin = stamenOrigin_Yellow,

            Scale = leafPartScale,

            Color = baseColor
        };

        // STAMEN GREEN
        Rectangle getStamenClip_Green(int frameY = 0) => Utils.Frame(stamenTexture_Green, 1, STAMENFRAMECOUNT_GREEN, frameY: frameY);
        Rectangle stamenClip_Green = getStamenClip_Green();
        Vector2 stamenOrigin_Green = stamenClip_Green.Centered();
        DrawInfo stamenDrawInfo_Green = new() {
            Clip = stamenClip_Green,
            Origin = stamenOrigin_Green,

            Scale = leafPartScale,

            Color = baseColor
        };

        // SUMMON MOUTH
        Rectangle getSummonMouthClip(int frameY = 0) => Utils.Frame(summonMouthTexture, 1, SUMMONMOUTHFRAMECOUNT, frameY: frameY);
        Rectangle summonMouthClip = getSummonMouthClip();
        Vector2 summonMouthOrigin = summonMouthClip.Centered();
        DrawInfo summonMouthDrawInfo = new() {
            Clip = summonMouthClip,
            Origin = summonMouthOrigin,

            Scale = plantScale,

            Color = baseColor
        };

        // SUMMON TENTACLE
        Rectangle summonTentacleClip = summonTentacleTexture.Bounds;
        Vector2 summonTentacleOrigin = summonTentacleClip.BottomCenter();
        DrawInfo summonTentacleDrawInfo = new() {
            Clip = summonTentacleClip,
            Origin = summonTentacleOrigin,

            Scale = plantScale,

            Color = baseColor
        };

        // SUMMON TENTACLE 2
        Rectangle summonTentacle2Clip = summonTentacle2Texture.Bounds;
        Vector2 summonTentacle2Origin = summonTentacle2Clip.BottomCenter();
        DrawInfo summonTentacle2DrawInfo = new() {
            Clip = summonTentacle2Clip,
            Origin = summonTentacle2Origin,

            Scale = plantScale,

            Color = baseColor
        };

        void drawMainStem() {
            float scaleFactor2 = plantScaleFactor;
            scaleFactor2 = MathF.Min(0.99f, scaleFactor2);
            int stem1Height = stem1Clip.Height - texturePadding;
            stem1Height = (int)(stem1Height * scaleFactor2);
            int stem3Height = stem3Clip.Height - texturePadding;
            stem3Height = (int)(stem3Height * scaleFactor2);
            Vector2 startPosition = RootPosition;
            Vector2 endPosition = center + center.DirectionFrom(startPosition) * stem1Height * appearanceFactor;
            endPosition = Vector2.Lerp(endPosition, RootPosition, Ease.SineIn(1f - appearanceFactor));
            //float scaleFactor = 1f;
            float getDistanceToBulb() => Vector2.Distance(startPosition, endPosition);
            bool isValidToContinue() => getDistanceToBulb() > stem1Height * 2;
            bool justStarted = true;
            while (isValidToContinue()) {
                //float lerpValue = 0.25f;
                //scaleFactor = Helper.Approach(scaleFactor, stem1Height, lerpValue);
                float step = stem1Height;

                //Vector2 scale = Vector2.One * (0.25f + Utils.GetLerpValue(0f, stem1Height, scaleFactor, true));
                //float sizeIncreaseFluff = 3f;
                //if (getDistanceToBulb() < stem1Height * sizeIncreaseFluff) {
                //    scale = Helper.Approach(scale, new Vector2(10f), lerpValue);
                //}

                Vector2 position = startPosition + new Vector2(1f, -3f);
                if (justStarted) {
                    step = stem3Height;

                    batch.Draw(stem3Texture, position, stem3DrawInfo);
                }
                else {
                    batch.Draw(stem1Texture, position, stem1DrawInfo);
                }

                Vector2 velocityToBulbPosition = startPosition.DirectionTo(endPosition);
                startPosition += velocityToBulbPosition * step;

                justStarted = false;
            }
        }
        void drawBulb_Back() {
            getBulbPosition(out _, out Vector2 bulbPosition_Result);

            if (!IsSecondFormActive) {
                batch.Draw(bulbTexture_Back, bulbPosition_Result, bulbDrawInfo);
            }
            else {
                batch.Draw(bulb2Texture_Back, bulbPosition_Result, bulb2DrawInfo);
            }

            //batch.Draw(stem3Texture, stem3Position, stem3DrawInfo);
        }
        void drawBulb() {
            getBulbPosition(out Vector2 bulbPosition_Origin, out Vector2 bulbPosition_Result);

            float stem2OffsetFromBulbValue = 32f * plantScaleFactor;
            Vector2 angleFromBulbToRoot = bulbPosition_Origin.DirectionTo(RootPosition);
            Vector2 stem2Position = bulbPosition_Origin + angleFromBulbToRoot * stem2OffsetFromBulbValue;
            stem2Position = Vector2.Lerp(stem2Position, RootPosition, Ease.SineIn(1f - appearanceFactor));

            float stem3OffsetFromBulbValue = 4f * plantScaleFactor;
            Vector2 stem3Position = bulbPosition_Origin + angleFromBulbToRoot * stem3OffsetFromBulbValue;
            stem3Position = Vector2.Lerp(stem3Position, RootPosition, 1f - appearanceFactor);

            batch.Draw(stem2Texture, stem2Position, stem2DrawInfo);

            if (!IsSecondFormActive) {
                batch.Draw(bulbTexture, bulbPosition_Result, bulbDrawInfo);
            }
            else {
                batch.Draw(bulb2Texture, bulbPosition_Result, bulb2DrawInfo);
            }

            //batch.Draw(stem3Texture, stem3Position, stem3DrawInfo);
        }
        const int STAMENCOUNTINSTEM = 3;
        int generalCurrentStamenIndex = 0,
            generalLeafStemIndex = 0;
        float transformFactorForScale = Utils.GetLerpValue(0f, 0.05f, TransformFactor, true);
        int leafFrame = 0;
        float plantScaleFactor2 = Ease.QuadIn(plantScaleFactor);
        void drawLeafStem(Vector2 startVelocity, bool stamenStem = false) {
            int leafStem1Height = leafStemClip.Height - texturePadding;
            leafStem1Height = (int)(leafStem1Height * 0.95f);

            int currentStamenIndex = 0;
            void drawStamen(Vector2 startPosition, int direction, float startRotation = 0f) {
                startRotation += MathHelper.Pi;

                int baseStamenHeight = stamenClip_Green.Height - texturePadding,
                    currentStamenHeight = baseStamenHeight;

                float currentLength = 3f;
                Vector2 position = startPosition;
                float currentRotation = startRotation;
                float scaleFactor = 0.75f,
                      scaleFactorLerpValue = 0.1f;
                while (currentLength > 0f) {
                    switch (currentLength) {
                        case 3f:
                            currentStamenHeight = (int)(baseStamenHeight * 0.55f);
                            break;
                        case 2f:
                            currentStamenHeight = (int)(baseStamenHeight * 0.55f);
                            break;
                        case 1f:
                            currentStamenHeight = (int)(baseStamenHeight * 0.7f);
                            break;
                    }

                    //uint seed = (uint)(randomnessSeed * (generalCurrentStamenIndex + currentStamenIndex) * 15);
                    //ulong seed_ulong = (ulong)seed;

                    //float sineOffset = 0f;
                    //switch (currentStamenIndex) {
                    //    case 1:
                    //        sineOffset = 2f;
                    //        break;
                    //    case 2:
                    //        sineOffset = 0f;
                    //        break;
                    //}
                    //sineOffset += MathUtils.PseudoRandRange(ref seed, -0.5f, 0.5f);

                    bool lastRight = direction > 0 && currentLength == 1f;

                    float baseStep = currentStamenHeight,
                          step = baseStep * scaleFactor * TransformFactor;

                    scaleFactor = Helper.Approach(scaleFactor, 1f, scaleFactorLerpValue);

                    currentRotation += -MathF.Sin(currentLength * 7.5f) * 0.5f * direction;

                    SpriteEffects flip = SpriteEffects.None;
                    if (lastRight) {
                        flip = SpriteEffects.FlipHorizontally;
                    }
                    Vector2 stamenPosition = position;
                    stamenPosition += plantScaleFactor2 * - Vector2.UnitY.RotatedBy(currentRotation) * baseStep;
                    //stamenPosition += Vector2.UnitX.RotatedBy(currentRotation) * 2f * direction;
                    int stamenFrame = (int)currentLength - 1;
                    batch.Draw(stamenTexture_Green, stamenPosition, stamenDrawInfo_Green.WithScale(scaleFactor * transformFactorForScale) with {
                        Clip = getStamenClip_Green(stamenFrame),
                        Rotation = currentRotation,
                        ImageFlip = flip
                    });
                    int activationSlotIndex = stamenFrame + 3 * generalCurrentStamenIndex;
                    activationSlotIndex = STAMENACTIVATIONSLOTCOUNT - 1 - activationSlotIndex;
                    float activationProgress = _stamenActivated[activationSlotIndex].Progress;
                    batch.Draw(stamenTexture_Yellow, stamenPosition, stamenDrawInfo_Yellow.WithColorModifier(activationProgress).WithScale(scaleFactor * transformFactorForScale) with {
                        Clip = getStamenClip_Yellow(stamenFrame),
                        Rotation = currentRotation,
                        ImageFlip = flip
                    });

                    //bool shouldDrawStamen = currentLength <= 1f;
                    //if (shouldDrawStamen) {
                    //    Vector2 stamenPosition = position;
                    //    stamenPosition += -Vector2.UnitY.RotatedBy(currentRotation) * baseStep;

                    //    batch.Draw(stamenTexture_Green, stamenPosition, stamenDrawInfo_Green.WithScale(transformFactorForScale));
                    //    batch.Draw(stamenTexture_Yellow, stamenPosition, stamenDrawInfo_Yellow.WithScale(transformFactorForScale));
                    //}

                    Vector2 velocity = Vector2.UnitY.RotatedBy(currentRotation);
                    position += -velocity * step;

                    currentLength = Helper.Approach(currentLength, 0f, 1f);
                }

                generalCurrentStamenIndex++;

                currentStamenIndex++;
                if (currentStamenIndex > STAMENCOUNTINSTEM) {
                    currentStamenIndex = 0;
                }
            }

            Vector2 startPosition = center;

            //startPosition.Y += 26f * (1f - appearanceFactor);

            float startPositionOffsetFactor = 2.5f * plantScaleFactor;
            startPosition += startVelocity * startPositionOffsetFactor;

            int direction = startVelocity.X.GetDirection();
            SpriteEffects flip = direction.ToSpriteEffects();
            bool facedRight = direction > 0;

            float currentLength = stamenStem ? 7f : 9f,
                  length = currentLength;
            float xLerpValue = 0.1f,
                  yLerpValue = 0f;
            float scaleFactor = 0.5f;
            float leafStemScaleLerpValue = 0.025f;
            while (currentLength > 0f) {
                scaleFactor = Helper.Approach(scaleFactor, 1f, leafStemScaleLerpValue);

                leafStemScaleLerpValue = Helper.Approach(leafStemScaleLerpValue, 0.1f, TimeSystem.LogicDeltaTime);

                float baseStep = leafStem1Height * 1.1f,
                      step = baseStep * scaleFactor * TransformFactor;

                step *= plantScaleFactor2;

                //step *= 0.75f;

                float lengthProgress = 1f - currentLength / length;

                float rotation = startVelocity.ToRotation() - MathHelper.PiOver2;

                float yLerpValueTo = 1f;
                float yLerpValueLerpValue = 0.5f;
                yLerpValue = Helper.Approach(yLerpValue, yLerpValueTo, yLerpValueLerpValue);

                ref float velocityX = ref startVelocity.X,
                          velocityY = ref startVelocity.Y;
                float xTo = 0.25f,
                      yTo = -5f;
                bool justStarted = lengthProgress > 0.375f,
                     justStarted2 = lengthProgress > 0.25f,
                     shouldGoDown = lengthProgress > 0.5f;
                if (stamenStem) {
                    xLerpValue = 0.05f;
                    if (justStarted2) {
                        yTo = -20f;
                        yLerpValue = 3f;
                    }
                    else {
                        yTo = -(2f + 10f * Ease.CubeIn(Utils.GetLerpValue(0f, 0.375f, lengthProgress, true)));
                    }
                    if (shouldGoDown) {
                        yTo = 5f;
                        yLerpValue = 2f;
                    }
                }
                else {
                    if (justStarted) {
                        yTo = -5f;
                    }
                    if (shouldGoDown) {
                        yTo = 5f;
                        yLerpValue = 2f;
                    }
                }

                float waveFrequency = 0.5f;
                uint seed = (uint)(randomnessSeed * (generalLeafStemIndex + 1) * 15);
                ulong seed_ulong = (ulong)seed;
                float seedForAnimation_LeafStem = seedForAnimation + MathUtils.PseudoRandRange(ref seed, -1f, 1f);
                xLerpValue *= Helper.Wave(0.75f, 1f, waveFrequency, seedForAnimation_LeafStem);
                yLerpValue *= Helper.Wave(0.5f, 1f, 2f, seedForAnimation_LeafStem);

                velocityX = Helper.Approach(velocityX, xTo, xLerpValue);
                velocityY = Helper.Approach(velocityY, yTo, yLerpValue);

                bool shouldDrawLeaf_Inner = currentLength <= 1f;
                bool shouldDrawStamen_Inner1 = currentLength == 2f,
                     shouldDrawStamen_Inner2 = currentLength == 3f;

                bool shouldDrawLeaf = !stamenStem && shouldDrawLeaf_Inner,
                     shouldDrawStamen = stamenStem && (shouldDrawStamen_Inner1 || shouldDrawStamen_Inner2);
                Vector2 position = startPosition;

                void drawStem(Vector2 position) {
                    batch.Draw(leafStemTexture, position, leafStemDrawInfo.WithScale(scaleFactor * transformFactorForScale) with {
                        Rotation = rotation,
                        ImageFlip = flip
                    });
                }

                if (shouldDrawStamen) {
                    drawStem(position);

                    if (shouldDrawStamen_Inner1) {
                        drawStamen(position, direction, rotation);
                    }

                    //if (shouldDrawStamen_Inner1) {
                    //    float stamenStemStartRotation = MathHelper.PiOver2 * 1f * direction;
                    //    drawStamen(position, direction, stamenStemStartRotation);
                    //    stamenStemStartRotation = MathHelper.PiOver2 * 0.375f * direction;
                    //    drawStamen(position, direction, stamenStemStartRotation);
                    //}
                    //if (shouldDrawStamen_Inner2) {
                    //    float stamenStemStartRotation = 0f;
                    //    drawStamen(position, direction, stamenStemStartRotation);
                    //}
                }
                else if (shouldDrawLeaf) {
                    float leafRotation = rotation + MathHelper.PiOver2 * direction;
                    if (leafFrame == 0) {
                        leafRotation += MathHelper.PiOver4 * direction * 0.5f;
                    }
                    Vector2 leafOffset = -Vector2.UnitX.RotatedBy(leafRotation) * baseStep * 1.9f;
                    position += leafOffset;
                    Vector2 leafOffset2 = -Vector2.UnitY.RotatedBy(leafRotation) * 2f;
                    position += leafOffset2;
                    batch.Draw(leafTexture, position, leafDrawInfo.WithScale(transformFactorForScale * 0.9f) with {
                        Clip = getLeafClip(leafFrame),
                        Rotation = leafRotation,
                        ImageFlip = flip
                    });
                    leafFrame++;
                }
                else {
                    drawStem(position);
                }

                currentLength = Helper.Approach(currentLength, 0f, 1f);

                startVelocity.Y *= TransformFactor;

                Vector2 velocityToLeafPosition = startVelocity.SafeNormalize();
                startPosition += velocityToLeafPosition * step;
            }

            generalLeafStemIndex++;
        }
        void drawLeafStems() {
            float startXVelocity = 6f;
            drawLeafStem(new Vector2(startXVelocity, 0f));
            drawLeafStem(new Vector2(-startXVelocity, 0f));
            drawLeafStem(new Vector2(startXVelocity, -2.5f), true);
            drawLeafStem(new Vector2(-startXVelocity, -2.5f), true);
        }
        void drawSummonMise() {
            if (!IsSecondFormActive) {
                return;
            }

            int currentSummonMouthIndex = 0;
            foreach (SummonMouthInfo summonMouthInfo in _summonMouthData) {
                currentSummonMouthIndex++;

                Vector2 position = Vector2.Lerp(summonMouthInfo.Position, center, 1f - plantScaleFactor2);
                float rotation = summonMouthInfo.Rotation;
                SpriteEffects flip = summonMouthInfo.FacedRight.ToSpriteEffects2();
                if (!summonMouthInfo.FacedRight) {
                    flip = SpriteEffects.None;
                }
                else {
                    flip |= SpriteEffects.FlipVertically;
                }

                int stemHeight = leafStemClip.Height - texturePadding;
                stemHeight = (int)(stemHeight * plantScaleFactor * 0.95f);
                Vector2 stemStartPosition = position,
                        stemEndPosition = center;
                float stemScaleFactor = 1.5f;
                int i = 0;
                float maxOffsetX = 50f;
                while (plantScaleFactor2 > 0.1f) {
                    i++;

                    float lerpValue = 0.15f;
                    stemScaleFactor = Helper.Approach(stemScaleFactor, 0.75f, lerpValue);

                    float step = stemHeight * stemScaleFactor;

                    Vector2 stemPosition = stemStartPosition;

                    Vector2 velocityToSummonMouthPosition = stemStartPosition.DirectionTo(stemEndPosition);
                    velocityToSummonMouthPosition = velocityToSummonMouthPosition.RotatedBy(Math.Sin(i + i * maxOffsetX + stemPosition.Length() / 64f) * 0.25f);

                    float stemRotation = velocityToSummonMouthPosition.ToRotation() + MathHelper.PiOver2;

                    if (Vector2.Distance(stemStartPosition, stemEndPosition) < step) {
                        break;
                    }

                    batch.Draw(leafStemTexture, stemPosition, leafStemDrawInfo.WithScale(stemScaleFactor) with {
                        Rotation = stemRotation
                    });

                    stemStartPosition += velocityToSummonMouthPosition * step;
                }

                int summonMouthFrame = (int)((TimeSystem.TimeForVisualEffects * 5 + currentSummonMouthIndex) % 2);
                batch.Draw(summonMouthTexture, position, summonMouthDrawInfo with {
                    Clip = getSummonMouthClip(summonMouthFrame),
                    Rotation = rotation,
                    ImageFlip = flip
                });
            }
        }
        void drawSummonTentacles() {
            if (!(ShouldTransform || IsSecondFormActive)) {
                return;
            }
            if (appearanceFactor < 0.5f) {
                return;
            }
            foreach (SummonTentacleInfo summonTentacleInfo in _summonTentacleData) {
                int segmentCount = summonTentacleInfo.SegmentCount;
                segmentCount -= 2;
                for (int i = 0; i < segmentCount; i++) {
                    bool last = i >= segmentCount - 2;
                    int nextIndex = Math.Min(segmentCount - 1, i + 1);
                    Vector2 position = summonTentacleInfo.SegmentPositions[i];
                    position = Vector2.Lerp(position, center, TransformFactor);

                    Vector2 nextPosition = summonTentacleInfo.SegmentPositions[nextIndex];
                    nextPosition = Vector2.Lerp(nextPosition, center, TransformFactor);
                    float rotation = summonTentacleInfo.SegmentRotations[i] + MathHelper.PiOver2;
                    float distanceToNext = position.Distance(nextPosition);
                    float scaleY = distanceToNext * 0.121f;
                    scaleY = MathF.Max(scaleY, 1f);
                    if (last) {
                        float step = summonTentacle2DrawInfo.Clip.Height * 0.4f * scaleY;
                        Vector2 endOffset = Vector2.UnitY.RotatedBy(rotation) * step;
                        position += endOffset;
                        batch.Draw(summonTentacle2Texture, position, summonTentacle2DrawInfo.WithScaleY(scaleY) with {
                            Rotation = rotation
                        });

                        break;
                    }
                    batch.Draw(summonTentacleTexture, position, summonTentacleDrawInfo.WithScaleY(scaleY) with {
                        Rotation = rotation
                    });
                }
            }
        }

        drawLeafStems();

        drawBulb_Back();

        drawSummonTentacles();
        drawSummonMise();

        drawMainStem();
        drawBulb();
    }

    private float AcceptedEnoughDamageProgress() {
        float result = 0f;
        result = _stamenActivated.Sum(checkStamenDamageDone => checkStamenDamageDone.Progress) / (float)STAMENACTIVATIONSLOTCOUNT;
        return result;
    }

    private bool AcceptedEnoughDamage() {
        bool result = false;
        result = _stamenActivated.Where(checkStamenDamageDone => checkStamenDamageDone.Progress >= 1f).Count() >= STAMENACTIVATIONSLOTCOUNT;
        return result;
    }

    public void AcceptDamage(Vector2 damageSourcePosition, int damageDone) {
        if (Projectile.Center.Distance(damageSourcePosition) > ACCEPTDAMAGEDISTANCE) {
            return;
        }

        SpawnEnergyParticle(damageSourcePosition);

        int stamenToAcceptDamageIndex = 0;
        while (_stamenActivated[stamenToAcceptDamageIndex].Progress >= 1f) {
            stamenToAcceptDamageIndex++;
            int lastIndex = STAMENACTIVATIONSLOTCOUNT - 1;
            if (stamenToAcceptDamageIndex > lastIndex) {
                stamenToAcceptDamageIndex = lastIndex;
                break;
            }
        }
        _stamenActivated[stamenToAcceptDamageIndex].DamageDone += damageDone;
    }
}
