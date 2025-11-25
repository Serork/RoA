using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Common.Players;
using RoA.Common.Projectiles;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class WardenHand : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 360;
    private static float GRASPTIMEINTICKS => 15f;
    private static byte FISTFRAMECOUNT => 3;
    private static float SEEDGOTODISTANCE => 45f;

    public enum WardenHandRequstedTextureType : byte {
        Base,
        BaseGlow,
        BaseTop,
        Seed,
        SeedGlow
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
         ((byte)WardenHandRequstedTextureType.SeedGlow, ResourceManager.NatureProjectileTextures + "SeedOfTheWarden_Glow")];

    private Vector2 _seedPosition, _goToPosition;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float SpawnValue => ref Projectile.localAI[1];
    public ref float RotationValue => ref Projectile.localAI[2];

    public ref float AITimer => ref Projectile.ai[0];
    public ref float AITimer2 => ref Projectile.ai[1];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
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
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();

        SpawnValue = Helper.Approach(SpawnValue, 1f, 0.1f);

        if (!Init) {
            Init = true;

            float baseSpeed = 15f;
            Projectile.velocity = Projectile.velocity.SafeNormalize() * baseSpeed;

            owner.SyncMousePosition();
            Projectile.Center = owner.GetViableMousePosition() - Projectile.velocity * baseSpeed * 1.5f;

            Projectile.SetDirection(-Projectile.velocity.X.GetDirection());

            _goToPosition = Projectile.Center + Projectile.velocity * baseSpeed * 1.5f;
            _seedPosition = Projectile.Center + Projectile.velocity.SafeNormalize().TurnRight() * new Vector2(Projectile.direction, -Projectile.direction) * 20f;
        }

        _seedPosition = Vector2.Lerp(_seedPosition, _goToPosition, 0.1f);

        if (SpawnValue < 1f) {
            return;
        }

        float distance = _seedPosition.Distance(Projectile.Center);
        float neededDistance = SEEDGOTODISTANCE;
        float distanceProgress = distance / neededDistance;
        if (distance < neededDistance) {
            _goToPosition = Projectile.Center;
            _seedPosition = Vector2.Lerp(_seedPosition, _goToPosition, 0.01f + 0.055f * (1f - distanceProgress));
            if (_seedPosition.Distance(_goToPosition) < 5f) {
                _seedPosition = _goToPosition;
            }
        }

        if (!ShouldUpdatePosition()) {
            return;
        }

        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.1f);

        float rotationFactor = 1f - RotationValue;
        float lerpValue = 0.25f + MathUtils.Clamp01(RotationValue - 0.25f);
        float to = -Projectile.velocity.SafeNormalize().X * MathUtils.Clamp01(MathF.Abs(Projectile.velocity.X) + MathF.Abs(Projectile.velocity.Y)) * rotationFactor;
        Projectile.rotation = Utils.AngleLerp(Projectile.rotation, -to * 0.75f * rotationFactor * rotationFactor, lerpValue);

        float max = GRASPTIMEINTICKS;
        if (AITimer < max * 2f) {
            AITimer++;
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
    }

    public override bool ShouldUpdatePosition() => _seedPosition.Distance(_goToPosition) < 100f;

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
                  seedGlowTexture = indexedTextureAssets[(byte)WardenHandRequstedTextureType.SeedGlow].Value;
        float getBaseProgress(float offset = 0f) => Ease.SineInOut(MathUtils.Clamp01(1f - (AITimer - GRASPTIMEINTICKS * 1.05f) / (GRASPTIMEINTICKS * 0.95f) + offset));
        SpriteBatch batch = Main.spriteBatch;
        float animationProgress = 1f - getBaseProgress();
        float startTime = 20f;
        float animationTime = 40f;
        float seedProgress = 1f - Utils.GetLerpValue(startTime * 0.75f, startTime * 0.75f + animationTime * 0.5f, AITimer2, true);
        float seedProgress2 = Utils.GetLerpValue(animationTime * 0.625f + startTime * 0.875f, startTime * 0.875f, AITimer2, true);
        float seedProgress3 = Utils.GetLerpValue(animationTime / 5f + startTime, startTime, AITimer2, true);
        float seedProgress4 = 1f - Utils.GetLerpValue(animationTime / 3f + startTime * 1.1f, animationTime / 5f + startTime * 1.05f, AITimer2, true);
        float glowProgress = Utils.GetLerpValue(startTime * 1.65f, startTime * 1.65f + animationTime * 0.2f, AITimer2, true);
        float glowProgress2 = Utils.GetLerpValue(startTime * 1.65f + animationTime * 0f, startTime * 1.65f + animationTime * 0.4f, AITimer2, true);
        float glowProgress3 = Utils.GetLerpValue(startTime * 1.65f + animationTime * 0.2f, startTime * 1.65f + animationTime * 0.8f, AITimer2, true);
        seedProgress *= 1f - glowProgress;
        seedProgress2 *= 1f - glowProgress;
        glowProgress *= 1f - glowProgress3;
        byte glowAlpha = (byte)MathHelper.Lerp(100, 255, glowProgress);
        animationProgress *= MathF.Max(seedProgress4, seedProgress3);
        Rectangle baseClip = new SpriteFrame(1, FISTFRAMECOUNT, 0, (byte)(animationProgress * (FISTFRAMECOUNT - 1))).GetSourceRectangle(baseTexture);
        Vector2 baseOrigin = baseClip.Centered();
        SpriteEffects baseEffects = Projectile.spriteDirection.ToSpriteEffects();
        Color baseColor = Color.White * Projectile.Opacity;
        Vector2 basePosition = Projectile.Center + Main.rand.RandomPointInArea(2f, 4f) * MathUtils.YoYo(1f - glowProgress2) * Ease.CubeIn(1f - glowProgress3);
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
        batch.Draw(baseTexture, basePosition, baseDrawInfo);

        void drawSeed() {
            Vector2 seedPosition = _seedPosition;
            Rectangle seedClip = seedTexture.Bounds;
            Vector2 seedOrigin = seedClip.Centered();
            float seedRotation = 0f;
            Color seedColor = baseColor * Ease.CubeOut(seedProgress);
            DrawInfo seedDrawInfo = DrawInfo.Default with {
                Clip = seedClip,
                Origin = seedOrigin,
                Rotation = seedRotation,
                Color = seedColor
            };
            batch.Draw(seedTexture, seedPosition, seedDrawInfo);
            Color seedGlowColor = baseColor * (1f - seedProgress) * Ease.CubeOut(seedProgress2) * 0.75f;
            batch.Draw(seedGlowTexture, seedPosition, seedDrawInfo with {
                Color = seedGlowColor with { A = 0 },
                Scale = Vector2.One * (0.75f + MathUtils.YoYo(1f - seedProgress2) * 0.25f)
            });
        }

        drawSeed();

        batch.Draw(baseTopTexture, basePosition, baseDrawInfo);

        Rectangle baseGlowClip = baseGlowTexture.Bounds;
        Vector2 baseGlowOrigin = baseGlowClip.Centered();
        Vector2 baseGlowPosition = basePosition + new Vector2(-4f * Projectile.direction, -12f);
        Color baseGlowColor = baseColor with { A = glowAlpha } * glowProgress;
        batch.Draw(baseGlowTexture, baseGlowPosition, baseDrawInfo with {
            Clip = baseGlowClip,
            Origin = baseGlowOrigin,
            Color = baseGlowColor
        });
        Vector2 baseGlowScale = Vector2.Lerp(Vector2.One, Vector2.One * 2f, glowProgress2) * 1f;
        batch.Draw(baseGlowTexture, baseGlowPosition, baseDrawInfo with {
            Clip = baseGlowClip,
            Origin = baseGlowOrigin,
            Color = baseGlowColor with { A = 100 } * 1.25f * MathUtils.YoYo(1f - glowProgress2),
            Scale = baseGlowScale
        });

        //int fingerIndex = 0;
        //Rectangle fingerClip = fingerPartTexture.Bounds;
        //Vector2 fingerOrigin = fingerClip.BottomCenter();
        //int fingerCount = (byte)FingerType.Count;
        //while (fingerIndex < fingerCount) {
        //    float fingerProgress = fingerIndex / (float)fingerCount;
        //    float fingerWaveValue = getBaseProgress(fingerProgress * 0.5f);
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
