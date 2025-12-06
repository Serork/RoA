using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.DataStructures;

namespace RoA.Content.Projectiles.Friendly.Nature;

sealed class Mist : NatureProjectile {
    private static byte MISTCOUNT => 200;

    private struct MistInfo {
        public Vector2 Position, Offset;
        public float OffsetSpeed;
        public byte Index;
        public Color Color;
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

    protected override void SafeSetDefaults() {
        Projectile.SetSizeValues(10);

        Projectile.friendly = true;
        Projectile.aiStyle = -1;

        Projectile.tileCollide = false;

        Projectile.manualDirectionChange = true;

        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
        behindProjectiles.Add(index);
    }

    public override void AI() {
        float maxFlameSpawn = 5f,
              minFlameSpawn = 2f;
        if (FlameSpawnValue++ > FlameSpawnValue2) {
            FlameSpawnValue = 0f;

            if (Projectile.IsOwnerLocal()) {
                ProjectileUtils.SpawnPlayerOwnedProjectile<MistFlame>(new ProjectileUtils.SpawnProjectileArgs(Projectile.GetOwnerAsPlayer(), Projectile.GetSource_FromAI()) {
                    Position = _flamePositions[_currentFlameIndex].Item2 + Main.rand.RandomPointInArea(_flamePositions[_currentFlameIndex].Item1),
                    Damage = Projectile.damage,
                    KnockBack = Projectile.knockBack
                });
            }

            FlameSpawnValue2 -= 0.1f;
            if (FlameSpawnValue2 < minFlameSpawn) {
                FlameSpawnValue2 = maxFlameSpawn;
            }

            _currentFlameIndex++;
            if (_currentFlameIndex > _flamePositions.Count - 1) {
                _currentFlameIndex = 0;
            }
        }

        if (!Init) {
            Init = true;

            FlameSpawnValue2 = maxFlameSpawn;

            _mists = new MistInfo[MISTCOUNT];
            _flamePositions = [];

            Projectile.velocity = Projectile.velocity.NormalizeWithMaxLength(17.5f);

            Projectile.direction = Projectile.velocity.X.GetDirection();

            if (Projectile.IsOwnerLocal()) {
                WaveOffset = Main.rand.NextFloat(10f);

                Projectile.netUpdate = true;
            }
        }

        MistOffset = Helper.Approach(MistOffset, 36f, 3f);

        float spawnFor = 40f;
        if (SpawnValue < spawnFor) {
            SpawnValue++;

            if (SpawnValue % 5 == 0) {
                _flamePositions.Add((MistOffset * 0.75f, Projectile.Center));
            }

            for (int i = 0; i < 5; i++) {
                SpawnMist();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
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
            float to = 0.25f;
            currentSegmentData.Opacity = Helper.Approach(currentSegmentData.Opacity, to, 0.1f);
            float maxRotation = 0.1f;
            currentSegmentData.Rotation = Helper.Wave(-maxRotation, maxRotation, 2.5f, currentSegmentData.Index);
        }
    }

    private void SpawnMist() {
        if (_currentMistIndex > MISTCOUNT - 1) {
            _currentMistIndex = 0;
        }
        float rotationDirection = Main.rand.NextFloatDirection();
        _mists[_currentMistIndex] = new MistInfo() {
            Index = (byte)_currentMistIndex,
            Position = Projectile.Center + Main.rand.RandomPointInArea(MistOffset),
            Offset = Main.rand.NextVector2() * 2.5f,
            OffsetSpeed = Main.rand.NextFloat(5f),
            Scale = 1f + Main.rand.NextFloatRange(0.2f),
            Opacity = 0f,
            Color = Color.Lerp(Color.White, Color.Lerp(Color.Gray, Color.DarkGray, Main.rand.NextFloat(1f)), 0.1f),
            FrameIndex = (byte)Main.rand.Next(3),
            Rotation = Projectile.rotation/*MathHelper.TwoPi * Main.rand.NextFloat()*/,
            RotationSpeed = MathF.Max(0.005f * rotationDirection.GetDirection(), MathHelper.PiOver4 * 0.01f * rotationDirection)
        };
        _currentMistIndex++;
    }

    public override void OnKill(int timeLeft) {

    }

    public override bool PreDraw(ref Color lightColor) {
        SpriteBatch batch = Main.spriteBatch;
        Texture2D texture = Projectile.GetTexture();
        SpriteFrame spriteFrame = new(1, 3);
        foreach (MistInfo mist in _mists) {
            Vector2 offset = mist.Offset * Helper.Wave(-1f, 1f, mist.OffsetSpeed, mist.Index);
            Vector2 position = mist.Position + offset;
            Color color = mist.Color.MultiplyRGB(Lighting.GetColor(position.ToTileCoordinates())) * mist.Opacity;
            Rectangle clip = spriteFrame.With(0, mist.FrameIndex).GetSourceRectangle(texture);
            Vector2 origin = clip.Centered();
            float rotation = mist.Rotation;
            Vector2 scale = Vector2.One * mist.Scale;
            DrawInfo mistDrawInfo = DrawInfo.Default with {
                Clip = clip,
                Origin = origin,
                Color = color,
                Scale = scale,
                Rotation = rotation
            };
            batch.Draw(texture, position, mistDrawInfo);
        }

        return false;
    }
}
