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

sealed class LeafSkyEntity : FadingSkyEntity {
    private const int STATE_ZIGZAG = 1;
    private const int STATE_GOOVERPLAYER = 2;
    private int _state;
    private int _direction;
    private float _waviness;

    public LeafSkyEntity(Player player, FastRandom random) {
        VirtualCamera camera = new VirtualCamera(player);
        Texture = ModContent.Request<Texture2D>(ResourceManager.AmbienceTextures + "Leaf");
        Frame = new SpriteFrame(1, 8);

        BeginZigZag(ref random, camera, (random.Next(2) == 1) ? 1 : (-1));

        SetPositionInWorldBasedOnScreenSpace(Position);
        OpacityNormalizedTimeToFadeIn = 0.1f;
        OpacityNormalizedTimeToFadeOut = 0.9f;
        BrightnessLerper = 0.2f;
        FinalOpacityMultiplier = 1f;
        FramingSpeed = 5;

        Scale = 1.75f;
    }

    private void BeginZigZag(ref FastRandom random, VirtualCamera camera, int direction) {
        _state = 1;
        LifeTime = random.Next(18, 31) * 120;
        _waviness = (random.NextFloat() * 1f + 1f) * 1f;
        float surfacePosition = (float)BackwoodsVars.BackwoodsTileForBackground - 1;
        if (surfacePosition == 0f) {
            surfacePosition = 1f;
        }
        Depth = MathHelper.Lerp(2.5f, 4f, random.NextFloat());
        bool flag2 = Depth <= 2.2f;
        Position.Y = MathHelper.Clamp(random.NextFloat(), flag2 ? 0.4f : 0.6f, 0.85f) * ((float)surfacePosition * 16f - 1600f) + 1200f;

        _direction = direction;
        Effects = _direction.ToSpriteEffects();

        int num = Main.screenWidth;
        Position.X = MathHelper.Lerp(camera.Position.X - num, camera.Position.X + camera.Size.X + num, random.NextFloat());
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

        Rotation = Velocity.ToRotation() - MathHelper.PiOver2;
    }

    private void ZigzagMove(int frameCount) {
        Velocity = new Vector2((float)Math.Cos((float)frameCount / 1200f * ((float)Math.PI * 2f)) * _waviness, 3);
        Velocity.X += Main.windSpeedCurrent * Main.windPhysicsStrength * 30f;
        Velocity.X = MathHelper.Clamp(Velocity.X, -30f, 30f);
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
