using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.BackwoodsSystems;
using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Common.CustomSkyAmbience;

sealed class FlederSkyEntity : FadingSkyEntity {
    private const int STATE_ZIGZAG = 1;
    private const int STATE_GOOVERPLAYER = 2;
    private int _state;
    private int _direction;
    private float _waviness;

    public FlederSkyEntity(Player player, FastRandom random) {
        VirtualCamera camera = new VirtualCamera(player);
        Texture = random.NextFloat() < 0.5f ? ModContent.Request<Texture2D>(ResourceManager.AmbienceTextures + "Fleder") : ModContent.Request<Texture2D>(ResourceManager.AmbienceTextures + "Fleder2");
        Frame = new SpriteFrame(1, 10);

        BeginZigZag(ref random, camera, (random.Next(2) == 1) ? 1 : (-1));

        SetPositionInWorldBasedOnScreenSpace(Position);
        OpacityNormalizedTimeToFadeIn = 0.1f;
        OpacityNormalizedTimeToFadeOut = 0.9f;
        BrightnessLerper = 0.2f;
        FinalOpacityMultiplier = 1f;
        FramingSpeed = 5;

        Scale = 1.5f;
    }

    private void BeginZigZag(ref FastRandom random, VirtualCamera camera, int direction) {
        _state = 1;
        LifeTime = random.Next(18, 31) * 60;
        _waviness = (random.NextFloat() * 1f + 1f) * 1f;
        float surfacePosition = (float)BackwoodsVars.BackwoodsTileForBackground - 1;
        if (surfacePosition == 0f) {
            surfacePosition = 1f;
        }
        Depth = MathHelper.Lerp(1.75f, 4f, random.NextFloat());
        bool flag2 = Depth <= 2.2f;
        Position.Y = MathHelper.Clamp(random.NextFloat(), flag2 ? 0.4f : 0.6f, 0.85f) * ((float)surfacePosition * 16f - 1600f) + 2400f;

        _direction = (random.NextFloat() < 0.5f).ToDirectionInt();
        Effects = _direction.ToSpriteEffects();

        int num = Main.screenWidth;
        if (Effects == SpriteEffects.FlipHorizontally)
            Position.X = camera.Position.X + camera.Size.X + num;
        else
            Position.X = camera.Position.X - num;
    }

    private void BeginChasingPlayer(ref FastRandom random, VirtualCamera camera) {
        _state = 2;
        LifeTime = random.Next(18, 31) * 60;
        Position = camera.Position + camera.Size * new Vector2(random.NextFloat(), random.NextFloat());
    }

    public override void UpdateVelocity(int frameCount) {
        switch (_state) {
            case 1:
                ZigzagMove(frameCount);
                break;
            case 2:
                ChasePlayerTop(frameCount);
                break;
        }

        Rotation = Velocity.X * 0.05f;
    }

    private void ZigzagMove(int frameCount) {
        Velocity = new Vector2(_direction * 3, (float)Math.Cos((float)frameCount / 1200f * ((float)Math.PI * 2f)) * _waviness);
    }

    private void ChasePlayerTop(int frameCount) {
        Vector2 vector = Main.LocalPlayer.Center + new Vector2(0f, -500f) - Position;
        if (vector.Length() >= 100f) {
            Velocity.X += 0.1f * (float)Math.Sign(vector.X);
            Velocity.Y += 0.1f * (float)Math.Sign(vector.Y);
            Velocity = Vector2.Clamp(Velocity, new Vector2(-18f), new Vector2(18f));
        }
    }
}
