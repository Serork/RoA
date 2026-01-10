using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Projectiles;
using RoA.Content.Projectiles.Friendly.Ranged;
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

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BiedermeierFlower : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static byte FLOWERCOUNTINABOUQUET => (byte)FlowerType.Count + 1 + 3;

    public enum BiedermeierFlowerTextureType : byte {
        Flower,
        Flower_Base,
        Stem,
        Leaf1,
        Leaf2,
        Leaf3,
        Leaf3_Base
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)BiedermeierFlowerTextureType.Flower, ResourceManager.NatureProjectileTextures + "BiedermeierFlower"),
         ((byte)BiedermeierFlowerTextureType.Flower_Base, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Base"),
         ((byte)BiedermeierFlowerTextureType.Stem, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Stem"),
         ((byte)BiedermeierFlowerTextureType.Leaf1, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Leaf1"),
         ((byte)BiedermeierFlowerTextureType.Leaf2, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Leaf2"),
         ((byte)BiedermeierFlowerTextureType.Leaf3, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Leaf3"),
         ((byte)BiedermeierFlowerTextureType.Leaf3_Base, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Leaf3_Base")];

    public enum FlowerType : byte {
        Sweet,
        Exotic,
        Weeping,
        Perfect1,
        Perfect2,
        Perfect3,
        Custom,
        Acalypha,
        Count
    }

    private record struct FlowerInfo(FlowerType FlowerType, Vector2 Offset, float Rotation, float Progress = 0f, float Progress2 = 0f, bool FacedRight = false, bool Released = false);

    private FlowerInfo[] _flowerData = null!;

    public ref float InitOnSpawnValue => ref Projectile.localAI[0];

    public bool Init {
        get => InitOnSpawnValue == 1f;
        set => InitOnSpawnValue = value.ToInt();
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
    }

    public override void AI() {
        void init() {
            if (!Init) {
                Init = true;

                _flowerData = new FlowerInfo[FLOWERCOUNTINABOUQUET];
                List<FlowerType> flowersInABouquet = [];
                FlowerType flowerTypeToAdd = Main.rand.GetRandomEnumValue<FlowerType>(1);
                flowersInABouquet.Add(flowerTypeToAdd);
                for (int i = 0; i < 2; i++) {
                    flowerTypeToAdd = Main.rand.GetRandomEnumValue<FlowerType>(1);
                    while (flowersInABouquet.Contains(flowerTypeToAdd)) {
                        flowerTypeToAdd = Main.rand.GetRandomEnumValue<FlowerType>(1);
                    }
                    flowersInABouquet.Add(flowerTypeToAdd);
                }
                int index = 0;
                while (index < FLOWERCOUNTINABOUQUET) {
                    FlowerType firstFlowerTypeInABouquet = flowersInABouquet[0],
                               secondFlowerTypeInABouquet = flowersInABouquet[1],
                               thirdFlowerTypeInABouquet = flowersInABouquet[2];
                    FlowerType flowerInABouquetToAdd = firstFlowerTypeInABouquet;
                    Vector2 offset = Vector2.Zero;
                    switch (index) {
                        case 0:
                            offset = new Vector2(0f, -75f);
                            flowerInABouquetToAdd = firstFlowerTypeInABouquet;
                            break;
                        case 1:
                            offset = new Vector2(25f, -85f);
                            flowerInABouquetToAdd = firstFlowerTypeInABouquet;
                            break;
                        case 2:
                            offset = new Vector2(-25f, -85f);
                            flowerInABouquetToAdd = firstFlowerTypeInABouquet;
                            break;
                        case 3:
                            offset = new Vector2(40f, -115f);
                            flowerInABouquetToAdd = firstFlowerTypeInABouquet;
                            break;
                        case 4:
                            offset = new Vector2(-40f, -115f);
                            flowerInABouquetToAdd = firstFlowerTypeInABouquet;
                            break;
                        case 5:
                            offset = new Vector2(30f, -130f);
                            flowerInABouquetToAdd = secondFlowerTypeInABouquet;
                            break;
                        case 6:
                            offset = new Vector2(-30f, -130f);
                            flowerInABouquetToAdd = secondFlowerTypeInABouquet;
                            break;
                        case 7:
                            offset = new Vector2(20f, -105f);
                            flowerInABouquetToAdd = secondFlowerTypeInABouquet;
                            break;
                        case 8:
                            offset = new Vector2(-20f, -105f);
                            flowerInABouquetToAdd = secondFlowerTypeInABouquet;
                            break;
                        case 9:
                            offset = new Vector2(15f, -125f);
                            flowerInABouquetToAdd = thirdFlowerTypeInABouquet;
                            if (flowerInABouquetToAdd == FlowerType.Acalypha || 
                                flowerInABouquetToAdd == FlowerType.Custom) {
                                //offset.Y -= 10f;
                            }
                            break;
                        case 10:
                            offset = new Vector2(-15f, -125f);
                            flowerInABouquetToAdd = thirdFlowerTypeInABouquet;
                            if (flowerInABouquetToAdd == FlowerType.Acalypha ||
                                flowerInABouquetToAdd == FlowerType.Custom) {
                                //offset.Y -= 10f;
                            }
                            break;
                        case 11:
                            offset = new Vector2(0f, -135f);
                            flowerInABouquetToAdd = thirdFlowerTypeInABouquet;
                            if (flowerInABouquetToAdd == FlowerType.Acalypha ||
                                flowerInABouquetToAdd == FlowerType.Custom) {
                                //offset.Y -= 10f;
                            }
                            break;
                    }
                    float rotation = offset.X / 100f;
                    offset.Y *= 0.7f;
                    offset.Y -= 30f;
                    if (flowerInABouquetToAdd == FlowerType.Acalypha) {
                        offset.Y -= 10f;
                    }
                    if (flowerInABouquetToAdd == FlowerType.Custom) {
                        offset.Y -= 5f;
                    }
                    offset = new(offset.X * Main.rand.NextFloat(0.975f, 1.025f), offset.Y * Main.rand.NextFloat(0.975f, 1.025f));
                    _flowerData[index] = new FlowerInfo(flowerInABouquetToAdd, offset, rotation, FacedRight: Main.rand.NextBool());
                    //flowersInABouquet.Remove(flowerInABouquetToAdd);
                    index++;
                }
            }
        }
        void setPosition() {
            Player player = Projectile.GetOwnerAsPlayer();
            float num = (float)Math.PI / 2f;
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
            int num2 = 2;
            float num3 = 0f;
            float num8 = 1f * Projectile.scale;
            Vector2 vector3 = vector;
            Vector2 value = Main.screenPosition + new Vector2(Main.mouseX, Main.mouseY) - vector3;
            if (player.gravDir == -1f)
                value.Y = (float)(Main.screenHeight - Main.mouseY) + Main.screenPosition.Y - vector3.Y;

            Vector2 vector4 = Vector2.Normalize(value);
            if (float.IsNaN(vector4.X) || float.IsNaN(vector4.Y))
                vector4 = -Vector2.UnitY;

            vector4 = Vector2.Normalize(Vector2.Lerp(vector4, Vector2.Normalize(Projectile.velocity), 0.85f));
            vector4 *= num8;
            if (vector4.X != Projectile.velocity.X || vector4.Y != Projectile.velocity.Y)
                Projectile.netUpdate = true;

            Projectile.velocity = vector4;
            Projectile.Center = player.GetPlayerCorePoint();
            Projectile.rotation = Projectile.velocity.ToRotation() + num;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
            player.ChangeDir(Projectile.direction);
            player.heldProj = Projectile.whoAmI;
            player.SetDummyItemTime(num2);
            player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(Projectile.velocity.Y * (float)Projectile.direction, Projectile.velocity.X * (float)Projectile.direction) + num3);
            player.SetCompositeBothArms(player.itemRotation - MathHelper.PiOver2 * Projectile.spriteDirection, Player.CompositeArmStretchAmount.Full);
        }
        void processFlowers() {
            float allProgress = 0f;
            int flowerCount = _flowerData.Length;
            for (int i = 0; i < flowerCount; i++) {
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Max(0, i - 1);
                ref FlowerInfo currentSegmentData = ref _flowerData![currentSegmentIndex],
                                previousSegmentData = ref _flowerData[previousSegmentIndex];
                allProgress += currentSegmentData.Progress;
                if (currentSegmentIndex > 0 && previousSegmentData.Progress < 0.25f) {
                    continue;
                }
                float lerpValue = 0.05f;
                currentSegmentData.Progress = Helper.Approach(currentSegmentData.Progress, 1f, lerpValue);
            }
            float allProgress2 = 0f;
            if (allProgress >= flowerCount) {
                for (int i = 0; i < flowerCount; i++) {
                    int currentSegmentIndex = i,
                        previousSegmentIndex = Math.Max(0, i - 1);
                    ref FlowerInfo currentSegmentData = ref _flowerData![currentSegmentIndex],
                                    previousSegmentData = ref _flowerData[previousSegmentIndex];
                    if (currentSegmentIndex > 0 && previousSegmentData.Progress2 < 1f) {
                        continue;
                    }
                    float lerpValue = 0.075f;
                    currentSegmentData.Progress2 = Helper.Approach(currentSegmentData.Progress2, 3f, lerpValue);
                    if (currentSegmentData.Progress2 >= 1.5f && !currentSegmentData.Released) {
                        if (Projectile.IsOwnerLocal()) {
                            ProjectileUtils.SpawnPlayerOwnedProjectile<BiedermeierPetal>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromAI()) {
                                Position = Projectile.Center + currentSegmentData.Offset.RotatedBy(Projectile.rotation) * 1.15f,
                                Velocity = Vector2.UnitY.RotatedBy(Projectile.rotation + MathHelper.Pi),
                                Damage = Projectile.damage,
                                KnockBack = Projectile.knockBack,
                                AI0 = (float)currentSegmentData.FlowerType,
                                AI1 = currentSegmentData.Rotation + MathHelper.PiOver2
                            });
                        }
                        currentSegmentData.Released = true;
                    }
                    allProgress2 += currentSegmentData.Progress2;
                }
            }
            if (allProgress2 >= 3f * flowerCount) {
                Projectile.Kill();
            }
        }

        init();
        setPosition();
        processFlowers();
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<BiedermeierFlower>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D texture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Flower].Value,
                  texture_Base = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Flower_Base].Value;
        SpriteBatch batch = Main.spriteBatch;
        IEnumerable<FlowerInfo> sortedFlowerData = _flowerData.OrderBy(x => x.Offset.Y);
        Texture2D stemTexture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Stem].Value,
                  leaf1Texture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Leaf1].Value,
                  leaf2Texture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Leaf2].Value,
                  leaf3Texture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Leaf3].Value,
                  leaf3Texture_Base = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Leaf3_Base].Value;
        foreach (FlowerInfo flowerInfo in sortedFlowerData) {
            float progress = flowerInfo.Progress;
            float progress2 = flowerInfo.Progress2;
            //progress -= Utils.GetLerpValue(2.5f, 3f, progress2, true);
            progress = Ease.CubeOut(progress);
            //progress2 = Ease.CubeInOut(progress2);
            float opacity = Utils.GetLerpValue(0f, 0.2f, progress, true);
            Color baseColor = Color.White * opacity,
                  flowerColor = baseColor;
            Color stemGlowColor = baseColor with { A = 100 } * 1f;
            Vector2 scale = Vector2.One;
            float leafProgressThresholdForGlowing = 0.5f;
            FlowerType flowerType = flowerInfo.FlowerType;
            Rectangle clip = Utils.Frame(texture, (byte)FlowerType.Count, 1, frameX: (byte)flowerType);
            Vector2 origin = clip.TopCenter();
            float flowerRotation = flowerInfo.Rotation,
                  baseRotation = Projectile.rotation;
            float rotation = baseRotation + flowerRotation;
            SpriteEffects flip = flowerInfo.FacedRight.ToSpriteEffects();
            Rectangle flowerClip = clip;
            float progress5 = MathUtils.Clamp01(progress2 - 1f);
            //flowerColor = Color.Lerp(flowerColor, Color.Black, progress5);
            stemGlowColor = Color.Lerp(stemGlowColor, baseColor, progress5);
            //flowerClip.Height = (int)(clip.Height * (1f - MathUtils.Clamp01(progress2 - 1f)));
            float stemGlowScaleFactor = 1f + (0.25f * opacity * (1f - progress5));
            DrawInfo drawInfo = new() {
                Clip = flowerClip,
                Origin = origin,
                Rotation = rotation + MathHelper.Pi,
                ImageFlip = flip,
                Color = flowerColor,
                Scale = scale
            };
            //flowerClip.Height = clip.Height;
            DrawInfo drawInfo2 = drawInfo with {
                Color = baseColor
            };
            float progress3 = progress2;
            progress2 = MathUtils.Clamp01(progress2);
            float progress4 = progress2;
            //progress2 *= Utils.GetLerpValue(3f, 2f, progress3, true);
            Vector2 playerCenter = Projectile.GetOwnerAsPlayer().GetPlayerCorePoint() - Projectile.velocity * 10f;
            Vector2 position = Vector2.Lerp(playerCenter, Projectile.Center + flowerInfo.Offset.RotatedBy(baseRotation), MathF.Max(0.1f, progress));
            float sineOffset = 0.05f;
            void drawStem() {
                int stemFrameX = 0;
                switch (flowerType) {
                    case FlowerType.Sweet:
                        break;
                    case FlowerType.Exotic:
                        stemFrameX = 1;
                        break;
                    case FlowerType.Weeping:
                        stemFrameX = 2;
                        break;
                    case FlowerType.Perfect1:
                    case FlowerType.Perfect2:
                    case FlowerType.Perfect3:
                        stemFrameX = 3;
                        break;
                    case FlowerType.Custom:
                        stemFrameX = 4;
                        break;
                    case FlowerType.Acalypha:
                        stemFrameX = 5;
                        break;
                }
                Rectangle stemClip = Utils.Frame(stemTexture, 6, 1, frameX: stemFrameX);
                int height = stemClip.Height;
                height = (int)(height * 0.85f);
                Vector2 stemStartPosition = position,
                        stemEndPosition = playerCenter;
                stemStartPosition += stemStartPosition.DirectionTo(stemEndPosition) * 10f;
                if (flowerType == FlowerType.Acalypha) {
                    stemStartPosition += Vector2.UnitX.RotatedBy(flowerRotation) * -4f * flowerInfo.FacedRight.ToDirectionInt();
                }
                Vector2 stemPosition = stemStartPosition;
                Vector2 stemOrigin = stemClip.BottomCenter();
                uint seedForRandomness = (uint)((byte)flowerType + flowerInfo.Offset.Length() * 2f);
                int index = (int)MathUtils.PseudoRandRange(ref seedForRandomness, 100f);
                int baseIndex = index;
                while (true) {
                    if (Vector2.Distance(stemPosition, stemEndPosition) < height * 0.75f) {
                        break;
                    }
                    float progress = stemPosition.Distance(stemEndPosition) / stemStartPosition.Distance(stemEndPosition);
                    Vector2 velocity = stemPosition.DirectionTo(stemEndPosition);
                    velocity = velocity.RotatedBy(Math.Sin(index) * sineOffset);
                    float stemRotation = velocity.ToRotation() - MathHelper.PiOver2;
                    SpriteEffects leafFlip = flip == SpriteEffects.None ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    DrawInfo stemDrawInfo = new() {
                        Clip = stemClip,
                        Origin = stemOrigin,
                        Rotation = stemRotation,
                        ImageFlip = leafFlip,
                        Color = baseColor,
                        Scale = scale
                    };
                    batch.Draw(stemTexture, stemPosition, stemDrawInfo);
                    //if (progress2 >= progress) 
                    {
                        float opacity = Utils.GetLerpValue(progress, progress * 1.5f, progress2, true);
                        batch.Draw(stemTexture, stemPosition, stemDrawInfo.WithScaleX(stemGlowScaleFactor) with {
                            Color = stemGlowColor * opacity
                        });
                    }
                    stemPosition += velocity * height;
                    index++;
                }
                if (flowerType == FlowerType.Custom) {
                    Texture2D leafTexture = leaf2Texture;
                    Rectangle leafClip = Utils.Frame(leafTexture, 2, 1, frameX: 0),
                              leafClip2 = Utils.Frame(leafTexture, 2, 1, frameX: 1);
                    Vector2 leafStartPosition = position + stemStartPosition.DirectionTo(stemEndPosition) * 10f,
                            leafEndPosition = stemEndPosition;
                    Vector2 leafPosition = leafStartPosition;
                    Vector2 leafOrigin = leafClip.BottomCenter();
                    seedForRandomness = (uint)((byte)flowerType + flowerInfo.Offset.Length() * 2f);
                    index = (int)MathUtils.PseudoRandRange(ref seedForRandomness, 100f);
                    while (true) {
                        if (Vector2.Distance(leafPosition, leafEndPosition) < height * 0.5f) {
                            break;
                        }
                        Vector2 velocity = leafPosition.DirectionTo(leafEndPosition);
                        velocity = velocity.RotatedBy(Math.Sin(index) * sineOffset);
                        float leafRotation = velocity.ToRotation() - MathHelper.PiOver2;
                        bool flipped = index % 2 == 0;
                        SpriteEffects leafFlip = flipped.ToSpriteEffects();
                        DrawInfo leafDrawInfo = new() {
                            Clip = leafClip,
                            Origin = leafOrigin,
                            Rotation = leafRotation,
                            ImageFlip = leafFlip,
                            Color = baseColor,
                            Scale = scale
                        };
                        int offsetDirection = flipped.ToDirectionInt();
                        bool shouldDraw = MathUtils.PseudoRandRange(ref seedForRandomness, 1f) < 0.35f;
                        if (shouldDraw) {
                            batch.Draw(leafTexture, leafPosition + Vector2.UnitX.RotatedBy(leafRotation) * -6f * offsetDirection, leafDrawInfo);
                            batch.Draw(leafTexture, leafPosition + Vector2.UnitX.RotatedBy(leafRotation) * 8f * offsetDirection, leafDrawInfo with {
                                Clip = leafClip2
                            });
                            if (progress2 >= leafProgressThresholdForGlowing) {
                                batch.Draw(leafTexture, leafPosition + Vector2.UnitX.RotatedBy(leafRotation) * -6f * offsetDirection, leafDrawInfo.WithScaleX(stemGlowScaleFactor) with {
                                    Color = stemGlowColor
                                });
                                batch.Draw(leafTexture, leafPosition + Vector2.UnitX.RotatedBy(leafRotation) * 8f * offsetDirection, leafDrawInfo.WithScaleX(stemGlowScaleFactor) with {
                                    Color = stemGlowColor,
                                    Clip = leafClip2
                                });
                            }
                        }
                        leafPosition += velocity * height;
                        index++;
                    }
                }
                if (flowerType == FlowerType.Acalypha) {
                    Texture2D leafTexture = leaf3Texture,
                              leafTexture_Base = leaf3Texture_Base;
                    Rectangle leafClip = Utils.Frame(leafTexture, 1, 2, frameY: 0),
                              leafClip2 = Utils.Frame(leafTexture, 1, 2, frameY: 1);
                    Vector2 leafStartPosition = position + stemStartPosition.DirectionTo(stemEndPosition) * 10f,
                            leafEndPosition = stemEndPosition - stemStartPosition.DirectionTo(stemEndPosition) * 10f;
                    Vector2 leafPosition = leafStartPosition;
                    Vector2 leafOrigin = leafClip.BottomRight();
                    seedForRandomness = (uint)((byte)flowerType + flowerInfo.Offset.Length() * 2f);
                    index = baseIndex;
                    int index2 = 0;
                    while (true) {
                        if (Vector2.Distance(leafPosition, leafEndPosition) < height * 0.5f) {
                            break;
                        }
                        Vector2 velocity = leafPosition.DirectionTo(leafEndPosition);
                        velocity = velocity.RotatedBy(Math.Sin(index) * sineOffset);
                        float leafRotation = velocity.ToRotation() - MathHelper.PiOver2;
                        bool flipped = index % 4 == 0;
                        if (flowerInfo.FacedRight) {
                            flipped = !flipped;
                        }
                        SpriteEffects leafFlip = (!flipped).ToSpriteEffects();
                        DrawInfo leafDrawInfo = new() {
                            Clip = leafClip,
                            Origin = leafOrigin,
                            Rotation = leafRotation,
                            ImageFlip = leafFlip,
                            Color = baseColor,
                            Scale = scale
                        };
                        int offsetDirection = flipped.ToDirectionInt();
                        bool shouldDraw = index2 == 1 || index2 == 2;
                        if (index > 29) {
                            shouldDraw = false;
                        }
                        if (shouldDraw) {
                            float offsetValue = -4f;
                            if (flipped) {
                                offsetValue = leafClip.Width - 2f;
                                offsetValue += -4f * flowerInfo.FacedRight.ToDirectionInt();
                            }
                            Vector2 offset = Vector2.UnitX.RotatedBy(leafRotation) * offsetValue * offsetDirection;
                            DrawInfo leafDrawInfo2 = leafDrawInfo;
                            if (index2 == 1) {
                                leafDrawInfo2 = leafDrawInfo with {
                                    Clip = leafClip2
                                };
                            }
                            batch.Draw(leafTexture, leafPosition + offset, leafDrawInfo2);
                            if (progress2 >= leafProgressThresholdForGlowing) {
                                batch.Draw(leafTexture_Base, leafPosition + offset, leafDrawInfo2.WithScaleX(stemGlowScaleFactor) with {
                                    Color = stemGlowColor
                                });
                            }
                        }
                        if (index2 > 2) {
                            index2 = 0;
                        }
                        leafPosition += velocity * height;
                        index++;
                        index2++;
                    }
                }
                if (flowerType <= FlowerType.Perfect3) {
                    void drawLeafs() {
                        Texture2D leafTexture = leaf1Texture;
                        Rectangle leafClip = Utils.Frame(leafTexture, 4, 1, frameX: (byte)stemFrameX);
                        Vector2 leafOrigin = leafClip.BottomCenter();
                        float leafRotation = stemStartPosition.AngleTo(stemEndPosition) - MathHelper.PiOver2;
                        SpriteEffects leafFlip = SpriteEffects.None;
                        Vector2 leafScale = new Vector2(1f, 1f * progress) * scale;
                        for (int i = 0; i < 2; i++) {
                            DrawInfo leafDrawInfo = new() {
                                Clip = leafClip,
                                Origin = leafOrigin,
                                Rotation = leafRotation,
                                ImageFlip = leafFlip,
                                Color = baseColor,
                                Scale = leafScale
                            };
                            bool flipped = leafFlip == SpriteEffects.None;
                            Vector2 leafPosition = stemEndPosition + Vector2.UnitX.RotatedBy(leafRotation) * 8f * flipped.ToDirectionInt();
                            float yOffset = 10f + MathUtils.PseudoRandRange(ref seedForRandomness, 40f);
                            leafPosition -= Vector2.UnitY.RotatedBy(leafRotation) * yOffset;
                            batch.Draw(leafTexture, leafPosition, leafDrawInfo);
                            if (progress2 >= leafProgressThresholdForGlowing) {
                                batch.Draw(leafTexture, leafPosition, leafDrawInfo.WithScaleX(stemGlowScaleFactor) with {
                                    Color = stemGlowColor
                                });
                            }
                            leafFlip = SpriteEffects.FlipHorizontally;
                        }
                    }
                    drawLeafs();
                }
            }
            drawStem();
            if (!flowerInfo.Released) {
                batch.Draw(texture, position, drawInfo);
            }
            batch.Draw(texture_Base, position, drawInfo2);
            if (progress2 >= 1f) {
                batch.Draw(texture_Base, position, drawInfo2.WithScaleX(stemGlowScaleFactor) with {
                    Color = stemGlowColor * 0.5f
                });
            }
        }
    }
}
