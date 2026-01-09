using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Newtonsoft.Json.Linq;

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

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BiedermeierFlower : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static byte FLOWERCOUNTINABOUQUET => (byte)FlowerType.Count;

    public enum BiedermeierFlowerTextureType : byte {
        Flower,
        Stem,
        Leaf1,
        Leaf2,
        Leaf3
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)BiedermeierFlowerTextureType.Flower, ResourceManager.NatureProjectileTextures + "BiedermeierFlower"),
         ((byte)BiedermeierFlowerTextureType.Stem, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Stem"),
         ((byte)BiedermeierFlowerTextureType.Leaf1, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Leaf1"),
         ((byte)BiedermeierFlowerTextureType.Leaf2, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Leaf2"),
         ((byte)BiedermeierFlowerTextureType.Leaf3, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Leaf3")];

    private enum FlowerType : byte {
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

    private record struct FlowerInfo(FlowerType FlowerType, Vector2 Offset, float Rotation, float Progress = 0f, bool FacedRight = false);

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
                List<FlowerType> flowersInABouquet = [FlowerType.Sweet, FlowerType.Exotic, FlowerType.Weeping, FlowerType.Perfect1, FlowerType.Perfect2, FlowerType.Perfect3, FlowerType.Custom, FlowerType.Acalypha];
                int index = 0;
                while (index < FLOWERCOUNTINABOUQUET) {
                    FlowerType flowerInABouquetToAdd = flowersInABouquet[Main.rand.Next(flowersInABouquet.Count)];
                    Vector2 offset = Vector2.Zero;
                    switch (index) {
                        case 0:
                            offset = new Vector2(0f, -90f);
                            break;
                        case 1:
                            offset = new Vector2(40f, -90f);
                            break;
                        case 2:
                            offset = new Vector2(-40f, -90f);
                            break;
                        case 3:
                            offset = new Vector2(30f, -130f);
                            break;
                        case 4:
                            offset = new Vector2(-30f, -130f);
                            break;
                        case 5:
                            offset = new Vector2(20f, -110f);
                            break;
                        case 6:
                            offset = new Vector2(-20f, -110f);
                            break;
                        case 7:
                            offset = new Vector2(0f, -140f);
                            break;
                    }
                    float rotation = offset.X / 100f;
                    offset.Y -= 30f;
                    if (flowerInABouquetToAdd == FlowerType.Acalypha) {
                        offset.Y -= 10f;
                    }
                    _flowerData[index] = new FlowerInfo(flowerInABouquetToAdd, offset, rotation, FacedRight: Main.rand.NextBool());
                    flowersInABouquet.Remove(flowerInABouquetToAdd);
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
            for (int i = 0; i < _flowerData.Length; i++) {
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Max(0, i - 1);
                ref FlowerInfo currentSegmentData = ref _flowerData![currentSegmentIndex],
                                previousSegmentData = ref _flowerData[previousSegmentIndex];
                if (currentSegmentIndex > 0 && previousSegmentData.Progress < 0.25f) {
                    continue;
                }
                float lerpValue = 0.05f;
                currentSegmentData.Progress = Helper.Approach(currentSegmentData.Progress, 1f, lerpValue);
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

        Texture2D texture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Flower].Value;
        SpriteBatch batch = Main.spriteBatch;
        IEnumerable<FlowerInfo> sortedFlowerData = _flowerData.OrderBy(x => x.Offset.Y);
        Texture2D leafTexture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Stem].Value,
                  leaf1Texture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Leaf1].Value,
                  leaf2Texture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Leaf2].Value,
                  leaf3Texture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Leaf3].Value;
        foreach (FlowerInfo flowerInfo in sortedFlowerData) {
            float progress = flowerInfo.Progress;
            progress = Ease.CubeOut(progress);
            float opacity = Utils.GetLerpValue(0f, 0.2f, progress, true);
            Color color = Color.White * opacity;
            FlowerType flowerType = flowerInfo.FlowerType;
            Rectangle clip = Utils.Frame(texture, FLOWERCOUNTINABOUQUET, 1, frameX: (byte)flowerType);
            Vector2 origin = clip.BottomCenter();
            float flowerRotation = flowerInfo.Rotation,
                  baseRotation = Projectile.rotation;
            float rotation = baseRotation + flowerRotation;
            SpriteEffects flip = flowerInfo.FacedRight.ToSpriteEffects();
            DrawInfo drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Rotation = rotation,
                ImageFlip = flip,
                Color = color
            };
            Vector2 playerCenter = Projectile.GetOwnerAsPlayer().GetPlayerCorePoint() - Projectile.velocity * 10f;
            Vector2 position = Vector2.Lerp(playerCenter, Projectile.Center + flowerInfo.Offset.RotatedBy(baseRotation), MathF.Max(0.1f, progress));
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
                Rectangle stemClip = Utils.Frame(leafTexture, 6, 1, frameX: stemFrameX);
                int height = stemClip.Height;
                height = (int)(height * 0.85f);
                Vector2 stemStartPosition = position,
                        stemEndPosition = playerCenter;
                if (flowerType == FlowerType.Acalypha) {
                    stemStartPosition += Vector2.UnitX.RotatedBy(flowerRotation) * 4f * flowerInfo.FacedRight.ToDirectionInt();
                }
                Vector2 stemPosition = stemStartPosition;
                Vector2 stemOrigin = stemClip.BottomCenter();
                uint seedForRandomness = (uint)((byte)flowerType + flowerInfo.Offset.Length() * 2f);
                int index = (int)MathUtils.PseudoRandRange(ref seedForRandomness, 100f);
                while (true) {
                    if (Vector2.Distance(stemPosition, stemEndPosition) < height * 0.75f) {
                        break;
                    }
                    Vector2 velocity = stemPosition.DirectionTo(stemEndPosition);
                    velocity = velocity.RotatedBy(Math.Sin(index) * 0.075f);
                    float stemRotation = velocity.ToRotation() - MathHelper.PiOver2;
                    DrawInfo stemDrawInfo = new() {
                        Clip = stemClip,
                        Origin = stemOrigin,
                        Rotation = stemRotation,
                        ImageFlip = flip,
                        Color = color
                    };
                    batch.Draw(leafTexture, stemPosition, stemDrawInfo);
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
                        velocity = velocity.RotatedBy(Math.Sin(index) * 0.05f);
                        float leafRotation = velocity.ToRotation() - MathHelper.PiOver2;
                        bool flipped = index % 2 == 0;
                        SpriteEffects leafFlip = flipped.ToSpriteEffects();
                        DrawInfo leafDrawInfo = new() {
                            Clip = leafClip,
                            Origin = leafOrigin,
                            Rotation = leafRotation,
                            ImageFlip = leafFlip,
                            Color = color
                        };
                        int offsetDirection = flipped.ToDirectionInt();
                        bool shouldDraw = MathUtils.PseudoRandRange(ref seedForRandomness, 1f) < 0.35f;
                        if (shouldDraw) {
                            batch.Draw(leafTexture, leafPosition + Vector2.UnitX.RotatedBy(leafRotation) * -6f * offsetDirection, leafDrawInfo);
                            batch.Draw(leafTexture, leafPosition + Vector2.UnitX.RotatedBy(leafRotation) * 8f * offsetDirection, leafDrawInfo with {
                                Clip = leafClip2
                            });
                        }
                        leafPosition += velocity * height;
                        index++;
                    }
                }
                if (flowerType == FlowerType.Acalypha) {
                    Texture2D leafTexture = leaf3Texture;
                    Rectangle leafClip = Utils.Frame(leafTexture, 1, 2, frameY: 0),
                              leafClip2 = Utils.Frame(leafTexture, 1, 2, frameY: 1);
                    Vector2 leafStartPosition = position + stemStartPosition.DirectionTo(stemEndPosition) * 10f,
                            leafEndPosition = stemEndPosition - stemStartPosition.DirectionTo(stemEndPosition) * 10f;
                    Vector2 leafPosition = leafStartPosition;
                    Vector2 leafOrigin = leafClip.BottomRight();
                    seedForRandomness = (uint)((byte)flowerType + flowerInfo.Offset.Length() * 2f);
                    index = (int)MathUtils.PseudoRandRange(ref seedForRandomness, 100f);
                    int index2 = 0;
                    while (true) {
                        if (Vector2.Distance(leafPosition, leafEndPosition) < height * 0.5f) {
                            break;
                        }
                        Vector2 velocity = leafPosition.DirectionTo(leafEndPosition);
                        velocity = velocity.RotatedBy(Math.Sin(index) * 0.05f);
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
                            Color = color
                        };
                        int offsetDirection = flipped.ToDirectionInt();
                        bool shouldDraw = index2 == 1 || index2 == 2;
                        if (shouldDraw) {
                            float offsetValue = 2f;
                            if (flipped) {
                                offsetValue = leafClip.Width - 2f;
                                offsetValue += 4f * flowerInfo.FacedRight.ToDirectionInt();
                            }
                            Vector2 offset = Vector2.UnitX.RotatedBy(leafRotation) * offsetValue * offsetDirection;
                            DrawInfo leafDrawInfo2 = leafDrawInfo;
                            if (index2 == 1) {
                                leafDrawInfo2 = leafDrawInfo with {
                                    Clip = leafClip2
                                };
                            }
                            batch.Draw(leafTexture, leafPosition + offset, leafDrawInfo2);
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
                        SpriteEffects flip = SpriteEffects.None;
                        Vector2 scale = new(1f, 1f * progress);
                        for (int i = 0; i < 2; i++) {
                            DrawInfo leafDrawInfo = new() {
                                Clip = leafClip,
                                Origin = leafOrigin,
                                Rotation = leafRotation,
                                ImageFlip = flip,
                                Color = color,
                                Scale = scale
                            };
                            bool flipped = flip == SpriteEffects.None;
                            Vector2 leafPosition = stemEndPosition + Vector2.UnitX.RotatedBy(leafRotation) * 8f * flipped.ToDirectionInt();
                            float yOffset = 10f + MathUtils.PseudoRandRange(ref seedForRandomness, 40f);
                            leafPosition -= Vector2.UnitY.RotatedBy(leafRotation) * yOffset;
                            batch.Draw(leafTexture, leafPosition, leafDrawInfo);
                            flip = SpriteEffects.FlipHorizontally;
                        }
                    }
                    drawLeafs();
                }
            }
            drawStem();
            batch.Draw(texture, position, drawInfo);
        }
    }
}
