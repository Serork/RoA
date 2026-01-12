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
using Terraria.GameContent;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class FallenLeavesBranch : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(10);

    public enum FallenLeavesRequstedTextureType : byte {
        Branch
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)FallenLeavesRequstedTextureType.Branch, ResourceManager.NatureProjectileTextures + "FallenLeaves_Branch")];

    private record struct BranchInfo(Vector2 Position, byte FrameY = 0, float Progress = 0f);

    private BranchInfo[] _branchData = null!;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float ReversedValue => ref Projectile.ai[0];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public bool Reversed {
        get => ReversedValue == 1f;
        set => ReversedValue = value.ToInt();
    }

    public float RotationFactor => Projectile.ai[2];

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.timeLeft = TIMELEFT;

        Projectile.tileCollide = false;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void AI() {
        void init() {
            if (!Init) {
                Init = true;

                int segmentCount = 36;

                BranchInfo[] sampleBranchInfo = new BranchInfo[segmentCount + 1];
                int direction = Reversed.ToDirectionInt();
                Vector2 startVelocity = Vector2.UnitY.RotatedBy(MathHelper.TwoPi * RotationFactor);
                int index = 0;
                Vector2 center = Projectile.Center;
                int step = 50;
                Vector2 startPosition = center + new Vector2(Main.rand.NextFloat(0.25f, 1f) * Main.rand.NextBool().ToDirectionInt(), Main.rand.NextFloat(0.5f, 1f) * Main.rand.NextBool().ToDirectionInt()) * startVelocity.SafeNormalize().Y * step * Main.rand.NextFloat(1f, 2f) * 0.75f,
                        baseStartPosition = startPosition;
                int segmentHeight = 20;
                while (true) {
                    if (index > segmentCount) {
                        break;
                    }
                    Vector2 bodyPositionToAdd = startPosition + startVelocity.SafeNormalize() * step * new Vector2(direction, 1f);
                    float sineOffset = step * 0.1f * Helper.Wave(-1f, 1f, 2f, index);
                    float sineFactor = MathF.Sin(index * 2f);
                    bodyPositionToAdd += Vector2.UnitX.RotatedBy(startVelocity.ToRotation()) * sineFactor * sineOffset;
                    Vector2 offset = new(10f * Main.rand.NextFloatDirection(), -10f * Main.rand.NextFloat());
                    startPosition += offset;
                    sampleBranchInfo[index] = new BranchInfo(bodyPositionToAdd);

                    float velocityStep = MathHelper.TwoPi / segmentHeight;
                    startVelocity = startVelocity.RotatedBy(velocityStep);

                    index++;
                }
                index = 0;
                _branchData = new BranchInfo[sampleBranchInfo.Length];
                Vector2 position = baseStartPosition;
                while (true) {
                    if (index > segmentCount) {
                        break;
                    }
                    int nextIndex = Math.Min(index + 1, segmentCount - 1);
                    Vector2 branchPosition = position + sampleBranchInfo[index].Position.DirectionTo(sampleBranchInfo[nextIndex].Position) * segmentHeight;
                    _branchData[index] = new BranchInfo(branchPosition, (byte)Main.rand.Next(3));
                    position = branchPosition;
                    index++;
                }
            }
        }
        void handleBodyPoints() {
            int count = _branchData.Length;
            bool processSegment(int i) {
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Max(0, i - 1);
                ref BranchInfo currentSegmentData = ref _branchData[currentSegmentIndex],
                               previousSegmentData = ref _branchData[previousSegmentIndex];
                float lerpValue = 0.2f;
                if (currentSegmentIndex > 0 && previousSegmentData.Progress < lerpValue * 2f) {
                    return false;
                }
                currentSegmentData.Progress = Helper.Approach(currentSegmentData.Progress, 20f, lerpValue);
                if (currentSegmentData.Progress < 20f && Main.rand.NextChance(MathUtils.Clamp01(currentSegmentData.Progress - 10f))) {
                    if (Main.rand.NextBool(3)) {
                        int size = 20;
                        Dust dust = Dust.NewDustDirect(currentSegmentData.Position + new Vector2(-2f, 8f) - Vector2.One * size * 0.5f, size, size, DustID.Torch, 0f, 0f, 100);
                        if (Main.rand.Next(2) == 0) {
                            dust.noGravity = true;
                            dust.fadeIn = 1.15f;
                        }
                        else {
                            dust.scale = 0.6f;
                        }

                        dust.velocity *= 0.6f;
                        dust.velocity.Y -= 1.2f;
                        dust.noLight = true;
                        dust.position.Y -= 4f;
                    }
                }
                return true;
            }
            if (!Reversed) {
                for (int i = 0; i < count; i++) {
                    if (!processSegment(i)) {
                        continue;
                    }
                }
                return;
            }
            for (int i = count - 1; i >= 0; i--) {
                if (!processSegment(i)) {
                    continue;
                }
            }
        }

        init();
        handleBodyPoints();
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<FallenLeavesBranch>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = indexedTextureAssets[(byte)FallenLeavesRequstedTextureType.Branch].Value;
        int count = _branchData.Length - 1;
        int[] flameTypes = [326, 327, 328];
        Main.instance.LoadProjectile(flameTypes[0]);
        Main.instance.LoadProjectile(flameTypes[1]);
        Main.instance.LoadProjectile(flameTypes[2]);
        for (int i = 0; i < count; i++) {
            int nextIndex = i + 1,
                currentIndex = i;
            if (nextIndex >= count) {
                break;
            }
            BranchInfo currentBranchInfo = _branchData[currentIndex],
                       nextBranchInfo = _branchData[nextIndex];
            int frameY = currentBranchInfo.FrameY;
            if (currentIndex == 0) {
                frameY = 3;
            }
            Rectangle clip = Utils.Frame(texture, 1, 4, frameY: frameY);
            Vector2 origin = clip.BottomCenter();
            Vector2 position = currentBranchInfo.Position,
                    nextPosition = nextBranchInfo.Position;
            float rotation = position.AngleTo(nextPosition) + MathHelper.PiOver2;
            float progress = MathUtils.Clamp01(currentBranchInfo.Progress),
                  progress2 = Utils.GetLerpValue(8f, 10f, currentBranchInfo.Progress , true),
                  progress3 = Utils.GetLerpValue(20f, 18.5f, currentBranchInfo.Progress, true),
                  progress4 = Utils.GetLerpValue(20f, 10f, currentBranchInfo.Progress, true);
            Color baseColor = Color.White;
            Color color = Lighting.GetColor(position.ToTileCoordinates()).MultiplyRGB(baseColor) * progress * progress3;
            Vector2 scale = new(1f * MathF.Max(0.5f, progress), 1f);
            int scaleThreshhold = 6;
            if (i > count - scaleThreshhold) {
                scale.X *= MathF.Abs((i - count) / (float)scaleThreshhold);
            }
            DrawInfo drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Rotation = rotation,
                Color = color,
                Scale = scale
            };
            batch.Draw(texture, position, drawInfo);
            ulong seed = (byte)(Main.TileFrameSeed + 1) ^ (((ulong)position.X << 32) | (uint)position.Y);
            for (int i2 = 0; i2 < 4; i2++) {
                int flameType = Utils.RandomInt(ref seed, 3);
                Texture2D flameTexture = TextureAssets.Projectile[flameTypes[flameType]].Value;
                clip = flameTexture.Bounds;
                origin = clip.Centered();
                rotation = 0f;
                color = new Color(120, 120, 120, 60) * progress2 * progress3;
                drawInfo = new() {
                    Clip = clip,
                    Origin = origin,
                    Rotation = rotation,
                    Color = color
                };
                batch.Draw(flameTexture, position + new Vector2(Utils.RandomInt(ref seed, -2, 3), Utils.RandomInt(ref seed, -2, 3)), drawInfo);
            }
        }
    }
}
