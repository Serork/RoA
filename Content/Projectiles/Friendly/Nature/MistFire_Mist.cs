using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Defaults;
using RoA.Core.Graphics.Data;
using RoA.Core.Utility;

using System;

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
    private int _currentMistIndex;

    public ref float InitValue => ref Projectile.localAI[0];
    public ref float SpawnValue => ref Projectile.ai[0];
    public ref float MistOffsetValue => ref Projectile.ai[1];
    public ref float WaveOffsetValue => ref Projectile.ai[2];

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
    }

    public override void AI() {
        if (!Init) {
            Init = true;

            _mists = new MistInfo[MISTCOUNT];

            Projectile.velocity = Projectile.velocity.NormalizeWithMaxLength(17.5f);

            Projectile.direction = Projectile.velocity.X.GetDirection();

            if (Projectile.IsOwnerLocal()) {
                WaveOffsetValue = Main.rand.NextFloat(10f);

                Projectile.netUpdate = true;
            }
        }

        MistOffsetValue = Helper.Approach(MistOffsetValue, 25f, 2f);

        float spawnFor = 40f;
        if (SpawnValue < spawnFor) {
            SpawnValue++;
            for (int i = 0; i < 5; i++) {
                SpawnMist();
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            float maxOffset = 7.5f;
            Projectile.position += Vector2.One.RotatedBy(Projectile.rotation) * Helper.Wave(-maxOffset, maxOffset, MistOffsetValue, WaveOffsetValue) * Projectile.direction;
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
            currentSegmentData.Rotation += currentSegmentData.RotationSpeed;
        }
    }

    private void SpawnMist() {
        if (_currentMistIndex > MISTCOUNT - 1) {
            _currentMistIndex = 0;
        }
        float rotationDirection = Main.rand.NextFloatDirection();
        _mists[_currentMistIndex] = new MistInfo() {
            Index = (byte)_currentMistIndex,
            Position = Projectile.Center + Main.rand.RandomPointInArea(MistOffsetValue),
            Offset = Main.rand.NextVector2() * 2.5f,
            OffsetSpeed = Main.rand.NextFloat(5f),
            Scale = 1f + Main.rand.NextFloatRange(0.2f),
            Opacity = 0f,
            Color = Color.Lerp(Color.White, Color.Lerp(Color.Gray, Color.DarkGray, Main.rand.NextFloat(0.5f)), 0.375f),
            FrameIndex = (byte)Main.rand.Next(3),
            Rotation = MathHelper.TwoPi * Main.rand.NextFloat(),
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
            Color color = mist.Color * mist.Opacity;
            Rectangle clip = spriteFrame.With(0, mist.FrameIndex).GetSourceRectangle(texture);
            Vector2 origin = clip.Centered();
            Vector2 scale = Vector2.One * mist.Scale;
            float rotation = mist.Rotation;
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
