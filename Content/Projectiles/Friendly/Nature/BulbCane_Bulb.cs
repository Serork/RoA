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

using Terraria;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Bulb : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(30);

    private static byte LEAFFRAMECOUNT => 2;
    private static byte STAMENFRAMECOUNT_YELLOW => 4;
    private static byte STAMENFRAMECOUNT_GREEN => 4;

    private static float ROOTLENGTH => 150f;
    private static byte SUMMONMOUTHCOUNT => 6;
    private static byte SUMMONTENTACLECOUNT => 3;

    public enum Bulb_RequstedTextureType : byte { 
        Bulb,
        Bulb2,
        Stem1,
        Stem2,
        Stem3,
        LeafStem1,
        Leaf,
        Stamen_Yellow,
        Stamen_Green,
        SummonMouth,
        SummonTentacle
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)Bulb_RequstedTextureType.Bulb, ResourceManager.NatureProjectileTextures + "Bulb"),
         ((byte)Bulb_RequstedTextureType.Bulb2, ResourceManager.NatureProjectileTextures + "Bulb2"),
         ((byte)Bulb_RequstedTextureType.Stem1, ResourceManager.NatureProjectileTextures + "Bulb_Stem1"),
         ((byte)Bulb_RequstedTextureType.Stem2, ResourceManager.NatureProjectileTextures + "Bulb_Stem2"),
         ((byte)Bulb_RequstedTextureType.Stem3, ResourceManager.NatureProjectileTextures + "Bulb_Stem3"),
         ((byte)Bulb_RequstedTextureType.LeafStem1, ResourceManager.NatureProjectileTextures + "Bulb_LeafStem1"),
         ((byte)Bulb_RequstedTextureType.Leaf, ResourceManager.NatureProjectileTextures + "Bulb_Leaf"),
         ((byte)Bulb_RequstedTextureType.Stamen_Yellow, ResourceManager.NatureProjectileTextures + "Bulb_Stamen_Yellow"),
         ((byte)Bulb_RequstedTextureType.Stamen_Green, ResourceManager.NatureProjectileTextures + "Bulb_Stamen_Green"),
         ((byte)Bulb_RequstedTextureType.SummonMouth, ResourceManager.NatureProjectileTextures + "Bulb_SummonMouth"),
         ((byte)Bulb_RequstedTextureType.SummonTentacle, ResourceManager.NatureProjectileTextures + "Bulb_SummonTentacle")];

    public record struct SummonMouthInfo(Vector2 Position, Vector2 Destination, Vector2 Velocity, float Rotation, float BaseRotation, bool FacedRight);
    public record struct SummonTentacleInfo(Vector2 RootPosition, Vector2 Destination, Vector2[] Position, float[] Rotation, float[] Scale, float WaveFrequency);

    private SummonMouthInfo[] _summonMouthData = null!;
    private SummonTentacleInfo[] _summonTentacleData = null!;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float RootPositionX => ref Projectile.ai[0];
    public ref float RootPositionY => ref Projectile.ai[1];
    public ref float SecondFormValue => ref Projectile.ai[2];

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
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

                initSummonMise();
                iniSummonTentacles();

                Init = true;
            }
        }
        void scaleUp() {
            Projectile.scale = 1.5f;
        }
        void enrage() {
            bool shouldActivateEnragedState = Projectile.timeLeft < TIMELEFT - MathUtils.SecondsToFrames(2);
            IsSecondFormActive = shouldActivateEnragedState;
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
                        mainDestination = rootPosition;

                for (int i2 = 0; i2 < segmentCount; i2++) {
                    float progress = i2 / (float)segmentCount;
                    float scale = 1f - 0.5f * Utils.GetLerpValue(0.7f, 1f, progress, true);
                    summonTentacleInfo.Scale[i2] = scale;
                }

                for (int i2 = 0; i2 < segmentCount; i2++) {
                    float scale = summonTentacleInfo.Scale[i2];
                    mainDestination += rootPosition.DirectionTo(summonTentacleInfo.Destination) * tentacleSegmentHeight * scale;
                }

                float maxXOffset = 40f;
                float waveOffset = i * maxXOffset,
                      waveFrequency = summonTentacleInfo.WaveFrequency;
                mainDestination.X += Helper.Wave(-maxXOffset, maxXOffset, waveFrequency, waveOffset);

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
        processSummonMise();
        processSummonTentacles();
    }

    public override void OnKill(int timeLeft) {

    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Bulb>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Vector2 center = Projectile.Center;
        float seedForAnimation = Projectile.whoAmI;

        SpriteBatch batch = Main.spriteBatch;

        Texture2D bulbTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb].Value,
                  bulb2Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Bulb2].Value,
                  stem1Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem1].Value,
                  stem2Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem2].Value,
                  stem3Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stem3].Value,
                  leafStem1Texture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.LeafStem1].Value,
                  leafTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Leaf].Value,
                  stamenTexture_Yellow = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stamen_Yellow].Value,
                  stamenTexture_Green = indexedTextureAssets[(byte)Bulb_RequstedTextureType.Stamen_Green].Value,
                  summonMouthTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.SummonMouth].Value,
                  summonTentacleTexture = indexedTextureAssets[(byte)Bulb_RequstedTextureType.SummonTentacle].Value;

        int texturePadding = 4;
        float plantScaleFactor = Projectile.scale;
        Vector2 plantScale = Vector2.One * plantScaleFactor;

        // BULB
        Rectangle bulbClip = bulbTexture.Bounds;
        Vector2 bulbOrigin = bulbClip.BottomCenter();
        DrawInfo bulbDrawInfo = new() {
            Clip = bulbClip,
            Origin = bulbOrigin,

            Scale = plantScale
        };

        // BULB 2
        Rectangle bulb2Clip = bulbTexture.Bounds;
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

        // STEM 3
        Rectangle stem3Clip = stem3Texture.Bounds;
        Vector2 stem3Origin = stem3Clip.BottomCenter();
        DrawInfo stem3DrawInfo = new() {
            Clip = stem3Clip,
            Origin = stem3Origin,

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
        Rectangle stamenClip_Yellow = Utils.Frame(stamenTexture_Yellow, 1, STAMENFRAMECOUNT_YELLOW);
        Vector2 stamenOrigin_Yellow = stamenClip_Yellow.Centered();
        DrawInfo stamenDrawInfo_Yellow = new() {
            Clip = stamenClip_Yellow,
            Origin = stamenOrigin_Yellow,

            Scale = plantScale
        };

        // STAMEN GREEN
        Rectangle stamenClip_Green = Utils.Frame(stamenTexture_Green, 1, STAMENFRAMECOUNT_GREEN);
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
        void drawBulb() {
            Vector2 bulbPosition = center;

            float stem2OffsetFromBulbValue = 8f * plantScaleFactor;
            Vector2 angleFromBulbToRoot = bulbPosition.DirectionTo(RootPosition);
            Vector2 stem2Position = bulbPosition + angleFromBulbToRoot * stem2OffsetFromBulbValue;

            float stem3OffsetFromBulbValue = 4f * plantScaleFactor;
            Vector2 stem3Position = bulbPosition + angleFromBulbToRoot * stem3OffsetFromBulbValue;

            batch.Draw(stem2Texture, stem2Position, stem2DrawInfo);
            if (!IsSecondFormActive) {
                batch.Draw(bulbTexture, bulbPosition, bulbDrawInfo);
            }
            else {
                batch.Draw(bulb2Texture, bulbPosition, bulb2DrawInfo);
            }
            batch.Draw(stem3Texture, stem3Position, stem3DrawInfo);
        }
        void drawLeafStem(Vector2 startVelocity, bool stamenStem = false) {
            int height = leafStem1Clip.Height - texturePadding;
            height = (int)(height * plantScaleFactor * 0.95f);

            void drawStamen(Vector2 startPosition, int direction, float startRotation = 0f) {
                float currentLength = 2f;
                Vector2 position = startPosition;
                float currentRotation = startRotation;
                while (currentLength > 0f) {
                    float step = height;

                    currentRotation += -MathF.Sin(currentLength * 7.5f) * 0.5f * direction;

                    batch.Draw(leafStem1Texture, position, leafStem1DrawInfo with {
                        Rotation = currentRotation
                    });

                    bool shouldDrawStamen = currentLength <= 1f;
                    if (shouldDrawStamen) {
                        Vector2 stamenPosition = position;
                        stamenPosition += -Vector2.UnitY.RotatedBy(currentRotation) * step;

                        batch.Draw(stamenTexture_Green, stamenPosition, stamenDrawInfo_Green);
                        float yellowPartScaleModifier = 0.625f;
                        batch.Draw(stamenTexture_Yellow, stamenPosition, stamenDrawInfo_Yellow.WithScale(yellowPartScaleModifier));
                    }

                    Vector2 velocity = Vector2.UnitY.RotatedBy(currentRotation);
                    position += -velocity * step;

                    currentLength = Helper.Approach(currentLength, 0f, 1f);
                }
            }

            Vector2 startPosition = center;
            float startPositionOffsetFactor = 2.5f * plantScaleFactor;
            startPosition += startVelocity * startPositionOffsetFactor;

            int direction = startVelocity.X.GetDirection();
            SpriteEffects flip = direction.ToSpriteEffects();
            bool facedRight = direction > 0;

            float currentLength = 8f,
                  length = currentLength;
            float xLerpValue = 0.1f,
                  yLerpValue = 0f;
            float scaleFactor = 0.5f;
            while (currentLength > 0f) {
                float leafStemScaleLerpValue = 0.1f;
                scaleFactor = Helper.Approach(scaleFactor, 1f, leafStemScaleLerpValue);

                float step = height * scaleFactor;

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
                xLerpValue *= Helper.Wave(0.75f, 1f, waveFrequency, seedForAnimation);
                yLerpValue *= Helper.Wave(0.5f, 1f, 2f, seedForAnimation);

                velocityX = Helper.Approach(velocityX, xTo, xLerpValue);
                velocityY = Helper.Approach(velocityY, yTo, yLerpValue);

                bool shouldDrawLeaf_Inner = currentLength <= 1f;
                bool shouldDrawStamen_Inner1 = currentLength == 2f,
                     shouldDrawStamen_Inner2 = currentLength == 3f;

                bool shouldDrawLeaf = !stamenStem && shouldDrawLeaf_Inner,
                     shouldDrawStamen = stamenStem && (shouldDrawStamen_Inner1 || shouldDrawStamen_Inner2);
                Vector2 position = startPosition;

                void drawStem(Vector2 position) {
                    batch.Draw(leafStem1Texture, position, leafStem1DrawInfo.WithScale(scaleFactor) with {
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
                    Vector2 leafOffset = -Vector2.UnitX.RotatedBy(leafRotation) * step;
                    if (!facedRight) {
                        leafOffset *= 0.8f;
                    }
                    position += leafOffset;
                    Vector2 leafOffset2 = -Vector2.UnitY.RotatedBy(leafRotation) * 2f;
                    position += leafOffset2;
                    batch.Draw(leafTexture, position, leafDrawInfo with {
                        Rotation = leafRotation,
                        ImageFlip = flip
                    });
                }
                else {
                    drawStem(position);
                }

                currentLength = Helper.Approach(currentLength, 0f, 1f);

                Vector2 velocityToLeafPosition = startVelocity.SafeNormalize();
                startPosition += velocityToLeafPosition * step;
            }
        }
        void drawLeafStems() {
            drawLeafStem(new Vector2(5f, 0f));
            drawLeafStem(new Vector2(-5f, 0f));
            drawLeafStem(new Vector2(5f, -2.5f), true);
            drawLeafStem(new Vector2(-5f, -2.5f), true);
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
                    float rotation = summonTentacleInfo.Rotation[i] + MathHelper.PiOver2;
                    float scale = summonTentacleInfo.Scale[i];
                    batch.Draw(summonTentacleTexture, position, summonTentacleDrawInfo.WithScaleX(scale) with {
                        Rotation = rotation
                    });
                }
            }
        }

        if (!IsSecondFormActive) {
            drawLeafStems();
        }
        else {
            drawSummonMise();
            drawSummonTentacles();
        }

        drawMainStem();
        drawBulb();
    }
}
