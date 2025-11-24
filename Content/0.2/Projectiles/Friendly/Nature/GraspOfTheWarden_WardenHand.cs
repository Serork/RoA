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

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class WardenHand : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => 360;
    private static float GRASPTIMEINTICKS => 15f;

    public enum WardenHandRequstedTextureType : byte {
        Base,
        Part1,
        Seed
    }

    public enum FingerType : byte {
        Pinkie,
        Ring,
        Middle,
        Pointer,
        Thumb,
        Count
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)WardenHandRequstedTextureType.Base, ResourceManager.NatureProjectileTextures + "WardenHand_Base"),
         ((byte)WardenHandRequstedTextureType.Part1, ResourceManager.NatureProjectileTextures + "WardenHand_Part1"),
         ((byte)WardenHandRequstedTextureType.Seed, ResourceManager.NatureProjectileTextures + "WardenHand_Seed")];

    private Vector2 _seedPosition, _goToPosition;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float AITimer => ref Projectile.localAI[1];
    public ref float RotationValue => ref Projectile.localAI[2];

    public ref float SpawnValue => ref Projectile.ai[0];

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
            _seedPosition = Projectile.Center + Projectile.velocity.TurnRight() * new Vector2(Projectile.direction, -Projectile.direction) * 20f;
        }

        _seedPosition = Vector2.Lerp(_seedPosition, _goToPosition, 0.1f);

        if (SpawnValue < 1f) {
            return;
        }

        if (_seedPosition.Distance(Projectile.Center) < 50f) {
            _goToPosition = Projectile.Center;
            _seedPosition = Vector2.Lerp(_seedPosition, _goToPosition, 0.2f);
        }

        if (!ShouldUpdatePosition()) {
            return;
        }

        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.1f);

        float max = GRASPTIMEINTICKS;
        if (AITimer < max * 2f) {
            AITimer++;
            float lerpValue = 0.25f + MathUtils.Clamp01(RotationValue - 0.25f);
            float to = -Projectile.velocity.SafeNormalize().X * MathUtils.Clamp01(MathF.Abs(Projectile.velocity.X) + MathF.Abs(Projectile.velocity.Y)) * (1f - RotationValue);
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, to * 0.75f, lerpValue);
        }
        if (AITimer >= max * 0.5f) {
            RotationValue = MathHelper.Lerp(RotationValue, 1f, 0.1f);
        }
        if (AITimer >= max) {
            Projectile.velocity *= 0.9f;
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
                  fingerPartTexture = indexedTextureAssets[(byte)WardenHandRequstedTextureType.Part1].Value,
                  seedTexture = indexedTextureAssets[(byte)WardenHandRequstedTextureType.Seed].Value;
        SpriteBatch batch = Main.spriteBatch;
        Rectangle baseClip = baseTexture.Bounds;
        Vector2 baseOrigin = baseClip.Centered();
        SpriteEffects baseEffects = Projectile.spriteDirection.ToSpriteEffects();
        Color color = Color.White * Projectile.Opacity;
        Vector2 basePosition = Projectile.Center;
        Vector2 baseScale = Vector2.One;
        int baseCount = 3;
        for (int baseIndex = 0; baseIndex < 3; baseIndex++) {
            float progress = baseIndex / (float)baseCount;
            DrawInfo baseDrawInfo = DrawInfo.Default with {
                Clip = baseClip,
                Origin = baseOrigin,
                ImageFlip = baseEffects,
                Scale = baseScale * MathHelper.Lerp(1f, 0.25f, progress),
                Color = color.ModifyRGB(MathHelper.Lerp(1f, 0.875f, progress))
            };
            batch.Draw(baseTexture, basePosition, baseDrawInfo);
        }

        float getBaseProgress(float offset = 0f) => Ease.SineInOut(MathUtils.Clamp01(1f - (AITimer - GRASPTIMEINTICKS * 0.9f) / (GRASPTIMEINTICKS * 1.1f) + offset));

        void drawSeed(float opacity = 1f) {
            Vector2 seedPosition = _seedPosition;
            Rectangle seedClip = seedTexture.Bounds;
            Vector2 seedOrigin = seedClip.Centered();
            float seedRotation = 0f;
            DrawInfo seedDrawInfo = DrawInfo.Default with {
                Clip = seedClip,
                Origin = seedOrigin,
                Rotation = seedRotation
            };
            batch.Draw(seedTexture, seedPosition, seedDrawInfo);
        }

        drawSeed();

        int fingerIndex = 0;
        Rectangle fingerClip = fingerPartTexture.Bounds;
        Vector2 fingerOrigin = fingerClip.BottomCenter();
        int fingerCount = (byte)FingerType.Count;
        while (fingerIndex < fingerCount) {
            float fingerProgress = fingerIndex / (float)fingerCount;
            float fingerWaveValue = getBaseProgress(fingerProgress * 0.5f);
            Vector2 fingerOffsetValue = new(fingerPartTexture.Width / 2f, fingerPartTexture.Height);
            Vector2 fingerOffset = new Vector2(0f, -0.75f) * fingerOffsetValue;
            Vector2 fingerPosition = basePosition + fingerOffset;
            float baseFingerRotation = Projectile.rotation + -0.25f * Projectile.direction;
            FingerType fingerType = (FingerType)fingerIndex;
            switch (fingerType) {
                case FingerType.Pointer:
                    fingerPosition += new Vector2(2f * Projectile.direction, 0.375f) * fingerOffsetValue;
                    baseFingerRotation += 0.55f * Projectile.direction;
                    break;
                case FingerType.Pinkie:
                    fingerPosition += new Vector2(-2f * Projectile.direction, 1f) * fingerOffsetValue;
                    baseFingerRotation -= 0.5f * Projectile.direction;
                    break;
                case FingerType.Ring:
                    fingerPosition += new Vector2(-1.5f * Projectile.direction, 0.5f) * fingerOffsetValue;
                    baseFingerRotation -= 0.125f * Projectile.direction;
                    break;
                case FingerType.Middle:
                    baseFingerRotation += 0.075f * Projectile.direction;
                    break;
                case FingerType.Thumb:
                    fingerPosition += new Vector2(2f * Projectile.direction, 0.75f) * fingerOffsetValue;
                    baseFingerRotation += 2.25f * Projectile.direction;
                    break;
            }
            float fingerRotation = baseFingerRotation;
            int fingerPartCount = 3;
            for (int i = 0; i < fingerPartCount; i++) {
                float progress = (float)(i + 1) / fingerPartCount;
                float rotationIncreaseValue = -MathHelper.Lerp(1f, 0.25f, fingerWaveValue) * Projectile.direction;
                float fingerExtraRotation = -0.375f * Projectile.direction;
                if (fingerType == FingerType.Thumb) {
                    rotationIncreaseValue *= MathHelper.Lerp(0.5f, 2f, fingerWaveValue);
                }
                else if (fingerType == FingerType.Pointer) {
                    rotationIncreaseValue *= MathHelper.Lerp(1f, 0.5f, 1f - fingerWaveValue);
                }
                else if (fingerType == FingerType.Middle) {
                    rotationIncreaseValue *= MathHelper.Lerp(1f, 0.5f, 1f - fingerWaveValue);
                    fingerExtraRotation *= MathHelper.Lerp(1f, 2f, 1f - fingerWaveValue);
                }
                else if (fingerType == FingerType.Pinkie || fingerType == FingerType.Ring) {
                    rotationIncreaseValue *= MathHelper.Lerp(1f, 0.75f, 1f - fingerWaveValue);
                }
                fingerRotation += rotationIncreaseValue * 2.5f * progress;
                Vector2 fingerScale = baseScale * MathHelper.Lerp(1f, 0.75f + 0.25f * (1f - progress), 1f - fingerWaveValue);
                DrawInfo fingerDrawInfo = DrawInfo.Default with {
                    Clip = fingerClip,
                    Origin = fingerOrigin,
                    ImageFlip = baseEffects,
                    Rotation = fingerRotation + fingerExtraRotation,
                    Scale = fingerScale,
                    Color = color.ModifyRGB(MathHelper.Lerp(0.875f, 1f, progress))
                };
                batch.Draw(fingerPartTexture, fingerPosition, fingerDrawInfo);
                fingerPosition += (fingerOffset * MathHelper.Lerp(1f, 0.1f, 1f - fingerWaveValue)).RotatedBy(fingerRotation);
            }
            fingerIndex++;
        }

        drawSeed(0.25f);
    }
}
