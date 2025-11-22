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
    public static byte SLASHCOUNT => 30;
    private static ushort DANGERDISTANCEINPIXELS => 160;
    private static float SLASHATTACKFREQUENCYINTICKS => 10f;
    private static ushort TIMELEFT => (ushort)MathUtils.SecondsToFrames(9);
    private static ushort BEFOREATTACKTIME => 25;

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
    public ref float ExtraOpacity => ref Projectile.localAI[2];

    public ref float StartedAttackValue => ref Projectile.ai[0];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public bool StartedAttack {
        get => StartedAttackValue == 1f;
        set => StartedAttackValue = value.ToInt();
    }

    public override string Texture => ResourceManager.NatureProjectileTextures + "HiTechCattleProd_Star";

    protected override void SafeSetDefaults() {
        SetNatureValues(Projectile, false, false);

        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;

        Projectile.tileCollide = false;

        Projectile.penetrate = -1;

        Projectile.Opacity = 0f;
    }

    public override bool? CanDamage() => false;
    public override bool? CanCutTiles() => false;

    public override void AI() {
        if (!Init) {
            Init = true;

            float scale = Projectile.GetOwnerAsPlayer().CappedMeleeOrDruidScale();
            Projectile.scale = scale;

            for (int i = 0; i < _hiTechBeams.Length; i++) {
                _hiTechBeams[i] = new HiTechBeamInfo {
                    Rotation = MathHelper.TwoPi * Main.rand.NextFloatDirection()
                };
            }
            ExtraOpacity = 0f;
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
        NPC? target = NPCUtils.FindClosestNPC(Projectile.Center, (int)(DANGERDISTANCEINPIXELS * Projectile.scale), false);
        bool hasTarget = target is not null;
        if (Projectile.timeLeft <= 60 * 2) {
            hasTarget = true;
        }
        bool justSpawned = Projectile.timeLeft > TIMELEFT - BEFOREATTACKTIME;
        if (!justSpawned && hasTarget) {
            if (!StartedAttack) {
                Projectile.timeLeft = 60 * 2;
                StartedAttack = true;
            }
        }
        if (StartedAttack) {
            hasTarget = true;
        }
        if (hasTarget && justSpawned) {
            hasTarget = false;
        }
        ExtraOpacity = Helper.Approach(ExtraOpacity, hasTarget ? 0.85f : 0f, TimeSystem.LogicDeltaTime * 2.5f);
        if (ExtraOpacity >= 0.75f && BeamCounter % SLASHATTACKFREQUENCYINTICKS == 0f) {
            if (Projectile.IsOwnerLocal()) {
                Vector2 position = Projectile.Center + Vector2.One.RotatedByRandom(MathHelper.TwoPi) * DANGERDISTANCEINPIXELS * 0.55f;
                ProjectileUtils.SpawnPlayerOwnedProjectile<HiTechSlash>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromAI()) {
                    Damage = Projectile.damage,
                    KnockBack = Projectile.knockBack,
                    Position = position,
                    Velocity = /*Vector2.One.RotatedByRandom(MathHelper.TwoPi)*/position.DirectionTo(Projectile.Center) * 10f,
                    AI0 = Projectile.Center.X,
                    AI1 = Projectile.Center.Y,
                    AI2 = Projectile.scale
                });
            }
        }

        if (Main.rand.NextBool(10)) {
            int num674 = Utils.SelectRandom<int>(Main.rand, 226, 229);
            Vector2 center8 = Projectile.Center;
            int num676 = DANGERDISTANCEINPIXELS / 2;
            int num677 = Dust.NewDust(center8 + Vector2.One * -num676, num676 * 2, num676 * 2, num674, 0f, 0f, 100, default(Color), 1f);
            Dust dust2 = Main.dust[num677];
            dust2.velocity *= 0.1f;
            if (Main.rand.Next(6) != 0)
                Main.dust[num677].noGravity = true;
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
        Vector2 baseScale = Vector2.One * Projectile.Opacity * Projectile.scale;
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

        float opacity2 = (1f - ExtraOpacity) * Projectile.Opacity;
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
                    Color = beamColor * opacity2,
                    Scale = beamScale,
                    Rotation = beamRotation
                });
                batch.DrawWithSnapshot(() => {
                    batch.Draw(beamTexture, beamPosition, DrawInfo.Default with {
                        Clip = beamClip,
                        Origin = beamOrigin,
                        Color = beamColor * 0.5f * opacity2,
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
            Vector2 starPartScale = Vector2.One * Projectile.scale * Helper.Wave(0.75f, 0.9f, waveSpeed, i * MathF.Pow(Utils.RandomInt(ref partSeed, 100), 2));
            batch.Draw(starPartTexture, starPartPosition, DrawInfo.Default with {
                Clip = starPartClip,
                Origin = starPartOrigin,
                Color = starPartColor * opacity2,
                Scale = starPartScale
            });
            batch.DrawWithSnapshot(() => {
                batch.Draw(starPartTexture, starPartPosition, DrawInfo.Default with {
                    Clip = starPartClip,
                    Origin = starPartOrigin,
                    Color = starPartColor * 0.5f * opacity2,
                    Scale = starPartScale * 1.25f
                });
            }, blendState: BlendState.Additive);
        }

        batch.Draw(starTexture, position, DrawInfo.Default with {
            Clip = starClip,
            Origin = starOrigin,
            Color = starColor * opacity2,
            Scale = starScale
        });
        batch.DrawWithSnapshot(() => {
            batch.Draw(starTexture, position, DrawInfo.Default with {
                Clip = starClip,
                Origin = starOrigin,
                Color = starColor * 0.5f * opacity2,
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
