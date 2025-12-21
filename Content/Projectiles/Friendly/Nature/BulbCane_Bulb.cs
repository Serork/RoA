using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class Bulb : NatureProjectile_NoTextureLoad, IRequestAssets {
    private class Bulb_DamageCounter : GlobalProjectile {
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
            int finalDamage = hit.Damage;
            foreach (Projectile bulbProjectile in TrackedEntitiesSystem.GetTrackedProjectile<Bulb>(checkProjectile => !checkProjectile.SameOwnerAs(projectile))) {
                bulbProjectile.As<Bulb>().AcceptDamage(finalDamage);
            }
        }
    }

    private static ushort TIMELEFT => MathUtils.SecondsToFrames(600);

    private static byte BULBFRAMECOUNT_ROW => 7;
    private static byte BULBFRAMECOUNT_COLUMN => 2;
    private static byte BULBTRANSFORMRAMECOUNT_ROW => 4;
    private static byte BULB2FRAMECOUNT_ROW => 5;

    private static byte LEAFFRAMECOUNT => 2;
    private static byte STAMENFRAMECOUNT_YELLOW => 4;
    private static byte STAMENFRAMECOUNT_GREEN => 4;

    private static float ROOTLENGTH => 150f;
    private static byte SUMMONMOUTHCOUNT => 6;
    private static byte SUMMONTENTACLECOUNT => 3;
    private static byte STAMENACTIVATIONSLOTCOUNT => 6;
    private static ushort DAMAGENEEDEDPERSTAMEN => 100;

    public enum Bulb_RequstedTextureType : byte { 
        Bulb,
        Bulb_Back,
        Bulb2,
        Bulb2_Back,
        Stem1,
        Stem2,
        LeafStem1,
        Leaf,
        Stamen_Yellow,
        Stamen_Green,
        SummonMouth,
        SummonTentacle
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)Bulb_RequstedTextureType.Bulb, ResourceManager.NatureProjectileTextures + "Bulb"),
         ((byte)Bulb_RequstedTextureType.Bulb_Back, ResourceManager.NatureProjectileTextures + "Bulb_Back"),
         ((byte)Bulb_RequstedTextureType.Bulb2, ResourceManager.NatureProjectileTextures + "Bulb2"),
         ((byte)Bulb_RequstedTextureType.Bulb2_Back, ResourceManager.NatureProjectileTextures + "Bulb2_Back"),
         ((byte)Bulb_RequstedTextureType.Stem1, ResourceManager.NatureProjectileTextures + "Bulb_Stem1"),
         ((byte)Bulb_RequstedTextureType.Stem2, ResourceManager.NatureProjectileTextures + "Bulb_Stem2"),
         ((byte)Bulb_RequstedTextureType.LeafStem1, ResourceManager.NatureProjectileTextures + "Bulb_LeafStem1"),
         ((byte)Bulb_RequstedTextureType.Leaf, ResourceManager.NatureProjectileTextures + "Bulb_Leaf"),
         ((byte)Bulb_RequstedTextureType.Stamen_Yellow, ResourceManager.NatureProjectileTextures + "Bulb_Stamen_Yellow"),
         ((byte)Bulb_RequstedTextureType.Stamen_Green, ResourceManager.NatureProjectileTextures + "Bulb_Stamen_Green"),
         ((byte)Bulb_RequstedTextureType.SummonMouth, ResourceManager.NatureProjectileTextures + "Bulb_SummonMouth"),
         ((byte)Bulb_RequstedTextureType.SummonTentacle, ResourceManager.NatureProjectileTextures + "Bulb_SummonTentacle")];

    public record struct SummonMouthInfo(Vector2 Position, Vector2 Destination, Vector2 Velocity, float Rotation, float BaseRotation, bool FacedRight);
    public record struct SummonTentacleInfo(Vector2 RootPosition, Vector2 Destination, Vector2[] Position, float[] Rotation, float[] Scale, float WaveFrequency);
    public record struct StamenDamageInfo(int DamageDone, int DamageNeeded) {
        public readonly float Progress => MathUtils.Clamp01((float)DamageDone / DamageNeeded);
        public readonly bool Activated => Progress >= 1f;
    }

    private SummonMouthInfo[] _summonMouthData = null!;
    private SummonTentacleInfo[] _summonTentacleData = null!;
    private StamenDamageInfo[] _stamenActivated = null!;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float ShouldTransformValue => ref Projectile.localAI[1];
    public ref float TransformFactorValue => ref Projectile.localAI[2];
    public ref float RootPositionX => ref Projectile.ai[0];
    public ref float RootPositionY => ref Projectile.ai[1];
    public ref float SecondFormValue => ref Projectile.ai[2];

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

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.penetrate = -1;
        Projectile.friendly = true;

        Projectile.timeLeft = TIMELEFT;

        Projectile.tileCollide = false;

        Projectile.manualDirectionChange = false;
    }

    public override void AI() {
        Vector2 center = Projectile.Center;
        float tentacleSegmentHeight = 9;

        void init() {
            if (!Init) {
                float yOffset = ROOTLENGTH;
                RootPosition = Projectile.Center + Vector2.UnitY * yOffset;

                TransformFactorValue = 1f;
                

                void initStamens() {
                    _stamenActivated = new StamenDamageInfo[STAMENACTIVATIONSLOTCOUNT];
                    for (int i = 0; i < _stamenActivated.Length; i++) {
                        _stamenActivated[i] = new StamenDamageInfo(0, DAMAGENEEDEDPERSTAMEN);
                    }
                }
                void initSummonMise() {
                    _summonMouthData = new SummonMouthInfo[SUMMONMOUTHCOUNT];
                    int summonMouthCount = _summonMouthData.Length,
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
                void iniSummonTentacles() {
                    _summonTentacleData = new SummonTentacleInfo[SUMMONTENTACLECOUNT];
                    int summonTentacleCount = _summonTentacleData.Length;
                    for (int i = 0; i < summonTentacleCount; i++) {
                        bool first = i == 0;
                        int nextIndex = i + 1;
                        ref SummonTentacleInfo summonTentacleInfo = ref _summonTentacleData[i];
                        int segmentCount = 10 + 3 * nextIndex;
                        float rootXOffset = 5f,
                              xOffset = (i % 2 == 0).ToDirectionInt() * (first ? 0f : rootXOffset),
                              yOffset = -tentacleSegmentHeight * segmentCount;
                        summonTentacleInfo.RootPosition = center + Vector2.UnitX * xOffset;
                        Vector2 destination = center + new Vector2(xOffset, yOffset);
                        summonTentacleInfo.Destination = destination;
                        summonTentacleInfo.Position = new Vector2[segmentCount];
                        summonTentacleInfo.Rotation = new float[segmentCount];
                        summonTentacleInfo.WaveFrequency = Main.rand.NextFloat(3f, 5f);
                        summonTentacleInfo.Scale = new float[segmentCount];
                        for (int i2 = 0; i2 < segmentCount; i2++) {
                            summonTentacleInfo.Position[i] = center;
                            summonTentacleInfo.Scale[i] = 1f;
                        }
                    }
                }
                initStamens();
                initSummonMise();
                iniSummonTentacles();

                Init = true;
            }
        }
        void scaleUp() {
            Projectile.scale = 1.5f;
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
        void processSummonMise() {
            if (!IsSecondFormActive) {
                return;
            }

            int summonMouthCount = _summonMouthData.Length;
            for (int i = 0; i < summonMouthCount; i++) {
                ref SummonMouthInfo summonMouthInfo = ref _summonMouthData[i];
                float baseRotation = summonMouthInfo.BaseRotation;
                float offsetValue = 125f;
                float centerYOffset = 50f;
                Vector2 destination = center - Vector2.UnitY * centerYOffset + Vector2.UnitY.RotatedBy(baseRotation) * offsetValue + summonMouthInfo.Destination;
                float minDistance = 60f;
                if (Vector2.Distance(destination, summonMouthInfo.Position) < minDistance) {
                    float maxOffsetValue = 50f;
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
            }
        }
        void processSummonTentacles() {
            //if (!IsSecondFormActive) {
            //    return;
            //}

            int summonTentacleCount = _summonTentacleData.Length;
            for (int i = 0; i < summonTentacleCount; i++) {
                ref SummonTentacleInfo summonTentacleInfo = ref _summonTentacleData[i];
                int segmentCount = summonTentacleInfo.Position.Length;

                Vector2 rootPosition = summonTentacleInfo.RootPosition,
                        mainDestination = rootPosition,
                        finalDestination = summonTentacleInfo.Destination;

                for (int i2 = 0; i2 < segmentCount; i2++) {
                    float progress = i2 / (float)segmentCount;
                    float scale = 1f - 0.5f * Utils.GetLerpValue(0.7f, 1f, progress, true);
                    summonTentacleInfo.Scale[i2] = scale;
                }

                for (int i2 = 0; i2 < segmentCount; i2++) {
                    float scale = summonTentacleInfo.Scale[i2];
                    Vector2 step = rootPosition.DirectionTo(finalDestination) * tentacleSegmentHeight * scale;
                    mainDestination += step;
                }

                float maxXOffset = 40f;
                float waveSinOffset = i * maxXOffset,
                      waveFrequency = summonTentacleInfo.WaveFrequency;
                float waveOffset = Helper.Wave(-maxXOffset, maxXOffset, waveFrequency, waveSinOffset);
                waveOffset *= 1f - TransformFactor;
                mainDestination.X += waveOffset;

                for (int i2 = 0; i2 < segmentCount; i2++) {
                    int currentSegmentIndex = i2,
                        previousSegmentIndex = Math.Max(0, currentSegmentIndex - 1),
                        nextSegmentIndex = Math.Min(segmentCount - 1, currentSegmentIndex + 1);
                    ref Vector2 currentSegmentPosition = ref summonTentacleInfo.Position[currentSegmentIndex],
                                previousSegmentPosition = ref summonTentacleInfo.Position[previousSegmentIndex],
                                nextSegmentPosition = ref summonTentacleInfo.Position[nextSegmentIndex];

                    bool first = currentSegmentIndex == 0,
                         last = currentSegmentIndex == nextSegmentIndex;

                    ref float currentSegmentRotation = ref summonTentacleInfo.Rotation[currentSegmentIndex],
                              nextSegmentRotation = ref summonTentacleInfo.Rotation[nextSegmentIndex];
                    float rotation = currentSegmentPosition.AngleTo(nextSegmentPosition);
                    if (last) {
                        rotation = previousSegmentPosition.AngleTo(currentSegmentPosition);
                    }              
                    currentSegmentRotation = rotation;

                    Vector2 centerPoint(Vector2 a, Vector2 b) => new((a.X + b.X) / 2f, (a.Y + b.Y) / 2f);
                    Vector2 startPosition = previousSegmentPosition;
                    Vector2 destination = nextSegmentPosition;
                    if (previousSegmentIndex == currentSegmentIndex) {
                        startPosition = rootPosition;
                    }
                    if (last) {
                        destination = mainDestination;
                    }
                    if (first) {
                        currentSegmentPosition = startPosition;
                        continue;
                    }
                    if (startPosition.Distance(destination) < 1f) {
                        continue;
                    }
                    currentSegmentPosition = centerPoint(startPosition, destination);
                    float maxDistance = 100f;
                    if (currentSegmentPosition.Distance(destination) > maxDistance) {
                        currentSegmentPosition = destination;
                    }
                }
            }
        }

        init();
        scaleUp();
        enrage();
        animateBulb();
        processSummonMise();
        processSummonTentacles();
    }

    public override void OnKill(int timeLeft) {

    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Bulb>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        float randomnessSeed = Projectile.position.Length();

        Vector2 center = Projectile.Center;
        float seedForAnimation = Projectile.whoAmI;

        SpriteBatch batch = Main.spriteBatch;

        Texture2D bulbTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb].Value,
                  bulbTexture_Back = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb_Back].Value,
                  bulb2Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb2].Value,
                  bulb2Texture_Back = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb2_Back].Value,
                  stem1Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem1].Value,
                  stem2Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem2].Value,
                  leafStem1Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.LeafStem1].Value,
                  leafTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Leaf].Value,
                  stamenTexture_Yellow = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stamen_Yellow].Value,
                  stamenTexture_Green = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stamen_Green].Value,
                  summonMouthTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.SummonMouth].Value,
                  summonTentacleTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.SummonTentacle].Value;

        int texturePadding = 4;
        float plantScaleFactor = Projectile.scale;
        Vector2 plantScale = Vector2.One * plantScaleFactor;

        int bulbFrame = Projectile.frame,
            bulbFrameColumn = ShouldTransform.ToInt();

        Color yellowPartColor_Active = new(251, 201, 40),
              yellowPartColor_NonActive = new(77, 74, 81);

        // BULB
        Rectangle bulbClip = Utils.Frame(bulbTexture, BULBFRAMECOUNT_COLUMN, BULBFRAMECOUNT_ROW, frameX: bulbFrameColumn, frameY: bulbFrame);
        Vector2 bulbOrigin = bulbClip.BottomCenter();
        DrawInfo bulbDrawInfo = new() {
            Clip = bulbClip,
            Origin = bulbOrigin,

            Scale = plantScale
        };

        // BULB 2
        Rectangle bulb2Clip = Utils.Frame(bulb2Texture, 1, BULB2FRAMECOUNT_ROW, frameY: bulbFrame);
        Vector2 bulb2Origin = bulb2Clip.BottomCenter();
        DrawInfo bulb2DrawInfo = new() {
            Clip = bulb2Clip,
            Origin = bulb2Origin,

            Scale = plantScale
        };

        // STEM 1
        Rectangle stem1Clip = stem1Texture.Bounds;
        Vector2 stem1Origin = stem1Clip.BottomCenter();
        DrawInfo stem1DrawInfo = new() {
            Clip = stem1Clip,
            Origin = stem1Origin,

            Scale = plantScale
        };

        // STEM 2
        Rectangle stem2Clip = stem2Texture.Bounds;
        Vector2 stem2Origin = stem2Clip.BottomCenter();
        DrawInfo stem2DrawInfo = new() {
            Clip = stem2Clip,
            Origin = stem2Origin,

            Scale = plantScale
        };

        // LEAF STEM 1
        Rectangle leafStem1Clip = leafStem1Texture.Bounds;
        Vector2 leafStem1Origin = leafStem1Clip.BottomCenter();
        DrawInfo leafStem1DrawInfo = new() {
            Clip = leafStem1Clip,
            Origin = leafStem1Origin,

            Scale = plantScale
        };

        // LEAF
        Rectangle leafClip = Utils.Frame(leafTexture, 1, LEAFFRAMECOUNT);
        Vector2 leafOrigin = leafStem1Clip.LeftCenter() + new Vector2(0f, texturePadding);
        DrawInfo leafDrawInfo = new() {
            Clip = leafClip,
            Origin = leafOrigin,

            Scale = plantScale
        };

        // STAMEN YELLOW
        Rectangle getStamenClip_Yellow(int frameY = 0) => Utils.Frame(stamenTexture_Yellow, 1, STAMENFRAMECOUNT_YELLOW, frameY: frameY);
        Rectangle stamenClip_Yellow = getStamenClip_Yellow();
        Vector2 stamenOrigin_Yellow = stamenClip_Yellow.Centered();
        DrawInfo stamenDrawInfo_Yellow = new() {
            Clip = stamenClip_Yellow,
            Origin = stamenOrigin_Yellow,

            Scale = plantScale
        };

        // STAMEN GREEN
        Rectangle getStamenClip_Green(int frameY = 0) => Utils.Frame(stamenTexture_Green, 1, STAMENFRAMECOUNT_GREEN, frameY: frameY);
        Rectangle stamenClip_Green = getStamenClip_Green();
        Vector2 stamenOrigin_Green = stamenClip_Green.Centered();
        DrawInfo stamenDrawInfo_Green = new() {
            Clip = stamenClip_Green,
            Origin = stamenOrigin_Green,

            Scale = plantScale
        };

        // SUMMON MOUTH
        Rectangle summonMouthClip = summonMouthTexture.Bounds;
        Vector2 summonMouthOrigin = summonMouthClip.Centered();
        DrawInfo summonMouthDrawInfo = new() {
            Clip = summonMouthClip,
            Origin = summonMouthOrigin,

            Scale = plantScale
        };

        // SUMMON TENTACLE
        Rectangle summonTentacleClip = summonTentacleTexture.Bounds;
        Vector2 summonTentacleOrigin = summonTentacleClip.BottomCenter();
        DrawInfo summonTentacleDrawInfo = new() {
            Clip = summonTentacleClip,
            Origin = summonTentacleOrigin,

            Scale = plantScale
        };

        void drawMainStem() {
            int height = stem1Clip.Height - texturePadding;
            height = (int)(height * plantScaleFactor);
            Vector2 startPosition = RootPosition;
            Vector2 endPosition = center + center.DirectionFrom(startPosition) * height;
            float scaleFactor = 0f;
            float getDistanceToBulb() => Vector2.Distance(startPosition, endPosition);
            while (getDistanceToBulb() > height) {
                float lerpValue = 0.25f;
                scaleFactor = Helper.Approach(scaleFactor, height, lerpValue);

                Vector2 scale = Vector2.One * (0.25f + Utils.GetLerpValue(0f, height, scaleFactor, true));
                float sizeIncreaseFluff = 3f;
                if (getDistanceToBulb() < height * sizeIncreaseFluff) {
                    scale = Helper.Approach(scale, new Vector2(10f), lerpValue);
                }

                Vector2 position = startPosition;
                batch.Draw(stem1Texture, position, stem1DrawInfo with {
                    Scale = plantScale * scale
                });

                Vector2 velocityToBulbPosition = startPosition.DirectionTo(endPosition);
                startPosition += velocityToBulbPosition * scaleFactor;
            }
        }
        void getBulbPosition(out Vector2 bulbPosition_Origin, out Vector2 bulbPosition_Result) {
            Vector2 bulbOffset = new(0f, 24f);
            Vector2 bulbPosition = center;

            bulbPosition_Origin = bulbPosition;

            bulbPosition += bulbOffset;

            bulbPosition_Result = bulbPosition;
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

            float stem2OffsetFromBulbValue = 8f * plantScaleFactor;
            Vector2 angleFromBulbToRoot = bulbPosition_Origin.DirectionTo(RootPosition);
            Vector2 stem2Position = bulbPosition_Origin + angleFromBulbToRoot * stem2OffsetFromBulbValue;

            float stem3OffsetFromBulbValue = 4f * plantScaleFactor;
            Vector2 stem3Position = bulbPosition_Origin + angleFromBulbToRoot * stem3OffsetFromBulbValue;

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
        void drawLeafStem(Vector2 startVelocity, bool stamenStem = false) {
            int height = leafStem1Clip.Height - texturePadding;
            height = (int)(height * plantScaleFactor * 0.95f);

            int currentStamenIndex = 0;
            void drawStamen(Vector2 startPosition, int direction, float startRotation = 0f) {
                float currentLength = 4f;
                Vector2 position = startPosition;
                float currentRotation = startRotation;
                float scaleFactor = 0.5f,
                      scaleFactorLerpValue = 0.1f;
                while (currentLength > 0f) {
                    uint seed = (uint)(randomnessSeed * (generalCurrentStamenIndex + currentStamenIndex) * 15);
                    ulong seed_ulong = (ulong)seed;

                    float sineOffset = 0f;
                    switch (currentStamenIndex) {
                        case 1:
                            sineOffset = 2f;
                            break;
                        case 2:
                            sineOffset = 0f;
                            break;
                    }
                    sineOffset += MathUtils.PseudoRandRange(ref seed, -0.5f, 0.5f);

                    float baseStep = height,
                          step = baseStep * scaleFactor * TransformFactor;

                    scaleFactor = Helper.Approach(scaleFactor, 1f, scaleFactorLerpValue);

                    currentRotation += -MathF.Sin(currentLength * 7.5f + sineOffset) * 0.5f * direction;

                    batch.Draw(leafStem1Texture, position, leafStem1DrawInfo.WithScale(scaleFactor * transformFactorForScale) with {
                        Rotation = currentRotation
                    });

                    bool shouldDrawStamen = currentLength <= 1f;
                    if (shouldDrawStamen) {
                        Vector2 stamenPosition = position;
                        stamenPosition += -Vector2.UnitY.RotatedBy(currentRotation) * baseStep;

                        batch.Draw(stamenTexture_Green, stamenPosition, stamenDrawInfo_Green.WithScale(transformFactorForScale) with {
                            Clip = getStamenClip_Green(Utils.RandomInt(ref seed_ulong, STAMENFRAMECOUNT_GREEN))
                        });
                        float yellowPartScaleModifier = 0.625f;
                        float activationProgress = _stamenActivated[generalCurrentStamenIndex].Progress;
                        Color yellowPartCurrentColor = Color.Lerp(yellowPartColor_NonActive, yellowPartColor_Active, activationProgress);
                        batch.Draw(stamenTexture_Yellow, stamenPosition, stamenDrawInfo_Yellow.WithScale(transformFactorForScale)
                            .WithScale(yellowPartScaleModifier)
                            .WithColor(yellowPartCurrentColor) with {
                            Clip = getStamenClip_Yellow(Utils.RandomInt(ref seed_ulong, STAMENFRAMECOUNT_YELLOW))
                        });
                    }

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
            float startPositionOffsetFactor = 2.5f * plantScaleFactor;
            startPosition += startVelocity * startPositionOffsetFactor;

            int direction = startVelocity.X.GetDirection();
            SpriteEffects flip = direction.ToSpriteEffects();
            bool facedRight = direction > 0;

            float currentLength = 9f,
                  length = currentLength;
            float xLerpValue = 0.1f,
                  yLerpValue = 0f;
            float scaleFactor = 0.5f;
            float leafStemScaleLerpValue = 0.025f;
            while (currentLength > 0f) {
                scaleFactor = Helper.Approach(scaleFactor, 1f, leafStemScaleLerpValue);

                leafStemScaleLerpValue = Helper.Approach(leafStemScaleLerpValue, 0.1f, TimeSystem.LogicDeltaTime);

                float baseStep = height,
                      step = baseStep * scaleFactor * TransformFactor;

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
                    batch.Draw(leafStem1Texture, position, leafStem1DrawInfo.WithScale(scaleFactor * transformFactorForScale) with {
                        Rotation = rotation,
                        ImageFlip = flip
                    });
                }

                if (shouldDrawStamen) {
                    drawStem(position);

                    if (shouldDrawStamen_Inner1) {
                        float stamenStemStartRotation = MathHelper.PiOver2 * 1f * direction;
                        drawStamen(position, direction, stamenStemStartRotation);
                        stamenStemStartRotation = MathHelper.PiOver2 * 0.375f * direction;
                        drawStamen(position, direction, stamenStemStartRotation);
                    }
                    if (shouldDrawStamen_Inner2) {
                        float stamenStemStartRotation = 0f;
                        drawStamen(position, direction, stamenStemStartRotation);
                    }
                }
                else if (shouldDrawLeaf) {
                    float leafRotation = rotation + MathHelper.PiOver2 * direction;
                    Vector2 leafOffset = -Vector2.UnitX.RotatedBy(leafRotation) * baseStep;
                    if (!facedRight) {
                        leafOffset *= 0.8f;
                    }
                    position += leafOffset;
                    Vector2 leafOffset2 = -Vector2.UnitY.RotatedBy(leafRotation) * 2f;
                    position += leafOffset2;
                    batch.Draw(leafTexture, position, leafDrawInfo.WithScale(transformFactorForScale) with {
                        Rotation = leafRotation,
                        ImageFlip = flip
                    });
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
            foreach (SummonMouthInfo summonMouthInfo in _summonMouthData) {
                Vector2 position = summonMouthInfo.Position;
                float rotation = summonMouthInfo.Rotation;
                SpriteEffects flip = summonMouthInfo.FacedRight.ToSpriteEffects2();

                int stemHeight = leafStem1Clip.Height - texturePadding;
                stemHeight = (int)(stemHeight * plantScaleFactor * 0.95f);
                Vector2 stemStartPosition = position,
                        stemEndPosition = center;
                float stemScaleFactor = 1.5f;
                int i = 0;
                float maxOffsetX = 50f;
                while (true) {
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

                    batch.Draw(leafStem1Texture, stemPosition, leafStem1DrawInfo.WithScale(stemScaleFactor) with {
                        Rotation = stemRotation
                    });

                    stemStartPosition += velocityToSummonMouthPosition * step;
                }

                batch.Draw(summonMouthTexture, position, summonMouthDrawInfo with {
                    Rotation = rotation,
                    ImageFlip = flip
                });
            }
        }
        void drawSummonTentacles() {
            foreach (SummonTentacleInfo summonTentacleInfo in _summonTentacleData) {
                int segmentCount = summonTentacleInfo.Position.Length;
                for (int i = 0; i < segmentCount; i++) {
                    Vector2 position = summonTentacleInfo.Position[i];
                    position = Vector2.Lerp(position, center, TransformFactor);
                    float rotation = summonTentacleInfo.Rotation[i] + MathHelper.PiOver2;
                    float scale = summonTentacleInfo.Scale[i];
                    batch.Draw(summonTentacleTexture, position, summonTentacleDrawInfo.WithScaleX(scale) with {
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

    private bool AcceptedEnoughDamage() {
        bool result = false;
        result = _stamenActivated.Where(checkStamenDamageDone => checkStamenDamageDone.Progress >= 1f).Count() >= STAMENACTIVATIONSLOTCOUNT;
        return result;
    }

    public void AcceptDamage(int damageDone) {
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
