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
using System.Linq;

using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class ForbiddenTwig : NatureProjectile_NoTextureLoad, IRequestAssets {
    private static byte BODYCOUNT => 100;
    private static ushort TIMELEFT => (ushort)MathUtils.SecondsToFrames(15);
    private static byte BODYFRAMECOUNT => 4;

    public struct VineBodyInfo() {
        public static float MAXPROGRESS => 30f;
        public static float SANDFALLSTATETIME => MAXPROGRESS;
        public static float MAXPROGRESS2 => MAXPROGRESS + SANDFALLSTATETIME;

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
        public readonly float Progress2 => Utils.Remap(ActualProgress, MAXPROGRESS / 2f, MAXPROGRESS - MAXPROGRESS / 5f, 0f, 1f, true);
        public readonly float Progress3 => Utils.Remap(ActualProgress, MAXPROGRESS - MAXPROGRESS / 5f, MAXPROGRESS, 0f, 1f, true);
        public readonly float Progress4 => Utils.Remap(ActualProgress, MAXPROGRESS - MAXPROGRESS / 5f * 2f, MAXPROGRESS, 0f, 1f, true);
        public readonly float Progress5 => Utils.Remap(ActualProgress, MAXPROGRESS2, MAXPROGRESS, 0f, 1f, true);
    }

    private VineBodyInfo[] _bodyData = null!;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float SandfallProgress => ref Projectile.localAI[1];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public List<VineBodyInfo> ActiveData => [.. _bodyData.Where(x => x.Active)];

    public enum DesertTendrilVineRequstedTextureType : byte {
        Twig1,
        Twig2,
        Twig3,
        Sandfall
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)DesertTendrilVineRequstedTextureType.Twig1, ResourceManager.NatureProjectileTextures + "ForbiddenTwig1"),
         ((byte)DesertTendrilVineRequstedTextureType.Twig2, ResourceManager.NatureProjectileTextures + "ForbiddenTwig2"),
         ((byte)DesertTendrilVineRequstedTextureType.Twig3, ResourceManager.NatureProjectileTextures + "ForbiddenTwig3"),
         ((byte) DesertTendrilVineRequstedTextureType.Sandfall, ResourceManager.NatureProjectileTextures + "ForbiddenSandfall")];

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;

        Projectile.friendly = true;

        Projectile.penetrate = -1;

        Projectile.timeLeft = TIMELEFT;

        Projectile.manualDirectionChange = true;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 2;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
        foreach (VineBodyInfo bodyInfo in ActiveData) {
            float progress3 = MathUtils.Clamp01(bodyInfo.Progress3 * 2f);
            Vector2 position = bodyInfo.Position;
            float height = MathF.Abs(Projectile.Center.Y - (Projectile.Center + position).Y) * progress3 * 1.5f;
            position.Y += height;
            position += Projectile.Center;
            if (progress3 > 0f && progress3 < 1f) {
                if (GeometryUtils.CenteredSquare(position, (int)(50 * MathUtils.YoYo(progress3))).Intersects(targetHitbox)) {
                    return true;
                }
            }
        }

        return false;
    }

    public override void AI() {
        Player owner = Projectile.GetOwnerAsPlayer();
        //Projectile.Center = owner.MountedCenter;
        //Projectile.Center = Utils.Floor(Projectile.Center) + Vector2.UnitY * owner.gfxOffY;

        SandfallProgress += 3f;
        if (SandfallProgress > 138) {
            SandfallProgress = 0f;
        }

        float stepLength = 22f;
        if (!Init) {
            Init = true;

            Projectile.position.Y = owner.Bottom.Y;

            Projectile.direction = owner.direction;

            Vector2 position = Vector2.Zero,
                    startPosition = position;
            _bodyData = new VineBodyInfo[BODYCOUNT];
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

        int length2 = _bodyData.Length;
        for (int i = 0; i < length2; i++) {
            int currentSegmentIndex = i,
                previousSegmentIndex = Math.Max(0, i - 1);
            ref VineBodyInfo currentSegmentData = ref _bodyData[currentSegmentIndex],
                             previousSegmentData = ref _bodyData[previousSegmentIndex];
            if (currentSegmentIndex > 0 && previousSegmentData.ActualProgress < 1f) {
                continue;
            }
            float progress = i / (float)ActiveData.Count;
            float slowValue = 0.5f + MathF.Max(0.5f, 1f - MathUtils.YoYo(progress));
            if (currentSegmentData.Progress >= 1f) {
                slowValue = 1f;
            }
            currentSegmentData.ActualProgress = Helper.Approach(currentSegmentData.ActualProgress, VineBodyInfo.MAXPROGRESS2, 0.5f * slowValue);
            float progress3 = currentSegmentData.Progress3;
            float progress2 = currentSegmentData.Progress2;
            Vector2 position = Projectile.Center + currentSegmentData.Position;
            bool active = currentSegmentData.Active;
            if (!active) {
                continue;
            }
            if (progress3 > 0f && progress3 < 1f && Main.rand.NextBool()) {
                Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(20f, 20f), DustID.Sand);
                dust.color = Color.Lerp(Color.White, new Color(100, 82, 58), Main.rand.NextFloat());
                dust.velocity.Y = MathF.Abs(dust.velocity.Y);
                dust.velocity.X *= 0.4f;
                dust.velocity.Y *= Main.rand.NextFloat(1f, 7.5f);
                dust.fadeIn = Main.rand.NextFloat(0.25f);
                dust.alpha = Main.rand.Next(100);
            }
            if (progress2 > 0f && progress2 < 1f && Main.rand.NextChance(progress2 * 0.5f) && Main.rand.NextBool()) {
                Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(20f, 20f), DustID.Sand);
                dust.color = Color.Lerp(Color.White, new Color(100, 82, 58), Main.rand.NextFloat());
                dust.velocity.Y = MathF.Abs(dust.velocity.Y);
                dust.velocity.X *= 0.4f;
                dust.velocity.Y *= Main.rand.NextFloat(1f, 7.5f);
                dust.fadeIn = Main.rand.NextFloat(0.25f);
                dust.alpha = Main.rand.Next(100);
            }
            if (currentSegmentData.ActualProgress >= VineBodyInfo.MAXPROGRESS2) {
                // explosion effect
                if (!Main.dedServ) {
                    int gore = Gore.NewGore(Projectile.GetSource_FromAI(), position - Vector2.UnitY * 6f + Main.rand.NextVector2Circular(10f, 10f),
                        Vector2.Zero, $"ForbiddenTwig{(byte)currentSegmentData.BodyType + 1}".GetGoreType());
                    Main.gore[gore].velocity *= 0.5f;
                    Main.gore[gore].velocity.Y = MathF.Abs(Main.gore[gore].velocity.Y);
                    Main.gore[gore].position -= new Vector2(Main.gore[gore].Width, Main.gore[gore].Height) / 2f;
                }

                for (int k = 0; k < 3; k++) {
                    float dustScale = 0.915f + 0.15f * Main.rand.NextFloat();
                    Dust dust = Main.dust[Dust.NewDust(position + Main.rand.NextVector2Circular(10f, 10f) * 0.75f, 
                        0, 0, DustID.WoodFurniture, 0f, 0f, Main.rand.Next(100), Color.Lerp(new Color(100, 82, 58), default, 0.25f * Main.rand.NextFloat()), dustScale)];
                    dust.noGravity = true;
                    dust.fadeIn = 0.5f;
                    dust.noLight = true;
                    dust.velocity *= Main.rand.NextFloat(0.5f, 1f);
                }

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

        Texture2D bodyTexture = indexedTextureAssets[(byte)DesertTendrilVineRequstedTextureType.Twig1].Value,
                  body2Texture = indexedTextureAssets[(byte)DesertTendrilVineRequstedTextureType.Twig2].Value,
                  body3Texture = indexedTextureAssets[(byte)DesertTendrilVineRequstedTextureType.Twig3].Value;
        SpriteBatch batch = Main.spriteBatch;
        List<VineBodyInfo> data = ActiveData;
        int count = data.Count;
        SpriteFrame frame = new(1, BODYFRAMECOUNT);
        int initialHeight = frame.GetSourceRectangle(bodyTexture).Height;
        Texture2D sandfallTexture = indexedTextureAssets[(byte)DesertTendrilVineRequstedTextureType.Sandfall].Value;
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
            Texture2D texture = bodyTexture;
            Rectangle clip = frame.With(0, currentFrame).GetSourceRectangle(texture);
            Vector2 origin = clip.Centered();
            int progressHeight = (int)(initialHeight * currentVineBodyInfo.Progress);
            clip.Height = progressHeight;
            Vector2 position = currentVineBodyInfo.Position,
                    nextPosition = nextVineBodyInfo.Position;
            float rotation = position.DirectionTo(nextPosition).ToRotation() + MathHelper.PiOver2;
            Vector2 scale = Vector2.One;
            Vector2 drawPosition = Projectile.Center + position;
            Color color = Lighting.GetColor(drawPosition.ToTileCoordinates());
            batch.Draw(body3Texture, drawPosition, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Rotation = rotation + MathHelper.Pi,
                ImageFlip = effects,
                Scale = scale,
                Color = color * currentVineBodyInfo.Progress3
            });
            batch.Draw(body2Texture, drawPosition, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Rotation = rotation + MathHelper.Pi,
                ImageFlip = effects,
                Scale = scale,
                Color = color * (1f - currentVineBodyInfo.Progress3)
            });
            batch.Draw(bodyTexture, drawPosition, DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Rotation = rotation + MathHelper.Pi,
                ImageFlip = effects,
                Scale = scale,
                Color = color * (1f - currentVineBodyInfo.Progress2)
            });
            uint seed = (uint)(position.GetHashCode() * 100 + Projectile.whoAmI);
            float randomValue = MathUtils.PseudoRandRange(ref seed, 0.625f, 1f),
                  randomValue2 = MathUtils.PseudoRandRange(ref seed, 0f, 1f);
            Rectangle sandfallClip = new(0, (int)(SandfallProgress + i * 138 + Projectile.whoAmI) % 138, sandfallTexture.Width, 138);
            float num = MathF.Abs(Projectile.Center.Y - (Projectile.Center + position).Y) * randomValue * 2.5f;
            Vector2 sandfallScale = new(Projectile.scale, (float)((double)num / sandfallTexture.Height));
            float sandfallRotation = 0f;
            Vector2 sandfallOrigin = sandfallClip.TopCenter();
            Color sandfallColor = Color.Lerp(new Color(212, 192, 100), new Color(100, 82, 58), randomValue2) * 1f * currentVineBodyInfo.Progress4 * currentVineBodyInfo.Progress5;
            sandfallColor = color.MultiplyRGB(sandfallColor);
            batch.DrawWithSnapshot(() => {
                Effect sandfallShader = ShaderLoader.Sandfall.Value;
                sandfallShader.Parameters["borderTop"].SetValue(0.05f + 0.95f * (1f - currentVineBodyInfo.Progress5));
                sandfallShader.Parameters["borderBottom"].SetValue(0.25f);
                sandfallShader.Parameters["uColor"].SetValue(sandfallColor.ToVector3());
                sandfallShader.Parameters["uOpacity"].SetValue(1f);
                sandfallShader.Parameters["uSourceRect"].SetValue(new Vector4(sandfallClip.X / (float)sandfallTexture.Width,
                                                                              sandfallClip.Y / (float)sandfallTexture.Height,
                                                                              sandfallClip.Width / (float)sandfallTexture.Width,
                                                                              sandfallClip.Height / (float)sandfallTexture.Height));
                sandfallShader.CurrentTechnique.Passes[0].Apply();
                batch.Draw(sandfallTexture, drawPosition, DrawInfo.Default with {
                    Clip = sandfallClip,
                    Origin = sandfallOrigin,
                    Rotation = sandfallRotation,
                    ImageFlip = effects,
                    Scale = sandfallScale,
                    Color = sandfallColor
                });

            }, sortMode: SpriteSortMode.Immediate, blendState: BlendState.Additive);
        }
    }
}
