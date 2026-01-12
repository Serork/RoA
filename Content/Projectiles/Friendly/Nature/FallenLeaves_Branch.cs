using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
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
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class FallenLeavesBranch : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(10);

    public enum FallenLeavesRequstedTextureType : byte {
        Branch,
        Burn,
        Glow
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)FallenLeavesRequstedTextureType.Branch, ResourceManager.NatureProjectileTextures + "FallenLeaves_Branch"),
         ((byte)FallenLeavesRequstedTextureType.Burn, ResourceManager.NatureProjectileTextures + "FallenLeaves_Branch_Burn"),
         ((byte)FallenLeavesRequstedTextureType.Glow, ResourceManager.NatureProjectileTextures + "FallenLeaves_Branch_Glow")];

    private record struct BranchSegmentInfo(Vector2 Position, byte FrameY = 0, float Progress = 0f, bool FacedRight = false, bool Destroyed = false, bool ShouldGlow = false);

    private BranchSegmentInfo[] _branchData = null!;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float ReversedValue => ref Projectile.ai[0];
    public ref float ShouldBurnValue => ref Projectile.ai[2];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public bool Reversed {
        get => ReversedValue == 1f;
        set => ReversedValue = value.ToInt();
    }

    public bool ShouldBurn {
        get => ShouldBurnValue == 1f;
        set => ShouldBurnValue = value.ToInt();
    }

    public float ProgressFactor => Projectile.ai[1];

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

                int segmentCount = 90;

                BranchSegmentInfo[] sampleBranchInfo = new BranchSegmentInfo[segmentCount + 1];
                int direction = Reversed.ToDirectionInt();
                Player player = Projectile.GetOwnerAsPlayer();
                float mouseRotation = player.Center.AngleTo(player.GetViableMousePosition()) - MathHelper.PiOver2;
                mouseRotation += 0.2f * Main.rand.NextFloatDirection();
                Vector2 startVelocity = Vector2.UnitY.RotatedBy(mouseRotation),
                        baseStartVelocity = startVelocity;
                int index = 0;
                Vector2 center = Projectile.Center;
                int step = 50;
                Vector2 startPosition = center + new Vector2(Main.rand.NextFloat(0.25f, 1f) * Main.rand.NextBool().ToDirectionInt(), Main.rand.NextFloat(0.5f, 1f) * Main.rand.NextBool().ToDirectionInt()) * startVelocity.SafeNormalize().Y * step * Main.rand.NextFloat(1f, 2f) * 0.25f,
                        baseStartPosition = startPosition;
                int segmentHeight = 20;
                float endFactor = 1f;
                while (true) {
                    if (index > segmentCount) {
                        break;
                    }
                    int endThreshhold = 10;
                    if (index > segmentCount - endThreshhold) {
                        endFactor = Helper.Approach(endFactor, 0.5f, 0.05f);
                    }
                    Vector2 bodyPositionToAdd = startPosition + startVelocity.SafeNormalize() * step * new Vector2(direction, 1f);
                    float sineOffset = step * 0.1f * Helper.Wave(-1f, 1f, 2f, index);
                    float sineFactor = MathF.Sin(index * 2f);
                    bodyPositionToAdd += Vector2.UnitX.RotatedBy(startVelocity.ToRotation()) * sineFactor * sineOffset;
                    Vector2 offset = new(5f * Main.rand.NextFloatDirection(), 5f * Main.rand.NextFloatDirection());
                    startPosition += offset;
                    Vector2 offset2 = baseStartVelocity * 10f * Main.rand.NextFloat(0.25f, 1f);
                    startPosition += offset2;
                    sampleBranchInfo[index] = new BranchSegmentInfo(bodyPositionToAdd);

                    float velocityStep = MathHelper.TwoPi / segmentHeight * Main.rand.NextFloat(0.75f, 1f) * endFactor;
                    velocityStep = Utils.AngleLerp(velocityStep, mouseRotation, 1f - endFactor);
                    startVelocity = startVelocity.RotatedBy(velocityStep * endFactor);

                    index++;
                }
                index = 0;
                _branchData = new BranchSegmentInfo[sampleBranchInfo.Length];
                Vector2 position = baseStartPosition;
                while (true) {
                    if (index > segmentCount) {
                        break;
                    }
                    int nextIndex = Math.Min(index + 1, segmentCount - 1);
                    Vector2 branchPosition = position + sampleBranchInfo[index].Position.DirectionTo(sampleBranchInfo[nextIndex].Position) * segmentHeight;
                    _branchData[index] = new BranchSegmentInfo(branchPosition, (byte)Main.rand.Next(3), 0f, Main.rand.NextBool(), false, Main.rand.NextBool(5));
                    position = branchPosition;
                    index++;
                }
            }
        }
        void handleBodyPoints() {
            int count = _branchData.Length;
            bool processSegment(int i, bool up = true) {
                int currentSegmentIndex = i,
                    previousSegmentIndex = Math.Max(0, i - 1);
                if (!up) {
                    previousSegmentIndex = Math.Min(count - 1, i + 1);
                }
                ref BranchSegmentInfo currentSegmentData = ref _branchData[currentSegmentIndex],
                                      previousSegmentData = ref _branchData[previousSegmentIndex];
                float lerpValue = 0.2f * ProgressFactor;
                if (up) {
                    if (currentSegmentIndex > 0 && previousSegmentData.Progress < lerpValue * 2f) {
                        return false;
                    }
                }
                else {
                    if (currentSegmentIndex < count - 1 && previousSegmentData.Progress < lerpValue * 2f) {
                        return false;
                    }
                }
                Vector2 position = currentSegmentData.Position,
                        nextPosition = previousSegmentData.Position;
                float rotation = position.AngleTo(nextPosition);
                position -= Vector2.UnitX.RotatedBy(rotation) * 10f;
                float to = 20f;
                currentSegmentData.Progress = Helper.Approach(currentSegmentData.Progress, to, lerpValue);
                int size = 20;
                Vector2 offset = new(-2f, 0f);
                position += offset;
                if (currentSegmentData.Progress >= to) {
                    // spawn dusts
                    if (!currentSegmentData.Destroyed) {
                        if (!ShouldBurn) {
                            if (!Main.dedServ) {
                                int gore = Gore.NewGore(Projectile.GetSource_FromAI(), position + Main.rand.NextVector2Circular(size, size) * 0.25f,
                                    Vector2.Zero, $"FallenLeavesBranchGore{(byte)Main.rand.Next(3) + 1}".GetGoreType());
                                Main.gore[gore].velocity *= 0.5f;
                                Main.gore[gore].velocity.Y = MathF.Abs(Main.gore[gore].velocity.Y);
                                Main.gore[gore].position -= new Vector2(Main.gore[gore].Width, Main.gore[gore].Height) / 2f;
                                Main.gore[gore].rotation = MathHelper.TwoPi * Main.rand.NextFloat();
                            }
                        }
                        for (int k = 0; k < 3; k++) {
                            float dustScale = 0.915f + 0.15f * Main.rand.NextFloat();
                            Dust dust = Main.dust[Dust.NewDust(position - Vector2.One * size * 0.5f, size, size, ModContent.DustType<FallenLeavesBranchDust>(), 0f, 0f, Main.rand.Next(100),
                                ShouldBurn ? Color.Lerp(Color.White, Color.Black, 0.25f) : default, dustScale)];
                            dust.noGravity = true;
                            dust.fadeIn = 0.5f;
                            dust.noLight = true;
                            dust.velocity *= 0.5f;
                            dust.velocity *= Main.rand.NextFloat(0.5f, 1f);
                        }
                    }
                    currentSegmentData.Destroyed = true;
                }
                bool shouldBurn = ShouldBurn && currentSegmentIndex < count - 3;
                if (shouldBurn && currentSegmentData.Progress < 20f && Main.rand.NextChance(MathUtils.Clamp01((currentSegmentData.Progress - 10f) * 3))) {
                    if (Main.rand.NextBool(3)) {
                        Dust dust = Dust.NewDustDirect(position + Vector2.UnitY * 8f - Vector2.One * size * 0.5f, size, size, DustID.Torch, 0f, 0f, 100);
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
        void killItself() {
            int count = _branchData.Length;
            int destroyedCount = 0;
            for (int i = 0; i < count; i++) {
                if (_branchData[i].Destroyed) {
                    destroyedCount++;
                }
            }
            if (destroyedCount >= count) {
                Projectile.Kill();
            }
        }

        init();
        handleBodyPoints();
        killItself();
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<FallenLeavesBranch>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        SpriteBatch batch = Main.spriteBatch;
        Texture2D branchTexture = indexedTextureAssets[(byte)FallenLeavesRequstedTextureType.Branch].Value,
                  burnTexture = indexedTextureAssets[(byte)FallenLeavesRequstedTextureType.Burn].Value,
                  glowTexture = indexedTextureAssets[(byte)FallenLeavesRequstedTextureType.Glow].Value;
        int count = _branchData.Length - 1;
        //int[] flameTypes = [326, 327, 328];
        //Main.instance.LoadProjectile(flameTypes[0]);
        //Main.instance.LoadProjectile(flameTypes[1]);
        //Main.instance.LoadProjectile(flameTypes[2]);
        for (int i = 0; i < count; i++) {
            int nextIndex = i + 1,
                currentIndex = i;
            if (nextIndex >= count) {
                break;
            }
            BranchSegmentInfo currentBranchInfo = _branchData[currentIndex],
                              nextBranchInfo = _branchData[nextIndex];
            SpriteEffects flip = currentBranchInfo.FacedRight.ToSpriteEffects();
            int frameY = currentBranchInfo.FrameY + 1;
            bool start = currentIndex == 0,
                 end = nextIndex == count - 2;
            if (start) {
                frameY = 4;
            }
            if (end) {
                frameY = 0;
            }
            Rectangle clip = Utils.Frame(branchTexture, 1, 5, frameY: frameY);
            Vector2 origin = clip.BottomCenter();
            Vector2 position = currentBranchInfo.Position,
                    nextPosition = nextBranchInfo.Position;
            float rotation = position.AngleTo(nextPosition) + MathHelper.PiOver2;
            float progress = MathUtils.Clamp01(currentBranchInfo.Progress),
                  progress2 = Utils.GetLerpValue(8f, 10f, currentBranchInfo.Progress , true),
                  progress3 = Utils.GetLerpValue(20f, 18.5f, currentBranchInfo.Progress, true),
                  progress4 = Utils.GetLerpValue(20f, 10f, currentBranchInfo.Progress, true);
            Color baseColor = Color.White;
            bool shouldBurn = ShouldBurn;
            if (shouldBurn) {
                baseColor = Color.Lerp(baseColor, Color.Lerp(Color.White, Color.Black, 0.5f), 1f - progress4);
            }
            Color color = Lighting.GetColor(position.ToTileCoordinates()).MultiplyRGB(baseColor) * progress;
            if (shouldBurn) {
                color *= progress3;
            }
            else {
                if (currentBranchInfo.Destroyed) {
                    color *= 0f;
                }
            }
            //Vector2 scale = new(1f * MathF.Max(0.5f, progress), 1f);
            //int scaleThreshhold = 6;
            //if (i > count - scaleThreshhold) {
            //    scale.X *= MathF.Abs((i - count) / (float)scaleThreshhold);
            //}
            Vector2 scale = Vector2.One;
            DrawInfo drawInfo = new() {
                Clip = clip,
                Origin = origin,
                Rotation = rotation,
                Color = color,
                Scale = scale,
                ImageFlip = flip
            };
            batch.Draw(branchTexture, position, drawInfo);
            if (currentBranchInfo.ShouldGlow) {
                batch.Draw(glowTexture, position, drawInfo);
            }
            if (shouldBurn) {
                ulong seed = (byte)(Main.TileFrameSeed + 1) ^ (((ulong)position.X << 32) | (uint)position.Y);
                for (int i2 = 0; i2 < 4; i2++) {
                    color = new Color(120, 120, 120, 60) * progress2 * progress3 * 0.75f;
                    batch.Draw(burnTexture, position + new Vector2(Utils.RandomInt(ref seed, -2, 3), Utils.RandomInt(ref seed, -2, 3)), drawInfo with {
                        Color = color
                    });
                }
                //for (int i2 = 0; i2 < 4; i2++) {
                //    int flameType = Utils.RandomInt(ref seed, 3);
                //    Texture2D flameTexture = TextureAssets.Projectile[flameTypes[flameType]].Value;
                //    clip = flameTexture.Bounds;
                //    origin = clip.Centered();
                //    rotation = 0f;
                //    color = new Color(120, 120, 120, 60) * progress2 * progress3;
                //    float scaleFactor = MathUtils.Clamp01(scale.X * 2f);
                //    drawInfo = new() {
                //        Clip = clip,
                //        Origin = origin,
                //        Rotation = rotation,
                //        Color = color,
                //        Scale = Vector2.One * scaleFactor,
                //        ImageFlip = flip
                //    };
                //    batch.Draw(flameTexture, position + new Vector2(Utils.RandomInt(ref seed, -2, 3), Utils.RandomInt(ref seed, -2, 3)) * scaleFactor, drawInfo);
                //}
            }
        }
    }
}
