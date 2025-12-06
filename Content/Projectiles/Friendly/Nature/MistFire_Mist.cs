using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
using Terraria.Graphics.Light;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace RoA.Content.Projectiles.Friendly.Nature;

[Tracked]
sealed class Mist : NatureProjectile {
    private static ushort TIMELEFT => 325;
    private static byte MISTCOUNT => 200;

    private struct MistInfo {
        public Vector2 Position, Offset;
        public float OffsetSpeed, CenterProgress;
        public byte Index;
        public Color OriginalColor, Color, GoToColor;
        public float GoToColorFactor;
        public float Scale;
        public float Opacity;
        public byte FrameIndex;
        public float Rotation, RotationSpeed;
    }

    private MistInfo[] _mists = null!;
    private List<(float, Vector2)> _flamePositions = null!;
    private byte _currentMistIndex, _currentFlameIndex;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float FlameSpawnValue => ref Projectile.localAI[1];
    public ref float FlameSpawnValue2 => ref Projectile.localAI[2];
    public ref float SpawnValue => ref Projectile.ai[0];
    public ref float MistOffset => ref Projectile.ai[1];
    public ref float WaveOffset => ref Projectile.ai[2];

    public bool Init {
        get => InitValue == 1f;
        set => InitValue = value.ToInt();
    }

    public override void Load() {
        On_TileLightScanner.ApplyWallLight += On_TileLightScanner_ApplyWallLight;
    }

    private void On_TileLightScanner_ApplyWallLight(On_TileLightScanner.orig_ApplyWallLight orig, TileLightScanner self, Tile tile, int x, int y, ref Terraria.Utilities.FastRandom localRandom, ref Vector3 lightColor) {
        orig(self, tile, x, y, ref localRandom, ref lightColor);

    }

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;

        Projectile.manualDirectionChange = true;

        Projectile.hide = true;

        Projectile.timeLeft = TIMELEFT;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindProjectiles.Add(index);
    }

    public override void AI() {
        Projectile.Opacity = Ease.CubeIn(Utils.GetLerpValue(0, 40, Projectile.timeLeft, true));

        float maxFlameSpawn = 4f,
              minFlameSpawn = 3f;
        if (Init) {
            FlameSpawnValue += 0.5f;
            if (Projectile.timeLeft > 110 && FlameSpawnValue > FlameSpawnValue2 && Projectile.Opacity >= 1f) {
                FlameSpawnValue = 0f;

                float ai0 = Main.rand.Next(3);
                if (Projectile.IsOwnerLocal()) {
                    ProjectileUtils.SpawnPlayerOwnedProjectile<IgnisFatuus>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromAI()) {
                        Position = _flamePositions[_currentFlameIndex].Item2 + Main.rand.RandomPointInArea(_flamePositions[_currentFlameIndex].Item1),
                        Damage = Projectile.damage,
                        KnockBack = Projectile.knockBack,
                        Velocity = Vector2.UnitY.RotatedByRandom(MathHelper.TwoPi),
                        AI0 = ai0,
                        AI2 = Projectile.rotation - MathHelper.PiOver2
                    });
                }
                int count = _mists.Length;
                int step = count / _flamePositions.Count;
                for (int i = _currentFlameIndex * step; i < (_currentFlameIndex + 1) * step; i++) {
                    int i2 = Utils.Clamp(i, 0, count - 1);
                    _mists[i2].GoToColorFactor = 1f;
                    _mists[i2].GoToColor = IgnisFatuus.GetLightColor((int)ai0);
                }

                FlameSpawnValue2 -= 0.5f;
                if (FlameSpawnValue2 < minFlameSpawn) {
                    FlameSpawnValue2 = minFlameSpawn;
                }

                _currentFlameIndex++;
                if (_currentFlameIndex > _flamePositions.Count - 1) {
                    _currentFlameIndex = 0;
                }
            }
        }

        if (!Init) {
            Init = true;

            FlameSpawnValue2 = maxFlameSpawn;

            _mists = new MistInfo[MISTCOUNT];
            _flamePositions = [];

            Projectile.velocity = Projectile.velocity.NormalizeWithMaxLength(17.5f);

            Projectile.SetDirection(Projectile.velocity.X.GetDirection());

            if (Projectile.IsOwnerLocal()) {
                WaveOffset = Main.rand.NextFloat(10f);

                Projectile.netUpdate = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();

            MistOffset = 12f;
        }

        MistOffset = Helper.Approach(MistOffset, 36f, 3f);

        float spawnFor = 40f;
        if (SpawnValue < spawnFor) {
            SpawnValue++;

            if (SpawnValue % maxFlameSpawn == 0) {
                _flamePositions.Add((MistOffset * 0.75f, Projectile.Center));
            }

            for (int i = 0; i < 5; i++) {
                SpawnMist();
            }

            float maxOffset = 5f;
            Projectile.position += Vector2.One.RotatedBy(Projectile.rotation) * Helper.Wave(-maxOffset, maxOffset, MistOffset, WaveOffset) * Projectile.direction;
        }
        if (SpawnValue > spawnFor * 0.85f) {
            Projectile.velocity *= 0.8f;
        }

        for (int i = 0; i < _mists.Length; i++) {
            int currentSegmentIndex = i,
                previousSegmentIndex = Math.Max(0, i - 1);
            ref MistInfo currentSegmentData = ref _mists[currentSegmentIndex],
                         previousSegmentData = ref _mists[previousSegmentIndex];
            float to = 0.5f;
            currentSegmentData.Opacity = Helper.Approach(currentSegmentData.Opacity, to, 0.1f);
            float maxRotation = 0.1f;
            currentSegmentData.RotationSpeed = Helper.Wave(-maxRotation, maxRotation, 2.5f, currentSegmentData.Index);
            currentSegmentData.GoToColorFactor = Helper.Approach(currentSegmentData.GoToColorFactor, 0f, TimeSystem.LogicDeltaTime);
            if (currentSegmentData.GoToColorFactor > 0.1f) {
                currentSegmentData.Color = Color.Lerp(currentSegmentData.Color, Color.Lerp(currentSegmentData.OriginalColor, currentSegmentData.GoToColor, 0.2f * currentSegmentData.CenterProgress), 0.1f);
            }
            else {
                currentSegmentData.Color = Color.Lerp(currentSegmentData.Color, currentSegmentData.OriginalColor, 0.025f);
            }

            if (SpawnValue >= spawnFor && currentSegmentData.Opacity >= 0.1f && Projectile.Opacity >= 0.75f) {
                if (Main.rand.NextBool(30)) {
                    float dustCount = 3;
                    for (int k = 0; k < dustCount; k++) {
                        float distanceToCenter = 0.4f + 0.8f * (float)Math.Pow(Main.rand.NextFloat(), 0.4f);
                        float dustRotation = Projectile.rotation - MathHelper.PiOver2;
                        Vector2 dustVelocity = (dustRotation + MathHelper.PiOver2).ToRotationVector2() * (distanceToCenter * 7f + 3f);
                        Dust dust = Dust.NewDustPerfect(currentSegmentData.Position + Main.rand.RandomPointInArea(10f) - Projectile.velocity * Main.rand.NextFloat(3.5f, 10f) + dustRotation.ToRotationVector2() * distanceToCenter, DustID.Sand, Vector2.Zero, 240);
                        dust.color = new(74, 74, 74);
                        dust.velocity = dustVelocity * 0.375f;
                        dust.noGravity = true;
                        dust.fadeIn = 0f;
                    }
                }
            }
        }
    }

    private void SpawnMist() {
        if (_currentMistIndex > MISTCOUNT - 1) {
            _currentMistIndex = 0;
        }
        float rotationDirection = Main.rand.NextFloatDirection();
        Color color = Color.Lerp(Color.White, Color.Lerp(Color.Gray, Color.DarkGray, Main.rand.NextFloat(1f)), 0.75f);
        _mists[_currentMistIndex] = new MistInfo() {
            Index = _currentMistIndex,
            CenterProgress = Vector2.Distance(Projectile.Center, Projectile.Center + Main.rand.RandomPointInArea(MistOffset)) / 36f,
            Position = Projectile.Center + Main.rand.RandomPointInArea(MistOffset),
            Offset = Main.rand.NextVector2() * 2.5f,
            OffsetSpeed = Main.rand.NextFloat(5f),
            Scale = 1f + Main.rand.NextFloatRange(0.2f),
            Opacity = 0f,
            Color = color,
            OriginalColor = color.ModifyRGB(0.4f),
            FrameIndex = (byte)Main.rand.Next(3),
            Rotation = Projectile.rotation + MathHelper.Pi/*MathHelper.TwoPi * Main.rand.NextFloat()*/
        };
        if (_currentMistIndex > 25) {
            float dustCount = 4;
            for (int i = 0; i < dustCount; i++) {
                if (Main.rand.NextBool()) {
                    continue;
                }
                float distanceToCenter = 0.4f + 0.8f * (float)Math.Pow(Main.rand.NextFloat(), 0.4f);
                float dustRotation = Projectile.rotation - MathHelper.PiOver2;
                Vector2 dustVelocity = (dustRotation + MathHelper.PiOver2).ToRotationVector2() * (distanceToCenter * 7f + 3f);
                Dust dust = Dust.NewDustPerfect(_mists[_currentMistIndex].Position + Main.rand.RandomPointInArea(10f) - Projectile.velocity * Main.rand.NextFloat(3.5f, 10f) + dustRotation.ToRotationVector2() * distanceToCenter, DustID.Sand, Vector2.Zero, 240);
                dust.color = new(74, 74, 74);
                dust.velocity = dustVelocity;
                dust.noGravity = true;
                dust.fadeIn = 0f;
            }
        }
        _currentMistIndex++;

    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        SpriteFrame spriteFrame = new(1, 3);
        SpriteEffects effects = Projectile.FacedRight() ? SpriteEffects.FlipVertically : SpriteEffects.None;
        foreach (MistInfo mist in _mists) {
            Vector2 offset = mist.Offset * Helper.Wave(-1f, 1f, mist.OffsetSpeed, mist.Index);
            Vector2 position = mist.Position + offset;
            Color color = mist.Color.MultiplyRGB(Lighting.GetColor(position.ToTileCoordinates())) * mist.Opacity * Projectile.Opacity;
            Rectangle clip = spriteFrame.With(0, mist.FrameIndex).GetSourceRectangle(texture);
            Vector2 origin = clip.Centered();
            float rotation = mist.Rotation + mist.RotationSpeed;
            Vector2 scale = Vector2.One * mist.Scale;
            DrawInfo mistDrawInfo = DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Scale = scale,
                Rotation = rotation,
                ImageFlip = effects
            };

            batch.Draw(texture, position, mistDrawInfo);

            Texture2D bloom = ResourceManager.Bloom;
            Rectangle bloomClip = bloom.Bounds;
            Vector2 bloomOrigin = bloomClip.Centered();
            Color bloomColor = Color.Black * mist.Opacity * 0.35f * Projectile.Opacity;
            batch.Draw(bloom, position, DrawInfo.Default with {
                Clip = bloomClip,
                Origin = bloomOrigin,
                Color = bloomColor,
                Scale = scale * 0.5f,
                Rotation = rotation
            });
        }

        return false;
    }
}
