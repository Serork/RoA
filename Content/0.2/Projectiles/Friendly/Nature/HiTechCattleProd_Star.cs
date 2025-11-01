using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Core;
using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class HiTechStar : NatureProjectile, IRequestAssets {
    private static byte MAXBEAMCOUNT => 40;
    public static byte SLASHCOUNT = 30;

    public record struct HiTechBeamInfo(float Rotation, float Opacity);

    public enum HiTechStarRequstedTextureType : byte {
        Beam,
        StarPart
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)HiTechStarRequstedTextureType.Beam, ResourceManager.NatureProjectileTextures + "HiTechCattleProd_Beam"),
         ((byte)HiTechStarRequstedTextureType.StarPart, ResourceManager.NatureProjectileTextures + "HiTechCattleProd_Star2")];

    private readonly HiTechBeamInfo[] _hiTechBeams = new HiTechBeamInfo[MAXBEAMCOUNT];
    private int _currentTechBeam;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float BeamCounter => ref Projectile.localAI[1];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public override string Texture => ResourceManager.NatureProjectileTextures + "HiTechCattleProd_Star";

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.timeLeft = 180;

        Projectile.tileCollide = false;

        Projectile.penetrate = -1;

        Projectile.Opacity = 0f;
    }

    public override void AI() {
        if (!Init) {
            Init = true;
            for (int i = 0; i < _hiTechBeams.Length; i++) {
                _hiTechBeams[i] = new HiTechBeamInfo {
                    Rotation = MathHelper.TwoPi * Main.rand.NextFloatDirection()
                };
            }

            Player owner = Projectile.GetOwnerAsPlayer();
            if (Projectile.IsOwnerLocal()) {
                ProjectileUtils.SpawnPlayerOwnedProjectile<HiTechSlash>(new ProjectileUtils.SpawnProjectileArgs(owner, Projectile.GetSource_FromAI()) {
                    Damage = Projectile.damage,
                    KnockBack = Projectile.knockBack,
                    Position = Projectile.Center,
                    Velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 5f,
                    AI0 = Projectile.Center.X,
                    AI1 = Projectile.Center.Y
                });
            }
        }
        else {
            for (int i = 0; i < _hiTechBeams.Length; i++) {
                ref HiTechBeamInfo hiTechBeamInfo = ref _hiTechBeams[i];
                hiTechBeamInfo.Opacity = Helper.Approach(hiTechBeamInfo.Opacity, 0f, TimeSystem.LogicDeltaTime * 2.5f);      
            }
        }

        Projectile.velocity *= 0f;

        Projectile.Opacity = Helper.Approach(Projectile.Opacity, 1f, 0.1f) * Utils.GetLerpValue(0, 10, Projectile.timeLeft, true);

        BeamCounter++;
        if (BeamCounter >= 15f && BeamCounter <= 60f) {
            if (BeamCounter % 2f == 0f) {
                ActivateBeam();
            }
        }
        if (BeamCounter > 60f) {
            BeamCounter = 0f;
        }
    }

    public void ActivateBeam() {
        ref HiTechBeamInfo hiTechBeamInfo = ref _hiTechBeams[_currentTechBeam];
        if (hiTechBeamInfo.Opacity <= 0f) {
            hiTechBeamInfo.Opacity = 1.25f;
        }
        else {
            _currentTechBeam++;
        }
        if (_currentTechBeam >= _hiTechBeams.Length) {
            _currentTechBeam = 0;
        }
    }

    public override bool PreDraw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<HiTechStar>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return false;
        }

        Texture2D starTexture = Projectile.GetTexture(),
                  beamTexture = indexedTextureAssets[(byte)HiTechStarRequstedTextureType.Beam].Value,
                  starPartTexture = indexedTextureAssets[(byte)HiTechStarRequstedTextureType.StarPart].Value;
        SpriteBatch batch = Main.spriteBatch;
        Vector2 position = Projectile.Center;
        Rectangle starClip = starTexture.Bounds;
        Vector2 starOrigin = starClip.Centered();
        Color starColor = Color.White;
        starColor.A = 255;
        float waveSpeed = 20f;
        Vector2 baseScale = Vector2.One * Projectile.Opacity;
        Vector2 starScale = baseScale * Helper.Wave(0.75f, 1.1f, waveSpeed, Projectile.whoAmI);

        Texture2D circle = ResourceManager.Circle3;
        Rectangle circleClip = circle.Bounds;
        Vector2 circleOrigin = circleClip.Centered();
        Color circleColor = starColor.MultiplyRGB(new Color(97, 200, 225)) * 0.95f;
        float mainWave = Helper.Wave(0.975f, 1.025f, waveSpeed, Projectile.whoAmI);
        Vector2 circleScale = baseScale * 2f * mainWave;

        Texture2D circle2 = ResourceManager.Circle2;
        Rectangle circle2Clip = circle2.Bounds;
        Vector2 circle2Origin = circle2Clip.Centered();
        Color circle2Color = circleColor * 0.45f;
        Vector2 circle2Scale = circleScale * 0.65f;

        for (int i = 0; i < _hiTechBeams.Length; i++) {
            HiTechBeamInfo hiTechBeamInfo = _hiTechBeams[i];
            if (hiTechBeamInfo.Opacity <= 0f) {
                continue;
            }
            Rectangle beamClip = beamTexture.Bounds;
            Vector2 beamOrigin = beamClip.BottomCenter();
            Color beamColor = Color.Lerp(Color.White, circleColor * 2f, 0.5f);
            beamColor.A = 100;
            beamColor *= Ease.CircIn(MathUtils.Clamp01(hiTechBeamInfo.Opacity));
            Vector2 beamScale = baseScale * mainWave * new Vector2(1f, 0.75f) * 0.75f;
            Vector2 beamPosition = position;
            float beamRotation = hiTechBeamInfo.Rotation;
            for (int i2 = 0; i2 < 3; i2++) {
                batch.Draw(beamTexture, beamPosition, DrawInfo.Default with {
                    Clip = beamClip,
                    Origin = beamOrigin,
                    Color = beamColor * Projectile.Opacity,
                    Scale = beamScale,
                    Rotation = beamRotation
                });
                batch.DrawWithSnapshot(() => {
                    batch.Draw(beamTexture, beamPosition, DrawInfo.Default with {
                        Clip = beamClip,
                        Origin = beamOrigin,
                        Color = beamColor * 0.5f * Projectile.Opacity,
                        Scale = beamScale * 1.25f,
                        Rotation = beamRotation
                    });
                });
            }
        }

        for (int i = 0; i < 8; i++) {
            SpriteFrame starPartFrame = new(1, 8, 0, (byte)i);
            Rectangle starPartClip = starPartFrame.GetSourceRectangle(starPartTexture);
            Vector2 starPartSize = starPartClip.Size();
            Vector2 starPartOrigin = Vector2.One.RotatedBy(MathHelper.TwoPi / 8 * i) * starPartSize + starPartSize * 0.5f;
            Color starPartColor = Color.White;
            starColor.A = 255;
            Vector2 starPartPosition = position - starOrigin / 4f + new Vector2(-3f, -3f) + starPartOrigin * 0.625f;
            ulong partSeed = (ulong)(i * i * Projectile.GetHashCode() / 2f + Projectile.position.GetHashCode() + Projectile.whoAmI);
            Vector2 starPartScale = Vector2.One * Helper.Wave(0.75f, 0.9f, waveSpeed, i * MathF.Pow(Utils.RandomInt(ref partSeed, 100), 2));
            batch.Draw(starPartTexture, starPartPosition, DrawInfo.Default with {
                Clip = starPartClip,
                Origin = starPartOrigin,
                Color = starPartColor * Projectile.Opacity,
                Scale = starPartScale
            });
            batch.DrawWithSnapshot(() => {
                batch.Draw(starPartTexture, starPartPosition, DrawInfo.Default with {
                    Clip = starPartClip,
                    Origin = starPartOrigin,
                    Color = starPartColor * 0.5f * Projectile.Opacity,
                    Scale = starPartScale * 1.25f
                });
            }, blendState: BlendState.Additive);
        }

        batch.Draw(starTexture, position, DrawInfo.Default with {
            Clip = starClip,
            Origin = starOrigin,
            Color = starColor * Projectile.Opacity,
            Scale = starScale
        });
        batch.DrawWithSnapshot(() => {
            batch.Draw(starTexture, position, DrawInfo.Default with {
                Clip = starClip,
                Origin = starOrigin,
                Color = starColor * 0.5f * Projectile.Opacity,
                Scale = starScale * 1.25f
            });
        }, blendState: BlendState.Additive);

        batch.DrawWithSnapshot(() => {
            batch.Draw(circle2, position, DrawInfo.Default with {
                Origin = circle2Origin,
                Clip = circle2Clip,
                Color = circle2Color * Projectile.Opacity,
                Scale = circle2Scale
            });
        }, blendState: BlendState.Additive);
        batch.DrawWithSnapshot(() => {
            batch.Draw(circle, position, DrawInfo.Default with {
                Origin = circleOrigin,
                Clip = circleClip,
                Color = circleColor * Projectile.Opacity,
                Scale = circleScale
            });
        }, blendState: BlendState.Additive);

        return false;
    }
}
