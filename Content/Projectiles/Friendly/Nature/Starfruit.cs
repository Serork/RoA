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
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

// also see NPCTargetting.cs
[Tracked]
sealed class Starfruit : ModProjectile_NoTextureLoad, IRequestAssets {
    private static float GLOWTO => 1.25f;
    public static ushort NPCTARGETTINGTILECOUNTDISTANCE => 15;
    private static ushort TIMELEFT => MathUtils.SecondsToFrames(10);

    public enum StarfruitRequstedTextureType : byte {
        Stem,
        Top,
        Glow
    }

    (byte, string)[] IRequestAssets.IndexedPathsToTexture =>
        [((byte)StarfruitRequstedTextureType.Stem, ResourceManager.NatureProjectileTextures + "Starfruit_Stem"),
         ((byte)StarfruitRequstedTextureType.Top, ResourceManager.NatureProjectileTextures + "Starfruit_Top"),
         ((byte)StarfruitRequstedTextureType.Glow, ResourceManager.NatureProjectileTextures + "Starfruit_Glow")];

    private Vector2 _startPosition, _growToPosition;
    private int _seed;
    private float _flowerGrowthFactor;
    private float _glowFactor;

    public ref float InitValue => ref Projectile.localAI[0];

    public ref float DirectionValue => ref Projectile.ai[0];
    public ref float GrowFactorValue => ref Projectile.ai[1];
    public ref float StemLengthValue => ref Projectile.ai[2];

    public bool Init {
        get => InitValue != 0f;
        set => InitValue = value.ToInt();
    }

    public int Direction => (int)DirectionValue;
    public float GrowFactor => Ease.CubeIn(1f - MathUtils.Clamp01(GrowFactorValue));
    public int StemLength => (int)StemLengthValue;
    public Vector2 NPCTargetPosition => _growToPosition - Vector2.UnitY * 20f;

    public override void SetDefaults() {
        Projectile.SetSizeValues(1);

        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;

        Projectile.timeLeft = TIMELEFT;

        Projectile.manualDirectionChange = true;
    }

    public override void AI() {
        Projectile.Opacity = Utils.GetLerpValue(0, 30, Projectile.timeLeft, true);

        if (!Init) {
            Init = true;

            _startPosition = Projectile.Center + Vector2.UnitY * 12f;
            _growToPosition = _startPosition - Vector2.UnitY * 80f;

            _seed = Main.rand.Next();

            if (Projectile.IsOwnerLocal()) {
                DirectionValue = Main.rand.NextBool().ToDirectionInt();
                StemLengthValue = Main.rand.NextFloat(5f, 8f);
                GrowFactorValue = Main.rand.NextFromList(-0.1f, 0f);
                Projectile.netUpdate = true;
            }
        }

        Projectile.SetDirection(Direction);

        GrowFactorValue = Helper.Approach(GrowFactorValue, 1f, TimeSystem.LogicDeltaTime * 1f);

        Vector2 position = NPCTargetPosition;
        float glowWaveFactor = Helper.Wave(0f, 1f, 10f, Projectile.identity);
        Lighting.AddLight(position, Color.Lerp(new Color(252, 232, 154), new Color(255, 214, 56), glowWaveFactor).ToVector3() * 0.875f * GrowFactorValue * Projectile.Opacity);
        if (!Main.dedServ && Main.rand.Next(100 + (int)(100 * (1f - Projectile.Opacity))) == 0) {
            Gore.NewGore(Projectile.GetSource_FromThis(), position - new Vector2(12f, 0f) + Main.rand.RandomPointInArea(10), Vector2.Zero, 16);
        }
        if (Main.rand.NextBool(50 + (int)(50 * (1f - Projectile.Opacity)))) {
            int type = DustID.YellowStarDust;
            int size = 20;
            Dust.NewDust(position - Vector2.One * size / 2f, size, size, type, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, 0f));
        }

        _flowerGrowthFactor = Helper.Approach(_flowerGrowthFactor, 1f, TimeSystem.LogicDeltaTime * 1.5f * Ease.QuadOut(GrowFactorValue));

        float to = GLOWTO;
        _glowFactor = Helper.Approach(_glowFactor, to, TimeSystem.LogicDeltaTime);
        if (_glowFactor >= to) {
            _glowFactor = 0f;
        }
    }

    protected override void Draw(ref Color lightColor) {
        if (!AssetInitializer.TryGetRequestedTextureAssets<Starfruit>(out Dictionary<byte, Asset<Texture2D>> indexedTextureAssets)) {
            return;
        }

        SpriteBatch batch = Main.spriteBatch;

        Texture2D stemTexture = indexedTextureAssets[(byte)StarfruitRequstedTextureType.Stem].Value,
                  topTexture = indexedTextureAssets[(byte)StarfruitRequstedTextureType.Top].Value,
                  glowTexture = indexedTextureAssets[(byte)StarfruitRequstedTextureType.Glow].Value;

        byte stemRowCount = 5,
             stemColumnCount = 3;
        Vector2 startPosition = _startPosition,
                endPosition = _growToPosition;

        float opacity = Ease.QuintOut(GrowFactorValue);

        float maxOffsetX = 4f;
        endPosition.X += Helper.Wave(-maxOffsetX, maxOffsetX, 1f, Projectile.identity) * opacity;

        int stemCurrentIndex = 0;

        Vector2 velocity = startPosition.DirectionTo(endPosition);

        float velocityLerpValue = 0.05f;

        int stemCount = StemLength;

        int direction = Projectile.direction;

        float scaleFactor = Projectile.scale * opacity;
        scaleFactor = MathF.Max(0.1f, scaleFactor);
        Vector2 scale = Vector2.One * scaleFactor;

        float globalOpacity = Projectile.Opacity;

        ulong seed = (ulong)_seed;
        while (true) {
            if (stemCurrentIndex >= stemCount) {
                break;
            }

            byte stemCurrentRow = (byte)Utils.RandomInt(ref seed, 5),
                 stemCurrentColumn = (byte)(stemCurrentIndex == 0 ? 2 : 1);
            Rectangle stemClip = Utils.Frame(stemTexture, stemRowCount, stemColumnCount, stemCurrentRow, stemCurrentColumn);

            float step = stemClip.Height - 4;
            step *= scaleFactor;

            bool last = stemCurrentIndex == stemCount - 1;
            if (last) {
                stemCurrentColumn = 0;
                stemClip = Utils.Frame(stemTexture, stemRowCount, stemColumnCount, stemCurrentRow, stemCurrentColumn);
            }

            velocity = Vector2.Lerp(velocity, velocity.RotatedBy(MathHelper.PiOver4 * 0.75f * direction), velocityLerpValue);

            Vector2 position = startPosition;
            Color color = Lighting.GetColor(position.ToTileCoordinates()) * opacity;

            float rotation = velocity.ToRotation() + MathHelper.PiOver2;

            SpriteEffects stemFlip = (Utils.RandomInt(ref seed, 2) == 0).ToSpriteEffects();

            Vector2 stemOrigin = stemClip.Centered();
            DrawInfo stemDrawInfo = new() {
                Clip = stemClip,
                Origin = stemOrigin,
                Rotation = rotation,
                ImageFlip = stemFlip,
                Color = color * globalOpacity,
                Scale = scale
            };
            batch.Draw(stemTexture, position, stemDrawInfo);

            if (last) {
                Rectangle topClip = Utils.Frame(topTexture, 1, 3);
                Vector2 topOrigin = topClip.Centered();
                Vector2 topPosition = position - Vector2.UnitY.RotatedBy(rotation) * step * 1f;
                SpriteEffects topFlip = (Utils.RandomInt(ref seed, 2) == 0).ToSpriteEffects();
                DrawInfo topDrawInfo = new() {
                    Clip = topClip,
                    Origin = topOrigin,
                    Rotation = rotation,
                    ImageFlip = topFlip,
                    Color = color * globalOpacity,
                    Scale = scale
                };
                batch.Draw(topTexture, topPosition, topDrawInfo);

                Rectangle topClip2 = Utils.Frame(topTexture, 1, 3, frameY: 1);
                batch.Draw(topTexture, topPosition, topDrawInfo.WithScale(1f - _flowerGrowthFactor) with {
                    Clip = topClip2
                });

                float glowWaveFactor = Helper.Wave(0f, 1f, 10f, Projectile.identity),
                      glowWaveFactor2 = Helper.Wave(0f, 1f, 10f, Projectile.identity + 1f),
                      glowWaveFactor3 = Helper.Wave(0f, 1f, 2f, Projectile.identity + 3f);

                Color glowColor = Color.White * opacity;

                Vector2 glowPosition = topPosition - Vector2.UnitY.RotatedBy(rotation) * step * 0.5f;
                Rectangle glowClip = glowTexture.Bounds;
                Vector2 glowOrigin = glowClip.Centered();
                float glowColorFactor = MathHelper.Lerp(0.725f, 0.775f, glowWaveFactor2) * 0.75f;
                Color glowColor2 = glowColor.MultiplyRGB(Color.Lerp(new Color(252, 232, 154), new Color(255, 214, 56), glowWaveFactor)).MultiplyAlpha(0f) * glowColorFactor;
                glowColor2 *= Utils.GetLerpValue(0f, 0.25f, _glowFactor, true);
                glowColor2 *= 1f - Utils.GetLerpValue(GLOWTO * 0.85f, GLOWTO, _glowFactor, true);
                float glowScaleFactor = MathHelper.Lerp(0.65f, 0.775f, glowWaveFactor3);
                Vector2 glowScale = scale * (Utils.Remap(_glowFactor, 0f, GLOWTO, 1f, 0.5f, true));
                DrawInfo glowDrawInfo = new() {
                    Clip = glowClip,
                    Origin = glowOrigin,
                    Rotation = rotation,
                    ImageFlip = topFlip,
                    Color = glowColor2 * globalOpacity,
                    Scale = glowScale
                };
                batch.Draw(glowTexture, glowPosition, glowDrawInfo.WithScale(Ease.CubeOut(_flowerGrowthFactor) * glowScaleFactor));

                topClip2 = Utils.Frame(topTexture, 1, 3, frameY: 2);
                batch.Draw(topTexture, topPosition, topDrawInfo.WithScale(Ease.CubeOut(_flowerGrowthFactor)) with {
                    Clip = topClip2,
                    Color = Color.Lerp(color, glowColor, 0.875f).MultiplyAlpha(MathHelper.Lerp(0.75f, 1f, glowWaveFactor3)) * 1f * globalOpacity
                });
            }
            startPosition += velocity * step;

            velocityLerpValue = Helper.Approach(velocityLerpValue, GrowFactor, 0.2f);

            stemCurrentIndex++;
        }
    }
}
