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
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class BiedermeierFlower : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static byte FLOWERCOUNTINABOUQUET => (byte)FlowerType.Count + 1 + 3;

    public enum BiedermeierFlowerTextureType : byte {
        Flower,
        Flower_Base,
        Flower_Base2,
        Stem,
        Stem_End,
        Leaf1,
        Leaf2,
        Leaf3,
        Leaf3_Base
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)BiedermeierFlowerTextureType.Flower, ResourceManager.NatureProjectileTextures + "BiedermeierFlower"),
         ((byte)BiedermeierFlowerTextureType.Flower_Base, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Base"),
         ((byte)BiedermeierFlowerTextureType.Flower_Base2, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Base2"),
         ((byte)BiedermeierFlowerTextureType.Stem, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Stem"),
         ((byte)BiedermeierFlowerTextureType.Stem_End, ResourceManager.NatureProjectileTextures + "BiedermeierFlower_Stem_End"),
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

    public enum FlowerLayer : byte {
        First,
        Second,
        Third
    }

    private record struct FlowerInfo(FlowerType FlowerType, FlowerLayer FlowerLayer, Vector2 Position, Vector2 CorePosition, Vector2 ThrowVelocity, Vector2 Offset, float Rotation, float Progress = 0f, float Progress2 = 0f, bool FacedRight = false, bool Released = false, float MoveProgress = 0f, float MoveDirectionY = 0f) {
        public readonly Vector2 GetDestinationPoint(Projectile projectile, float rotation) => projectile.Center + Offset.RotatedBy(rotation);
    
        public readonly float GetMoveProgress(Projectile projectile, float rotation, out int direction) {
            Vector2 a = Position,
                    b = GetDestinationPoint(projectile, rotation);
            float moveProgress = (a - b).Length();
            moveProgress *= 0.1f;
            direction = (a.X - b.X).GetDirection();
            return moveProgress;
        }
    }   

    private FlowerInfo[] _flowerData = null!;
    private bool _active;

    public ref float InitOnSpawnValue => ref Projectile.localAI[0];

    public bool Init {
        get => InitOnSpawnValue == 1f;
        set => InitOnSpawnValue = value.ToInt();
    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    private void MakeTulipDust(FlowerType flowerType, Vector2 position, Vector2 velocity) {
        float offset2 = 10f;
        Vector2 randomOffset = Main.rand.RandomPointInArea(offset2, offset2),
                spawnPosition = position - randomOffset / 2f + randomOffset;

        Dust dust = Dust.NewDustPerfect(position,
                                        ModContent.DustType<Dusts.Tulip2>(),
                                        (spawnPosition - position).SafeNormalize(Vector2.Zero) * 2.5f * Main.rand.NextFloat(1.25f, 1.5f) + velocity,
                                        Scale: Main.rand.NextFloat(1.375f, 1.5f) * 1.25f,
                                        Alpha: (byte)flowerType);
        dust.customData = Main.rand.NextFloatRange(50f);
    }

    public override void AI() {
        void init() {
            if (!Init) {
                Init = true;

                _active = true;

                _flowerData = new FlowerInfo[FLOWERCOUNTINABOUQUET];
                List<FlowerType> flowersInABouquet = [];
                void fillBouquet() {
                    FlowerType flowerTypeToAdd = Main.rand.GetRandomEnumValue<FlowerType>(1);
                    flowersInABouquet.Add(flowerTypeToAdd);
                    for (int i = 0; i < 2; i++) {
                        flowerTypeToAdd = Main.rand.GetRandomEnumValue<FlowerType>(1);
                        while (flowersInABouquet.Contains(flowerTypeToAdd)) {
                            flowerTypeToAdd = Main.rand.GetRandomEnumValue<FlowerType>(1);
                        }
                        flowersInABouquet.Add(flowerTypeToAdd);
                    }
                }
                fillBouquet();
                while (!flowersInABouquet.Contains(FlowerType.Perfect1) &&
                       !flowersInABouquet.Contains(FlowerType.Perfect2) &&
                       !flowersInABouquet.Contains(FlowerType.Perfect3)) {
                    flowersInABouquet.Clear();
                    fillBouquet();
                }
                int index = 0;
                while (index < FLOWERCOUNTINABOUQUET) {
                    FlowerType firstFlowerTypeInABouquet = flowersInABouquet[0],
                               secondFlowerTypeInABouquet = flowersInABouquet[1],
                               thirdFlowerTypeInABouquet = flowersInABouquet[2];
                    FlowerType flowerInABouquetToAdd = firstFlowerTypeInABouquet;
                    Vector2 offset = Vector2.Zero;
                    FlowerLayer flowerLayer = FlowerLayer.First;
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
                            flowerLayer = FlowerLayer.Second;
                            break;
                        case 6:
                            offset = new Vector2(-30f, -130f);
                            flowerInABouquetToAdd = secondFlowerTypeInABouquet;
                            flowerLayer = FlowerLayer.Second;
                            break;
                        case 7:
                            offset = new Vector2(20f, -105f);
                            flowerInABouquetToAdd = secondFlowerTypeInABouquet;
                            flowerLayer = FlowerLayer.Second;
                            break;
                        case 8:
                            offset = new Vector2(-20f, -105f);
                            flowerInABouquetToAdd = secondFlowerTypeInABouquet;
                            flowerLayer = FlowerLayer.Second;
                            break;
                        case 9:
                            offset = new Vector2(15f, -125f);
                            flowerInABouquetToAdd = thirdFlowerTypeInABouquet;
                            flowerLayer = FlowerLayer.Third;
                            break;
                        case 10:
                            offset = new Vector2(-15f, -125f);
                            flowerInABouquetToAdd = thirdFlowerTypeInABouquet;
                            flowerLayer = FlowerLayer.Third;
                            break;
                        case 11:
                            offset = new Vector2(0f, -135f);
                            flowerInABouquetToAdd = thirdFlowerTypeInABouquet;
                            flowerLayer = FlowerLayer.Third;
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
                    _flowerData[index] = new FlowerInfo(flowerInABouquetToAdd, flowerLayer, Projectile.Center, Projectile.Center, Vector2.Zero, offset, rotation, FacedRight: Main.rand.NextBool());
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

            if (_active) {
                Projectile.velocity = vector4;
            }
            Projectile.Center = player.GetPlayerCorePoint();

            int flowerCount = _flowerData.Length;
            if (!_active) {
                Projectile.Opacity = Helper.Approach(Projectile.Opacity, 0f, TimeSystem.LogicDeltaTime * 2f);
                if (Projectile.Opacity <= 0f) {
                    Projectile.Kill();
                }

                for (int i = 0; i < flowerCount; i++) {
                    int currentSegmentIndex = i;
                    ref FlowerInfo currentSegmentData = ref _flowerData[currentSegmentIndex];
                    float baseRotation = Projectile.rotation;
                    currentSegmentData.CorePosition += currentSegmentData.ThrowVelocity;
                    currentSegmentData.Position += currentSegmentData.ThrowVelocity;

                    currentSegmentData.Progress2 = Helper.Approach(currentSegmentData.Progress2, 0f, 0.1f);

                    //float rotation = currentSegmentData.ThrowVelocity.ToRotation();
                    //int direction = currentSegmentData.ThrowVelocity.X.GetDirection();
                    //float speed = 2f;
                    //currentSegmentData.CorePosition -= Vector2.UnitY.RotatedBy(0f) * speed;
                    //currentSegmentData.Position -= Vector2.UnitY.RotatedBy(MathHelper.PiOver2 * direction) * speed;

                    currentSegmentData.ThrowVelocity *= 0.99f;
                    currentSegmentData.ThrowVelocity = new Vector2(currentSegmentData.ThrowVelocity.X, currentSegmentData.ThrowVelocity.Y + 0.2f);
                }
            }
            else {
                for (int i = 0; i < flowerCount; i++) {
                    int currentSegmentIndex = i;
                    ref FlowerInfo currentSegmentData = ref _flowerData[currentSegmentIndex];
                    float baseRotation = Projectile.rotation;
                    float moveProgress = currentSegmentData.GetMoveProgress(Projectile, baseRotation, out int direction);
                    float lerpValue = 0.25f * Ease.QuintOut(MathUtils.Clamp01(currentSegmentData.Progress));
                    currentSegmentData.MoveDirectionY = MathHelper.Lerp(currentSegmentData.MoveDirectionY, (Projectile.Center - currentSegmentData.Position).SafeNormalize().Y, lerpValue * 2f);
                    moveProgress *= direction * currentSegmentData.MoveDirectionY;
                    float maxValue = 1.25f;
                    moveProgress = Utils.Clamp(moveProgress, -maxValue, maxValue);
                    currentSegmentData.MoveProgress = MathHelper.Lerp(currentSegmentData.MoveProgress, moveProgress, lerpValue);
                    currentSegmentData.CorePosition = Projectile.GetOwnerAsPlayer().GetPlayerCorePoint() - Projectile.velocity * 10f;
                }
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + num;
            Projectile.spriteDirection = Projectile.direction;
            Projectile.timeLeft = 2;
            if (_active) {
                player.ChangeDir(Projectile.direction);
                player.heldProj = Projectile.whoAmI;
                player.SetDummyItemTime(num2);
                player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(Projectile.velocity.Y * (float)Projectile.direction, Projectile.velocity.X * (float)Projectile.direction) + num3);
                player.SetCompositeBothArms(player.itemRotation - MathHelper.PiOver2 * Projectile.spriteDirection, Player.CompositeArmStretchAmount.Full);
            }
        }
        void processFlowers() {
            float allProgress = 0f;
            int flowerCount = _flowerData.Length;
            for (int i = 0; i < flowerCount; i++) {
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Max(0, i - 1);
                ref FlowerInfo currentSegmentData = ref _flowerData[currentSegmentIndex],
                               previousSegmentData = ref _flowerData[previousSegmentIndex];
                float flowerRotation = currentSegmentData.Rotation,
                      baseRotation = Projectile.rotation;
                float positionLerpValue = 0.15f;
                if (_active) {
                    currentSegmentData.Position = Vector2.Lerp(currentSegmentData.Position, currentSegmentData.GetDestinationPoint(Projectile, baseRotation), positionLerpValue);
                    currentSegmentData.Position += Projectile.GetOwnerAsPlayer().velocity;
                }
                allProgress += currentSegmentData.Progress;
                if (currentSegmentIndex > 0 && previousSegmentData.Progress < 0.25f) {
                    continue;
                }
                float lerpValue = 0.05f;
                currentSegmentData.Progress = Helper.Approach(currentSegmentData.Progress, 1f, lerpValue);
            }
            float firstLayerProgress = 0f,
                  secondLayerProgress = 0f;
            int firstLayerCount = 0,
                secondLayerCount = 0;
            for (int i = 0; i < flowerCount; i++) {
                int currentSegmentIndex = i;
                ref FlowerInfo currentSegmentData = ref _flowerData[currentSegmentIndex];
                if (currentSegmentData.FlowerLayer == FlowerLayer.First) {
                    firstLayerProgress += currentSegmentData.Progress2;
                    firstLayerCount++;
                }
                if (currentSegmentData.FlowerLayer == FlowerLayer.Second) {
                    secondLayerProgress += currentSegmentData.Progress2;
                    secondLayerCount++;
                }
            }
            float allProgress2 = 0f;
            float attackStartThreshhold = 1f;
            if (allProgress >= flowerCount * 0.825f * attackStartThreshhold) {
                for (int i = 0; i < flowerCount; i++) {
                    int currentSegmentIndex = i,
                        previousSegmentIndex = Math.Max(0, i - 1);
                    ref FlowerInfo currentSegmentData = ref _flowerData![currentSegmentIndex],
                                   previousSegmentData = ref _flowerData[previousSegmentIndex];
                    float betweenLayerDelay = 2f;
                    if (currentSegmentData.FlowerLayer == FlowerLayer.Second && firstLayerProgress < firstLayerCount * betweenLayerDelay) {
                        continue;
                    }
                    if (currentSegmentData.FlowerLayer == FlowerLayer.Third && secondLayerProgress < secondLayerCount * betweenLayerDelay) {
                        continue;
                    }
                    if (currentSegmentIndex > 0 && previousSegmentData.Progress2 < 0.25f) {
                        continue;
                    }
                    float lerpValue = 0.075f;
                    currentSegmentData.Progress2 = Helper.Approach(currentSegmentData.Progress2, 3f, lerpValue);
                    if (currentSegmentData.Progress2 >= 1.5f * attackStartThreshhold && !currentSegmentData.Released) {
                        Vector2 flowerPosition = Projectile.Center + currentSegmentData.Offset.RotatedBy(Projectile.rotation) * 1.15f;
                        Vector2 velocity = Vector2.UnitY.RotatedBy(Projectile.rotation + MathHelper.Pi);
                        if (Projectile.IsOwnerLocal()) {
                            ProjectileUtils.SpawnPlayerOwnedProjectile<BiedermeierPetal>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromAI()) {
                                Position = flowerPosition,
                                Velocity = velocity,
                                Damage = Projectile.damage,
                                KnockBack = Projectile.knockBack,
                                AI0 = (float)currentSegmentData.FlowerType,
                                AI1 = currentSegmentData.Rotation + MathHelper.PiOver2
                            });
                        }
                        float offsetModifier = 1.5f;
                        if (currentSegmentData.FlowerLayer == FlowerLayer.Third) {
                            offsetModifier = 1.425f;
                        }
                        if (currentSegmentData.FlowerLayer == FlowerLayer.Second) {
                            offsetModifier = 1.4f;
                        }
                        flowerPosition = Projectile.Center + currentSegmentData.Offset.RotatedBy(Projectile.rotation) * offsetModifier;
                        for (int k = 0; k < 6; k++) {
                            MakeTulipDust(currentSegmentData.FlowerType, flowerPosition, velocity * 0.5f);
                        }
                        currentSegmentData.Released = true;
                    }
                    allProgress2 += currentSegmentData.Progress2;
                }
            }
            if (allProgress2 >= 3f * flowerCount) {
                if (_active) {
                    for (int i = 0; i < flowerCount; i++) {
                        int currentSegmentIndex = i;
                        ref FlowerInfo currentSegmentData = ref _flowerData[currentSegmentIndex];
                        currentSegmentData.ThrowVelocity = Projectile.velocity.RotatedBy(MathHelper.PiOver4 * Main.rand.NextFloatDirection() * 0.75f) 
                            * Main.rand.NextFloat(4f, 6f) * 1f;
                        currentSegmentData.ThrowVelocity = new Vector2(currentSegmentData.ThrowVelocity.X, currentSegmentData.ThrowVelocity.Y + 1f);
                        currentSegmentData.Progress2 = Main.rand.NextFloat(1f, 2f);
                    }
                    //_throwVelocity = Projectile.velocity * 5f;
                }
                _active = false;
                //Projectile.Kill();
            }
        }
        void addLight() {
            foreach (FlowerInfo flowerInfo in _flowerData) {
                FlowerType currentType = flowerInfo.FlowerType;
                Vector2 flowerPosition = Projectile.Center + flowerInfo.Offset.RotatedBy(Projectile.rotation) * 1.15f;
                if (currentType == FlowerType.Perfect3) {
                    float num6 = (float)Main.rand.Next(90, 111) * 0.015f;
                    num6 *= Main.essScale;
                    num6 *= MathUtils.Clamp01(flowerInfo.Progress - Utils.GetLerpValue(2.5f, 3f, flowerInfo.Progress2, true));
                    Lighting.AddLight((int)flowerPosition.X / 16, (int)flowerPosition.Y / 16, 0.1f * num6, 0.1f * num6, 0.6f * num6);
                }
                else if (currentType == FlowerType.Perfect1) {
                    float num5 = (float)Main.rand.Next(90, 111) * 0.015f;
                    num5 *= Main.essScale;
                    num5 *= MathUtils.Clamp01(flowerInfo.Progress - Utils.GetLerpValue(2.5f, 3f, flowerInfo.Progress2, true));
                    Lighting.AddLight((int)flowerPosition.X / 16, (int)flowerPosition.Y / 16, 0.5f * num5, 0.3f * num5, 0.05f * num5);
                }
                else if (currentType == FlowerType.Perfect2) {
                    float num8 = (float)Main.rand.Next(90, 111) * 0.015f;
                    num8 *= Main.essScale;
                    num8 *= MathUtils.Clamp01(flowerInfo.Progress - Utils.GetLerpValue(2.5f, 3f, flowerInfo.Progress2, true));
                    Lighting.AddLight((int)flowerPosition.X / 16, (int)flowerPosition.Y / 16, 0.1f * num8, 0.5f * num8, 0.2f * num8);
                }
            }
        }

        init();
        setPosition();
        processFlowers();
        addLight();
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<BiedermeierFlower>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D texture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Flower].Value,
                  texture_Base = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Flower_Base].Value,
                  texture_Base2 = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Flower_Base2].Value;
        SpriteBatch batch = Main.spriteBatch;
        IEnumerable<FlowerInfo> sortedFlowerData = _flowerData.OrderBy(x => x.Offset.Y);
        Texture2D stemTexture = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Stem].Value,
                  stemTexture_End = indexedTextureAssets[(byte)BiedermeierFlowerTextureType.Stem_End].Value,
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
            float opacity = Utils.GetLerpValue(0f, 0.2f, progress, true) * Projectile.Opacity;
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
            //flowerClip.Height = (int)(clip.Height * (1f - MathUtils.Clamp01(progress2 - 1f)));
            float stemGlowScaleFactor = 1f + (0.25f * opacity * (1f - progress5));
            Vector2 flowerScale = scale * (1f + 0.25f * Ease.CubeIn(Utils.GetLerpValue(0.75f, 1.5f, progress2, true)) * (Utils.GetLerpValue(2.75f, 2f, progress2, true)));
            float progress3 = progress2;
            progress2 = MathUtils.Clamp01(progress2);
            float progress4 = progress2;
            //progress2 *= Utils.GetLerpValue(3f, 2f, progress3, true);
            Vector2 playerCenter = flowerInfo.CorePosition;
            Vector2 position = Vector2.Lerp(playerCenter, flowerInfo.Position, MathF.Max(0.1f, progress));
            if (!_active) {
                position = flowerInfo.Position;
                opacity *= MathUtils.Clamp01(progress2);
            }
            Vector2 position2 = Vector2.Lerp(playerCenter, flowerInfo.GetDestinationPoint(Projectile, baseRotation), MathF.Max(0.1f, progress));
            Color lightColor2 = Lighting.GetColor(position.ToTileCoordinates());
            Color baseColor = lightColor2 * opacity,
                  flowerColor = baseColor;
            if (flowerInfo.FlowerType >= FlowerType.Perfect1 && flowerInfo.FlowerType <= FlowerType.Perfect3) {
                flowerColor = TulipPetalSoul.SoulColor2;
            }
            flowerColor *= opacity;
            Color stemGlowColor = baseColor with { A = 100 } * 1f;
            stemGlowColor = Color.Lerp(stemGlowColor, baseColor, progress5);
            stemGlowColor *= opacity;
            if (!_active) {
                stemGlowColor *= 0f;
            }
            DrawInfo drawInfo = new() {
                Clip = flowerClip,
                Origin = origin,
                Rotation = rotation + MathHelper.Pi,
                ImageFlip = flip,
                Color = flowerColor,
                Scale = flowerScale
            };
            //flowerClip.Height = clip.Height;
            DrawInfo drawInfo2 = drawInfo with {
                Color = baseColor
            };
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
                Vector2 stemPosition = stemStartPosition;
                Vector2 stemOrigin = stemClip.BottomCenter();
                uint seedForRandomness = (uint)((byte)flowerType + flowerInfo.Offset.Length() * 2f);
                float index = 2.5f;
                int index3 = (int)MathUtils.PseudoRandRange(ref seedForRandomness, 100f);
                int baseIndex3 = index3;
                float baseIndex = index;
                int attempts = 100;
                Vector2 velocity = stemPosition.DirectionTo(stemEndPosition);
                while (attempts-- > 0) {
                    if (_active) {
                        if (Vector2.Distance(stemPosition, Projectile.Center) > Vector2.Distance(stemPosition, stemEndPosition)) {
                            break;
                        }
                    }
                    else {
                        if (Vector2.Distance(stemPosition, stemEndPosition) < height * 0.5f) {
                            break;
                        }
                    }
                    float moveOffset = 2f * flowerInfo.MoveProgress;
                    if (_active) {
                        velocity = velocity.RotatedBy(Math.Sin(index) * sineOffset * moveOffset);
                    }
                    velocity = velocity.RotatedBy(Math.Sin(index3) * sineOffset);
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
                    Texture2D texture = stemTexture;
                    if (Vector2.Distance(stemPosition, stemEndPosition) < height * 2f) {
                        texture = stemTexture_End;
                    }
                    batch.Draw(texture, stemPosition, stemDrawInfo);
                    //if (progress2 >= progress) 
                    {
                        float opacity2 = Utils.GetLerpValue(progress * 0.75f, progress, progress2, true);
                        batch.Draw(texture, stemPosition, stemDrawInfo.WithScaleX(stemGlowScaleFactor) with {
                            Color = stemGlowColor * opacity2
                        });
                    }
                    if (Vector2.Distance(stemPosition, stemEndPosition) < height * 2f) {
                        velocity += stemPosition.DirectionTo(stemEndPosition);
                        velocity = velocity.SafeNormalize();
                    }
                    stemPosition += velocity * height;
                    index++;
                    index3++;
                }
                if (progress >= 0.3f) {
                    if (flowerType == FlowerType.Custom) {
                        Texture2D leafTexture = leaf2Texture;
                        Rectangle leafClip = Utils.Frame(leafTexture, 2, 1, frameX: 0),
                                  leafClip2 = Utils.Frame(leafTexture, 2, 1, frameX: 1);
                        Vector2 leafStartPosition = position + stemStartPosition.DirectionTo(stemEndPosition) * 20f,
                                leafEndPosition = stemEndPosition;
                        Vector2 leafPosition = leafStartPosition;
                        Vector2 leafOrigin = leafClip.BottomCenter();
                        seedForRandomness = (uint)((byte)flowerType + flowerInfo.Offset.Length() * 2f);
                        index = baseIndex;
                        index3 = baseIndex3;
                        int attempts2 = 100;
                        Vector2 velocity2 = leafPosition.DirectionTo(leafEndPosition);
                        while (attempts2-- > 0) {
                            if (_active) {
                                if (Vector2.Distance(leafPosition, Projectile.Center) > Vector2.Distance(leafPosition, leafEndPosition)) {
                                    break;
                                }
                            }
                            if (Vector2.Distance(leafPosition, leafEndPosition) < height * 1f) {
                                break;
                            }
                            float moveOffset = 2f * flowerInfo.MoveProgress;
                            if (_active) {
                                velocity2 = velocity2.RotatedBy(Math.Sin(index) * sineOffset * moveOffset);
                            }
                            velocity2 = velocity2.RotatedBy(Math.Sin(index3) * sineOffset);
                            float leafRotation = velocity2.ToRotation() - MathHelper.PiOver2;
                            bool flipped = (int)index % 2 == 0;
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
                            leafPosition += velocity2 * height;
                            index++;
                            index3++;
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
                        index = baseIndex;
                        index3 = baseIndex3;
                        int index2 = 0;
                        int attempts2 = 100;
                        Vector2 velocity2 = leafPosition.DirectionTo(leafEndPosition);
                        while (attempts2-- > 0) {
                            if (Vector2.Distance(leafPosition, leafEndPosition) < height * 2f) {
                                break;
                            }
                            float moveOffset = 2f * flowerInfo.MoveProgress;
                            if (_active) {
                                velocity2 = velocity2.RotatedBy(Math.Sin(index) * sineOffset * moveOffset);
                            }
                            velocity2 = velocity2.RotatedBy(Math.Sin(index3) * sineOffset);
                            float leafRotation = velocity2.ToRotation() - MathHelper.PiOver2;
                            bool flipped = (int)index % 4 == 0;
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
                                float offsetValue = -0f;
                                if (flipped) {
                                    offsetValue = leafClip.Width - 2f;
                                    offsetValue += 2f * flowerInfo.FacedRight.ToDirectionInt();
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
                            leafPosition += velocity2 * height;
                            index++;
                            index2++;
                            index3++;
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
                                float yOffset = 10f + MathUtils.PseudoRandRange(ref seedForRandomness, 30f);
                                leafPosition -= Vector2.UnitY.RotatedBy(leafRotation) * yOffset;
                                leafPosition += Vector2.UnitY.RotatedBy(leafRotation) * 5f;
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
            }
            drawStem();
            //position += Vector2.UnitY.RotatedBy(drawInfo.Rotation) * 30f;
            float moveOffset = 0.5f * flowerInfo.MoveProgress;
            if (_active) {
                drawInfo = drawInfo.WithRotation(moveOffset);
                drawInfo2 = drawInfo2.WithRotation(moveOffset);
            }
            if (!flowerInfo.Released) {
                batch.Draw(texture, position, drawInfo);
            }
            else { 
                batch.Draw(texture_Base2, position, drawInfo2);
                if (!flowerInfo.Released && progress2 >= 1f) {
                    batch.Draw(texture_Base2, position, drawInfo2.WithScaleX(stemGlowScaleFactor) with {
                        Color = stemGlowColor * 0.5f
                    });
                }
            }
            if (!flowerInfo.Released && progress2 >= 1f) {
                batch.Draw(texture_Base, position, drawInfo2.WithScaleX(stemGlowScaleFactor) with {
                    Color = stemGlowColor * 0.5f
                });
            }
        }
    }
}
