using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.BackwoodsSystems;
using RoA.Core;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Common.CustomSkyAmbience;

class BackwoodsBirdsPackSkyEntity : FadingSkyEntity {
    public BackwoodsBirdsPackSkyEntity(Player player, FastRandom random) {
        VirtualCamera virtualCamera = new VirtualCamera(player);
        Effects = !(Main.WindForVisuals > 0f) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        int num = Main.screenWidth;
        if (Effects == SpriteEffects.FlipHorizontally)
            Position.X = virtualCamera.Position.X + virtualCamera.Size.X + num;
        else
            Position.X = virtualCamera.Position.X - num;

        float surfacePosition = (float)BackwoodsVars.BackwoodsTileForBackground - 1;
        if (surfacePosition == 0f) {
            surfacePosition = 1f;
        }
        Depth = Main.rand.NextFloat(1.75f, 4f);
        bool flag2 = Depth <= 2.2f;
        Position.Y = MathHelper.Clamp(random.NextFloat(), flag2 ? 0.4f : 0.6f, 0.85f) * ((float)surfacePosition * 16f - 1600f) + 2400f;
        SetPositionInWorldBasedOnScreenSpace(Position);
        Texture = random.NextFloat() < 0.35f ? ModContent.Request<Texture2D>(ResourceManager.Textures + "Ambience/BirdsVShape") : Main.Assets.Request<Texture2D>("Images/Backgrounds/Ambience/BirdsVShape");
        Frame = new SpriteFrame(1, 4);
        LifeTime = random.Next(60, 121) * 60;
        OpacityNormalizedTimeToFadeIn = 0.085f;
        OpacityNormalizedTimeToFadeOut = 0.915f;
        BrightnessLerper = 0.2f;
        FinalOpacityMultiplier = 1f;
        FramingSpeed = 5;
    }

    public override void UpdateVelocity(int frameCount) {
        float num = 3f + Math.Abs(Main.WindForVisuals) * 0.8f;
        Velocity = new Vector2(num * (Effects != SpriteEffects.FlipHorizontally ? 1 : -1), 0f);
    }

    public override void Update(int frameCount) {
        base.Update(frameCount);
        if (!Main.dayTime || Main.eclipse)
            StartFadingOut(frameCount);
    }
}
