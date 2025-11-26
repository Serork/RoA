using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Content.Dusts;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class WardenHand : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static byte MAXCOUNT => 3;
    private static ushort TIMELEFT => 9000;
    private static ushort MINTIMELEFT => 25;
    private static float GRASPTIMEINTICKS => 15f;
    private static byte FISTFRAMECOUNT => 3;
    private static float SEEDGOTODISTANCE => 45f;
    private static byte ROOTCOUNT => 6;
    private static float STARTTIME => 20f;
    private static float ANIMATIONTIME => 40f;
    private static float MAXDISTANCEPLAYER => TileHelper.TileSize * 35;

    public enum WardenHandRequstedTextureType : byte {
        Base,
        BaseGlow,
        BaseTop,
        Seed,
        SeedGlow,
        Root
    }

    public readonly record struct RootInfo(float Rotation, RootInfo.RootPartInfo[] RootParts, float SpawnOffset, bool Flip) {
        public enum RootPartType {
            End,
            Mid,
            Start
        }

        public readonly record struct RootPartInfo(RootPartType RootPartType, bool LeftFramed);
    }

    //public enum FingerType : byte {
    //    Pinkie,
    //    Ring,
    //    Middle,
    //    Pointer,
    //    Thumb,
    //    Count
    //}

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)WardenHandRequstedTextureType.Base, ResourceManager.NatureProjectileTextures + "FistOfTheWarden"),
         ((byte)WardenHandRequstedTextureType.BaseGlow, ResourceManager.NatureProjectileTextures + "FistOfTheWarden_Glow"),
         ((byte)WardenHandRequstedTextureType.BaseTop, ResourceManager.NatureProjectileTextures + "FistOfTheWarden_Top"),
         ((byte)WardenHandRequstedTextureType.Seed, ResourceManager.NatureProjectileTextures + "SeedOfTheWarden"),
         ((byte)WardenHandRequstedTextureType.SeedGlow, ResourceManager.NatureProjectileTextures + "SeedOfTheWarden_Glow"),
         ((byte)WardenHandRequstedTextureType.Root, ResourceManager.NatureProjectileTextures + "RootOfTheWarden")];

    private Vector2[] _seedPositions = null!;
    private Vector2 _goToPosition;
    private RootInfo[] _rootData = null!;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float SpawnValue => ref Projectile.localAI[1];
    public ref float RotationValue => ref Projectile.localAI[2];

    public ref float AITimer => ref Projectile.ai[0];
    public ref float AITimer2 => ref Projectile.ai[1];

    public ref Vector2 SeedPosition => ref _seedPositions[0];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    private bool IsMoving => Projectile.velocity.Length() > 1f;
    private bool ShouldUpdatePositionWithVelocity => SeedPosition.Distance(_goToPosition) < 100f;

    private float GetBaseProgress(float offset = 0f) => Ease.SineInOut(MathUtils.Clamp01(1f - (AITimer - GRASPTIMEINTICKS * 1.05f) / (GRASPTIMEINTICKS * 0.95f) + offset));
    private float GetArmProgress() => 1f - GetBaseProgress();
    private float GetRootProgress(float offset = 0f) => Utils.GetLerpValue(STARTTIME * 0.5f + ANIMATIONTIME * (0.4f + offset), STARTTIME * 0.5f + ANIMATIONTIME * (0.8f + offset), AITimer2, true);

    public override void SetStaticDefaults() {
        Projectile.SetTrail(2, 3);
    }

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile);

        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;

        Projectile.penetrate = -1;

        Projectile.manualDirectionChange = true;

        Projectile.Opacity = 0f;

        Projectile.tileCollide = false;

        Projectile.usesLocalNPCImmunity = true;
    }

    public override bool? CanDamage() => true;

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        if (IsMoving) {
            if (Helper.DeathrayHitbox(Projectile.Center - Vector2.UnitY.RotatedBy(Projectile.rotation) * 25f, Projectile.Center + Vector2.UnitY.RotatedBy(Projectile.rotation) * 30f,
                targetHitbox, 65f)) {
                return true;
            }
        }
        else {
            foreach (RootInfo rootInfo in _rootData) {
                float progress = Ease.SineInOut(GetRootProgress(MathUtils.Clamp01(rootInfo.SpawnOffset)));
                if (Helper.DeathrayHitbox(Projectile.Center, Projectile.Center + Vector2.UnitY.RotatedBy(rootInfo.Rotation) * progress * 140f,
                    targetHitbox, 40f)) {
                    return true;
                }
            }
        }

        return false;
    }

    private void UpdateMaxCount(Player player) {
        IEnumerable<Projectile> list2 = TrackedEntitiesSystem.GetTrackedProjectile<WardenHand>(checkProjectile => checkProjectile.SameAs(Projectile) || checkProjectile.owner != player.whoAmI);
        List<Projectile> list = [];
        foreach (Projectile projectile in list2) {
            if (projectile.timeLeft > Projectile.timeLeft) {
                list.Add(projectile);
            }
        }
        if (list.Count >= MAXCOUNT && Projectile.timeLeft > MINTIMELEFT) {
            Projectile.timeLeft = MINTIMELEFT;
        }
    }

    public override void AI() {
        Projectile.localNPCHitCooldown = IsMoving ? 10 : 15;

        Player owner = Projectile.GetOwnerAsPlayer();

        UpdateMaxCount(owner);

        SpawnValue = Helper.Approach(SpawnValue, 1f, 0.1f);

        if (!Init) {
            _seedPositions = new Vector2[Projectile.GetTrailCount()];
        }

        for (int num6 = _seedPositions.Length - 1; num6 > 0; num6--) {
            _seedPositions[num6] = _seedPositions[num6 - 1];
        }

        if (!Init) {
            Init = true;

            float baseSpeed = 15f;
            Projectile.velocity = Projectile.velocity.SafeNormalize() * baseSpeed;

            owner.SyncMousePosition();
            Projectile.Center = owner.GetViableMousePosition();
            Vector2 center = owner.Center;
            if (Projectile.Center.Distance(center) > MAXDISTANCEPLAYER) {
                Projectile.Center = center + center.DirectionTo(Projectile.Center) * MAXDISTANCEPLAYER;
            }

            Projectile.Center -= Projectile.velocity * baseSpeed * 1.5f;

            Projectile.SetDirection(-Projectile.velocity.X.GetDirection());

            _goToPosition = Projectile.Center + Projectile.velocity * baseSpeed * 1.5f;
            SeedPosition = Projectile.Center + Projectile.velocity.SafeNormalize().TurnRight() * new Vector2(Projectile.direction, -Projectile.direction) * 20f;

            if (Projectile.IsOwnerLocal()) {
                _rootData = new RootInfo[ROOTCOUNT];
                for (int i = 0; i < _rootData.Length; i++) {
                    byte rootPartCount = 3;
                    RootInfo.RootPartInfo[] rootParts = new RootInfo.RootPartInfo[rootPartCount];
                    int count = rootParts.Length;
                    for (int k = 0; k < count; k++) {
                        rootParts[k] = new RootInfo.RootPartInfo() {
                            RootPartType = (RootInfo.RootPartType)k,
                            LeftFramed = Main.rand.NextBool()
                        };
                    }
                    _rootData[i] = new RootInfo() {
                        Rotation = (float)i / ROOTCOUNT * MathHelper.TwoPi + Main.rand.NextFloatDirection() * MathHelper.PiOver4 / 4f,
                        RootParts = rootParts,
                        SpawnOffset = Main.rand.NextFloat(0.5f),
                        Flip = Main.rand.NextBool()
                    };
                }
            }
        }

        SeedPosition = Vector2.Lerp(SeedPosition, _goToPosition, 0.1f);

        if (SpawnValue < 1f) {
            return;
        }

        float distance = SeedPosition.Distance(Projectile.Center);
        float neededDistance = SEEDGOTODISTANCE;
        float distanceProgress = distance / neededDistance;
        if (distance < neededDistance) {
            _goToPosition = Projectile.Center;
            SeedPosition = Vector2.Lerp(SeedPosition, _goToPosition, 0.01f + 0.055f * (1f - distanceProgress));
            if (SeedPosition.Distance(_goToPosition) < 5f) {
                SeedPosition = _goToPosition;
            }
        }

        if (!ShouldUpdatePosition()) {
            return;
        }

        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f * Utils.GetLerpValue(0, MINTIMELEFT / 2, Projectile.timeLeft, true), 0.1f);

        float max = GRASPTIMEINTICKS;
        if (AITimer < max * 2f) {
            AITimer++;

            float rotationFactor = 1f - RotationValue;
            float lerpValue = 0.25f + MathUtils.Clamp01(RotationValue - 0.25f);
            float to = -Projectile.velocity.SafeNormalize().X * MathUtils.Clamp01(MathF.Abs(Projectile.velocity.X) + MathF.Abs(Projectile.velocity.Y)) * rotationFactor;
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, -to * 0.75f * rotationFactor * rotationFactor, lerpValue);
        }
        else {
            AITimer2++;
        }
        if (AITimer >= max * 0.5f) {
            RotationValue = MathHelper.Lerp(RotationValue, 1f, 0.025f);
        }
        if (AITimer >= max) {
            float lerpValue2 = 0.1f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, lerpValue2);
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, 0f, lerpValue2);
        }

        float velocityFactor = Ease.QuintIn(MathUtils.Clamp01(Projectile.velocity.Length() / 5f));
        int dustCount = (int)(4 * velocityFactor);
        Color woodColor = new Color(85, 90, 80).ModifyRGB(1.375f);
        Color blueColor = new(35, 105, 230);
        for (int k = 0; k < dustCount; k++) {
            if (Main.rand.NextBool()) {
                continue;
            }
            if (Main.rand.NextBool(3)) {
                continue;
            }
            int dust = Dust.NewDust(Projectile.Center + Main.rand.RandomPointInArea(20f) - Projectile.velocity * 2.5f, 0, 0, Main.rand.NextBool() ? DustID.WoodFurniture : ModContent.DustType<WoodFurniture>(),
                0, 1f, Main.rand.Next(100), woodColor, 1f + Main.rand.NextFloatRange(0.1f));
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = new Vector2(0, Main.rand.NextFloat(6f) * Main.rand.NextFloat(0.25f, 0.9f));
            Main.dust[dust].velocity.X += Main.dust[dust].position.DirectionTo(Projectile.Center).X * Main.rand.NextFloat(0f, 0.75f);
        }

        if (AITimer2 == max * 2.4f) {
            for (int k = 0; k < 20; k++) {
                Vector2 dustPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(Projectile.width, Projectile.height);
                Vector2 dustVelocity = Projectile.Center.DirectionTo(dustPosition).RotatedBy(MathHelper.PiOver2 * Projectile.direction) * Main.rand.NextFloat(3f, 10f) * 0.75f;
                dustVelocity.Y *= 0.5f;
                dustVelocity = dustVelocity.RotatedBy(MathHelper.PiOver4 * -Projectile.direction);
                Dust dust = Dust.NewDustPerfect(dustPosition - Vector2.UnitY * 12f, DustID.TintableDustLighted, dustVelocity, 200, blueColor);
                dust.noGravity = true;
                dust.fadeIn = Main.rand.NextFloat() * 1.2f;
                dust.scale = Main.rand.NextFloat() * Main.rand.NextFloat(1f, 1.25f);
                dust.scale *= 1.75f;
                dust.noLight = true;
            }
        }

        if (Main.rand.NextChance(1f - velocityFactor) && Main.rand.NextBool(3)) {
            int num730 = Dust.NewDust(Projectile.position + new Vector2(-9f + (Projectile.direction < 0 ? -6f : 0f), 35f + 15f * Main.rand.NextFloat()), 30, 8, Main.rand.NextBool() ? DustID.WoodFurniture : ModContent.DustType<WoodFurniture>(),
                0, 1f, Main.rand.Next(100), woodColor, 1f + Main.rand.NextFloatRange(0.1f));
            Main.dust[num730].noGravity = true;
            Main.dust[num730].velocity = new Vector2(0, Main.rand.NextFloat(6f) * Main.rand.NextFloat(0.25f, 0.9f));
            Main.dust[num730].velocity.X += Main.dust[num730].position.DirectionTo(Projectile.Center).X * Main.rand.NextFloat(0.25f, 0.75f);
            Main.dust[num730].velocity.Y *= Main.rand.NextFloat(0.875f, 1f);
        }

        float lightMin = max * 1.6f,
              lightMax = max * 3.4f;
        if (AITimer2 >= lightMin && AITimer2 < lightMax) {
            float baseProgress = Utils.GetLerpValue(lightMin, lightMax, AITimer2, true);
            float progress = MathUtils.YoYo(baseProgress);
            Color baseColor = Color.Lerp(new Color(49, 75, 188), Color.White, 0.75f);
            Vector3 lightColor = Color.Lerp(baseColor, Color.Lerp(baseColor, blueColor, 0.25f), Ease.CubeIn(baseProgress)).ToVector3();
            lightColor *= progress;
            Lighting.AddLight(Projectile.Center - Vector2.UnitY * 12f, lightColor * 1.075f);
        }
        lightMax *= 1f;
        if (AITimer2 > lightMax) {
            float progress = Ease.CubeOut(Utils.GetLerpValue(lightMax, lightMax * 1.5f, AITimer2, true));
            float offset = 0.5f * progress;
            Projectile.position.Y += Helper.Wave(AITimer2 * TimeSystem.LogicDeltaTime, -offset, offset, 5f, 0f);
        }
    }

    public override bool ShouldUpdatePosition() => ShouldUpdatePositionWithVelocity;

    public override void OnKill(int timeLeft) {

    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<WardenHand>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D baseTexture = indexedTextureAssets[(byte)WardenHandRequstedTextureType.Base].Value,
                  seedTexture = indexedTextureAssets[(byte)WardenHandRequstedTextureType.Seed].Value,
                  baseTopTexture = indexedTextureAssets[(byte)WardenHandRequstedTextureType.BaseTop].Value,
                  baseGlowTexture = indexedTextureAssets[(byte)WardenHandRequstedTextureType.BaseGlow].Value,
                  seedGlowTexture = indexedTextureAssets[(byte)WardenHandRequstedTextureType.SeedGlow].Value,
                  rootTexture = indexedTextureAssets[(byte)WardenHandRequstedTextureType.Root].Value;
        SpriteBatch batch = Main.spriteBatch;
        float animationProgress = 1f - GetBaseProgress();
        float startTime = STARTTIME;
        float animationTime = ANIMATIONTIME;
        float seedProgress = 1f - Utils.GetLerpValue(startTime * 0.75f, startTime * 0.75f + animationTime * 0.5f, AITimer2, true);
        float seedProgress2 = Utils.GetLerpValue(animationTime * 0.625f + startTime * 0.875f, startTime * 0.875f, AITimer2, true);
        float seedProgress3 = Utils.GetLerpValue(animationTime / 6f + startTime * 1.25f, startTime * 1.25f, AITimer2, true);
        float seedProgress4 = 1f - Utils.GetLerpValue(animationTime / 5f + startTime * 1.25f, animationTime / 6f + startTime * 1.25f, AITimer2, true);
        float glowProgress = Utils.GetLerpValue(startTime * 1.65f, startTime * 1.65f + animationTime * 0.2f, AITimer2, true);
        float glowProgress2 = Utils.GetLerpValue(startTime * 1.65f + animationTime * 0f, startTime * 1.65f + animationTime * 0.4f, AITimer2, true);
        float glowProgress3 = Utils.GetLerpValue(startTime * 1.65f + animationTime * 0.2f, startTime * 1.65f + animationTime * 0.8f, AITimer2, true);
        seedProgress *= 1f - glowProgress;
        seedProgress2 *= 1f - glowProgress;
        glowProgress *= 1f - glowProgress3;
        byte glowAlpha = (byte)MathHelper.Lerp(100, 255, glowProgress);
        Rectangle baseClip = new SpriteFrame(1, FISTFRAMECOUNT, 0, (byte)(animationProgress * (FISTFRAMECOUNT - 1))).GetSourceRectangle(baseTexture);
        Vector2 baseOrigin = baseClip.Centered();
        SpriteEffects baseEffects = Projectile.spriteDirection.ToSpriteEffects();
        Color baseColor = Lighting.GetColor(Projectile.Center.ToTileCoordinates()) * Projectile.Opacity,
              baseGlowColor = Color.Lerp(new Color(49, 75, 188), Color.White, 0.5f),
              baseGlowColor2 = Color.Lerp(new Color(49, 75, 188), Color.White, 0.1f);
        Vector2 shakeVelocity = Main.rand.RandomPointInArea(1.5f, 3f) * MathUtils.YoYo(1f - glowProgress2) * Ease.CubeIn(1f - glowProgress3);
        Vector2 basePosition = Projectile.Center + shakeVelocity;
        Vector2 baseScale = Vector2.One;
        float baseRotation = Projectile.rotation;

        //int baseCount = 3;
        //for (int baseIndex = 0; baseIndex < 3; baseIndex++) {
        //    float progress = baseIndex / (float)baseCount;
        //    DrawInfo baseDrawInfo = DrawInfo.Default with {
        //        Clip = baseClip,
        //        Origin = baseOrigin,
        //        ImageFlip = baseEffects,
        //        Scale = baseScale * MathHelper.Lerp(1f, 0.25f, progress),
        //        Color = baseColor.ModifyRGB(MathHelper.Lerp(1f, 0.875f, progress))
        //    };
        //    batch.Draw(baseTexture, basePosition, baseDrawInfo);
        //}

        DrawInfo baseDrawInfo = DrawInfo.Default with {
            Clip = baseClip,
            Origin = baseOrigin,
            ImageFlip = baseEffects,
            Scale = baseScale,
            Color = baseColor,
            Rotation = baseRotation
        };

        int trailCount = Projectile.GetTrailCount();
        for (int i = 1; i < trailCount; i++) {
            float maxAlpha = 0.5f;
            float alphaStep = maxAlpha / trailCount;
            batch.Draw(baseTexture, Projectile.oldPos[i] + Projectile.Size / 2f + shakeVelocity, baseDrawInfo with {
                Color = baseColor * (maxAlpha - (i * alphaStep)),
                Rotation = Projectile.oldRot[i]
            });
        }
        batch.Draw(baseTexture, basePosition, baseDrawInfo);

        void drawSeed() {
            Vector2 seedPosition = SeedPosition;
            Rectangle seedClip = seedTexture.Bounds;
            Vector2 seedOrigin = seedClip.Centered();
            float colorOpacity = Ease.CubeOut(seedProgress);
            Color seedColor = Lighting.GetColor(SeedPosition.ToTileCoordinates()) * Projectile.Opacity * colorOpacity;
            float velocityRotation = SeedPosition.DirectionTo(_goToPosition).ToRotation() - MathHelper.PiOver2;
            if (float.IsNaN(velocityRotation)) {
                velocityRotation = 0f;
            }
            float seedRotation = Utils.AngleLerp(velocityRotation, MathHelper.PiOver4 * Projectile.direction, Ease.SineInOut(GetArmProgress()));
            seedPosition += Vector2.UnitX.RotatedBy(seedRotation) * GetArmProgress() * 6f * -Projectile.direction;
            Vector2 seedScale = Vector2.One * Projectile.Opacity;
            DrawInfo seedDrawInfo = DrawInfo.Default with {
                Clip = seedClip,
                Origin = seedOrigin,
                Rotation = seedRotation,
                Color = seedColor,
                ImageFlip = baseEffects,
                Scale = seedScale
            };
            for (int i = 1; i < trailCount; i++) {
                float maxAlpha = 0.5f;
                float alphaStep = maxAlpha / trailCount;
                batch.Draw(seedTexture, _seedPositions[i] + shakeVelocity, seedDrawInfo with {
                    Color = seedColor * (maxAlpha - (i * alphaStep)) * (1f - GetArmProgress())
                });
            }
            batch.Draw(seedTexture, seedPosition, seedDrawInfo);
            Color seedGlowColor = baseGlowColor * (1f - seedProgress) * Ease.CubeOut(seedProgress2) * 0.75f;
            batch.Draw(seedGlowTexture, seedPosition, seedDrawInfo with {
                Color = seedGlowColor with { A = 0 } * Projectile.Opacity,
                Scale = seedScale * (0.75f + MathUtils.YoYo(1f - seedProgress2) * 0.5f),
                ImageFlip = baseEffects
            });
        }

        drawSeed();

        void drawRoots(Func<RootInfo, float> rootProgress, Color baseColor, float opacity = 1f) {
            for (int i = 0; i < _rootData.Length; i++) {
                RootInfo rootInfo = _rootData[i];
                RootInfo.RootPartInfo[] rootParts = rootInfo.RootParts;
                Vector2 rootPosition = basePosition;
                for (int k = 0; k < rootParts.Length; k++) {
                    RootInfo.RootPartInfo rootPart = rootParts[k];
                    Rectangle rootClip;
                    int clipX = 42 * rootPart.LeftFramed.ToInt();
                    switch (rootPart.RootPartType) {
                        case RootInfo.RootPartType.Start:
                            rootClip = new Rectangle(clipX, 2, 42, 52);
                            break;
                        case RootInfo.RootPartType.Mid:
                            rootClip = new Rectangle(clipX, 56, 42, 68);
                            break;
                        default:
                            rootClip = new Rectangle(clipX, 126, 42, 22);
                            break;
                    }
                    ;
                    Vector2 rootOrigin = rootClip.BottomCenter();
                    float rootRotation = rootInfo.Rotation;
                    SpriteEffects rootEffects = rootInfo.Flip.ToInt().ToSpriteEffects();
                    Vector2 rootScale = Vector2.One * new Vector2(Ease.CubeOut(rootProgress(rootInfo)), Ease.SineInOut(rootProgress(rootInfo)));
                    Color rootColor = baseColor * opacity;
                    DrawInfo rootDrawInfo = DrawInfo.Default with {
                        Clip = rootClip,
                        Origin = rootOrigin,
                        Color = rootColor,
                        Rotation = rootRotation + MathHelper.Pi,
                        ImageFlip = rootEffects,
                        Scale = rootScale
                    };
                    batch.Draw(rootTexture, rootPosition, rootDrawInfo);
                    rootPosition += Vector2.UnitY.RotatedBy(rootRotation) * rootClip.Height * Ease.SineInOut(rootProgress(rootInfo));
                }
            }
        }

        drawRoots((rootInfo) => GetRootProgress(-0.08f + MathUtils.Clamp01(rootInfo.SpawnOffset)), baseGlowColor2 with { A = 25 } * Projectile.Opacity, 0.5f);
        drawRoots((rootInfo) => GetRootProgress(MathUtils.Clamp01(rootInfo.SpawnOffset)), baseColor);

        Rectangle baseGlowClip = baseGlowTexture.Bounds;
        Vector2 baseGlowOrigin = baseGlowClip.Centered();
        Vector2 baseGlowPosition = basePosition + new Vector2(-4f * Projectile.direction, -12f);
        baseGlowColor = baseGlowColor with { A = glowAlpha } * glowProgress;

        batch.Draw(baseTopTexture, basePosition, baseDrawInfo);

        batch.Draw(baseGlowTexture, baseGlowPosition, baseDrawInfo with {
            Clip = baseGlowClip,
            Origin = baseGlowOrigin,
            Color = baseGlowColor * Projectile.Opacity,
            ImageFlip = baseEffects
        });

        //batch.Draw(baseTopTexture, basePosition, baseDrawInfo with {
        //    Color = baseDrawInfo.Color * MathUtils.Clamp01(2f * glowProgress)
        //});

        Vector2 baseGlowScale = Vector2.Lerp(Vector2.One, Vector2.One * 2f, glowProgress2) * 1f;
        batch.Draw(baseGlowTexture, baseGlowPosition, baseDrawInfo with {
            Clip = baseGlowClip,
            Origin = baseGlowOrigin,
            Color = baseGlowColor with { A = 100 } * 1.25f * MathUtils.YoYo(1f - glowProgress2) * Projectile.Opacity,
            Scale = baseGlowScale,
            ImageFlip = baseEffects
        });


        //int fingerIndex = 0;
        //Rectangle fingerClip = fingerPartTexture.Bounds;
        //Vector2 fingerOrigin = fingerClip.BottomCenter();
        //int fingerCount = (byte)FingerType.Count;
        //while (fingerIndex < fingerCount) {
        //    float fingerProgress = fingerIndex / (float)fingerCount;
        //    float fingerWaveValue = GetBaseProgress(fingerProgress * 0.5f);
        //    Vector2 fingerOffsetValue = new(fingerPartTexture.Width / 2f, fingerPartTexture.Height);
        //    Vector2 fingerOffset = new Vector2(0f, -0.75f) * fingerOffsetValue;
        //    Vector2 fingerPosition = basePosition + fingerOffset;
        //    float baseFingerRotation = Projectile.rotation + -0.25f * Projectile.direction;
        //    FingerType fingerType = (FingerType)fingerIndex;
        //    switch (fingerType) {
        //        case FingerType.Pointer:
        //            fingerPosition += new Vector2(2f * Projectile.direction, 0.375f) * fingerOffsetValue;
        //            baseFingerRotation += 0.55f * Projectile.direction;
        //            break;
        //        case FingerType.Pinkie:
        //            fingerPosition += new Vector2(-2f * Projectile.direction, 1f) * fingerOffsetValue;
        //            baseFingerRotation -= 0.5f * Projectile.direction;
        //            break;
        //        case FingerType.Ring:
        //            fingerPosition += new Vector2(-1.5f * Projectile.direction, 0.5f) * fingerOffsetValue;
        //            baseFingerRotation -= 0.125f * Projectile.direction;
        //            break;
        //        case FingerType.Middle:
        //            baseFingerRotation += 0.075f * Projectile.direction;
        //            break;
        //        case FingerType.Thumb:
        //            fingerPosition += new Vector2(2f * Projectile.direction, 0.75f) * fingerOffsetValue;
        //            baseFingerRotation += 2.25f * Projectile.direction;
        //            break;
        //    }
        //    float fingerRotation = baseFingerRotation;
        //    int fingerPartCount = 3;
        //    for (int i = 0; i < fingerPartCount; i++) {
        //        float progress = (float)(i + 1) / fingerPartCount;
        //        float rotationIncreaseValue = -MathHelper.Lerp(1f, 0.25f, fingerWaveValue) * Projectile.direction;
        //        float fingerExtraRotation = -0.375f * Projectile.direction;
        //        if (fingerType == FingerType.Thumb) {
        //            rotationIncreaseValue *= MathHelper.Lerp(0.5f, 2f, fingerWaveValue);
        //        }
        //        else if (fingerType == FingerType.Pointer) {
        //            rotationIncreaseValue *= MathHelper.Lerp(1f, 0.5f, 1f - fingerWaveValue);
        //        }
        //        else if (fingerType == FingerType.Middle) {
        //            rotationIncreaseValue *= MathHelper.Lerp(1f, 0.5f, 1f - fingerWaveValue);
        //            fingerExtraRotation *= MathHelper.Lerp(1f, 2f, 1f - fingerWaveValue);
        //        }
        //        else if (fingerType == FingerType.Pinkie || fingerType == FingerType.Ring) {
        //            rotationIncreaseValue *= MathHelper.Lerp(1f, 0.75f, 1f - fingerWaveValue);
        //        }
        //        fingerRotation += rotationIncreaseValue * 2.5f * progress;
        //        Vector2 fingerScale = baseScale * MathHelper.Lerp(1f, 0.75f + 0.25f * (1f - progress), 1f - fingerWaveValue);
        //        DrawInfo fingerDrawInfo = DrawInfo.Default with {
        //            Clip = fingerClip,
        //            Origin = fingerOrigin,
        //            ImageFlip = baseEffects,
        //            Rotation = fingerRotation + fingerExtraRotation,
        //            Scale = fingerScale,
        //            Color = baseColor.ModifyRGB(MathHelper.Lerp(0.875f, 1f, progress))
        //        };
        //        batch.Draw(fingerPartTexture, fingerPosition, fingerDrawInfo);
        //        fingerPosition += (fingerOffset * MathHelper.Lerp(1f, 0.1f, 1f - fingerWaveValue)).RotatedBy(fingerRotation);
        //    }
        //    fingerIndex++;
        //}

        //drawSeed(0.25f);
    }
}
