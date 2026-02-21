using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Core;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Common.CustomSkyAmbience;

sealed class LittleFlederSkyEntity : FadingSkyEntity {
    private const int STATE_ZIGZAG = 1;
    private const int STATE_GOOVERPLAYER = 2;
    private int _state;
    private int _direction;
    private float _waviness;

    public LittleFlederSkyEntity(Player player, FastRandom random) {
        VirtualCamera camera = new VirtualCamera(player);
        Texture = ModContent.Request<Texture2D>(ResourceManager.Textures + "Ambience/LittleFleder");
        Frame = new SpriteFrame(1, 9);
        Depth = random.NextFloat() * 3f + 4.5f;

        BeginChasingPlayer(ref random, camera);

        SetPositionInWorldBasedOnScreenSpace(Position);
        OpacityNormalizedTimeToFadeIn = 0.1f;
        OpacityNormalizedTimeToFadeOut = 0.9f;
        BrightnessLerper = 0.2f;
        FinalOpacityMultiplier = 1f;
        FramingSpeed = 5;
    }

    private void BeginZigZag(ref FastRandom random, VirtualCamera camera, int direction) {
        _state = 1;
        LifeTime = random.Next(18, 31) * 60;
        _direction = direction;
        _waviness = random.NextFloat() * 1f + 1f;
        Position.Y = camera.Position.Y;
        int num = 100;
        if (_direction == 1)
            Position.X = camera.Position.X - (float)num;
        else
            Position.X = camera.Position.X + camera.Size.X + (float)num;
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

        Rotation = Velocity.X * 0.1f;
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
