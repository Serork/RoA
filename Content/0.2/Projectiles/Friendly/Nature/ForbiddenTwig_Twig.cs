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
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class ForbiddenTwig : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static byte BODYCOUNT => 100;
    private static ushort TIMELEFT => (ushort)MathUtils.SecondsToFrames(5);
    private static byte BODYFRAMECOUNT => 4;

    public struct VineBodyInfo() {
        public static float MAXPROGRESS => 20f;

        public enum VineBodyType : byte {
            End,
            Mid1,
            Mid2,
            Start
        }

        public VineBodyType BodyType;
        public Vector2 Position;
        public bool Flip;
        public float ActualProgress;

        public bool Active {
            readonly get => Position != default;
            set {
                if (!value) {
                    Position = default;
                }
            }
        }

        public readonly float Progress => MathUtils.Clamp01(ActualProgress);
        public readonly float Progress2 => Utils.Remap(ActualProgress, 1f, MAXPROGRESS, 0f, 1f, true);
    }

    private VineBodyInfo[] _bodyData = null!;

    public ref float InitValue => ref Projectile.localAI[0];

    public ref float WaveRotationFactor => ref Projectile.localAI[1];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public List<VineBodyInfo> ActiveData => [.. _bodyData.Where(x => x.Active)];

    public enum DesertTendrilVineRequstedTextureType : byte {
        Twig1,
        Twig2,
        Twig3
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)DesertTendrilVineRequstedTextureType.Twig1, ResourceManager.NatureProjectileTextures + "ForbiddenTwig1"),
         ((byte)DesertTendrilVineRequstedTextureType.Twig2, ResourceManager.NatureProjectileTextures + "ForbiddenTwig2"),
         ((byte)DesertTendrilVineRequstedTextureType.Twig3, ResourceManager.NatureProjectileTextures + "ForbiddenTwig3")];

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;

        Projectile.timeLeft = TIMELEFT;

        Projectile.manualDirectionChange = true;
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();
        //Projectile.Center = owner.MountedCenter;
        //Projectile.Center = Utils.Floor(Projectile.Center) + Vector2.UnitY * owner.gfxOffY;

        WaveRotationFactor += 0.25f;

        if (!Init) {
            Init = true;

            Projectile.direction = owner.direction;

            Vector2 position = Vector2.Zero,
                    startPosition = position;
            _bodyData = new VineBodyInfo[BODYCOUNT];
            float stepLength = 22f;
            int direction = Projectile.direction;
            float startAngle = MathHelper.Pi * direction;
            float angleStepLength = 0.2f;
            int length = _bodyData.Length;
            for (int i = 0; i < length; i++) {
                bool first = i < 2;
                _bodyData[i] = new VineBodyInfo() {
                    Position = position,
                    BodyType = first ? VineBodyInfo.VineBodyType.Start : Main.rand.GetRandomEnumValue<VineBodyInfo.VineBodyType>(1, 1),
                    Flip = Main.rand.NextBool()
                };
                float angleTo = MathHelper.Pi * 2f * direction;
                float progress = (MathF.Abs(startAngle) - MathHelper.Pi) / (MathF.Abs(angleTo) - MathHelper.Pi);
                float lerpValue = angleStepLength * Main.rand.NextFloat(-0.5f, 4.5f) * MathHelper.Clamp(MathUtils.YoYo(progress), 0.1f, 0.9f);
                lerpValue *= Utils.GetLerpValue(1.25f, 0.75f, progress, true);
                if (progress >= 0.35f && progress <= 0.7f) {
                    lerpValue *= 0.25f;
                }
                startAngle = Helper.Approach(startAngle, angleTo, lerpValue);
                position += Vector2.UnitY.RotatedBy(startAngle) * stepLength;
                bool last = i == length - 1;
                bool reachedEnd = position.Y >= startPosition.Y + stepLength;
                if (reachedEnd || last) {
                    _bodyData[i - 1].BodyType = VineBodyInfo.VineBodyType.End;
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < _bodyData.Length; i++) {
            int currentSegmentIndex = i,
                previousSegmentIndex = Math.Max(0, i - 1);
            ref VineBodyInfo currentSegmentData = ref _bodyData[currentSegmentIndex],
                             previousSegmentData = ref _bodyData[previousSegmentIndex];
            if (currentSegmentIndex > 0 && previousSegmentData.ActualProgress < 1f) {
                continue;
            }
            currentSegmentData.ActualProgress = Helper.Approach(currentSegmentData.ActualProgress, VineBodyInfo.MAXPROGRESS, 0.5f);
            if (currentSegmentData.Active && currentSegmentData.Progress2 >= 1f) {
                // explosion effect

                currentSegmentData.Active = false;
            }
        }
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnKill(int timeLeft) {

    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<ForbiddenTwig>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        Texture2D bodyTexture = indexedTextureAssets[(byte)DesertTendrilVineRequstedTextureType.Twig1].Value;
        SpriteBatch batch = Main.spriteBatch;
        List<VineBodyInfo> data = ActiveData;
        int count = data.Count;
        SpriteFrame frame = new(1, BODYFRAMECOUNT);
        int initialHeight = frame.GetSourceRectangle(bodyTexture).Height;
        for (int i = 0; i < count - 1; i++) {
            VineBodyInfo currentVineBodyInfo = data[i],
                         nextVineBodyInfo = data[Math.Min(count - 1, i + 1)];
            if (!currentVineBodyInfo.Active) {
                continue;
            }
            byte currentFrame = (byte)currentVineBodyInfo.BodyType;
            SpriteEffects effects = SpriteEffects.FlipVertically;
            if (currentVineBodyInfo.Flip) {
                effects |= SpriteEffects.FlipHorizontally;
            }
            Rectangle clip = frame.With(0, currentFrame).GetSourceRectangle(bodyTexture);
            Vector2 origin = clip.Centered();
            int progressHeight = (int)(initialHeight * currentVineBodyInfo.Progress);
            clip.Height = progressHeight;
            Vector2 position = currentVineBodyInfo.Position,
                    nextPosition = nextVineBodyInfo.Position;
            float rotation = position.DirectionTo(nextPosition).ToRotation() + MathHelper.PiOver2;
            Vector2 scale = Vector2.One;
            batch.Draw(bodyTexture, Projectile.Center + position, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Rotation = rotation + MathHelper.Pi,
                ImageFlip = effects,
                Scale = scale
            });
        }
    }
}
