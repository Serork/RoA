using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;
using ReLogic.Utilities;

using RoA.Common.BackwoodsSystems;
using RoA.Core;

using System;

using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Ambience;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Common.CustomSkyAmbience;

sealed class CustomAmbientSky : CustomSky {
    private abstract class SkyEntity {
        public Vector2 Position;
        public Asset<Texture2D> Texture;
        public SpriteFrame Frame;
        public float Depth;
        public SpriteEffects Effects;
        public bool IsActive = true;
        public float Rotation;

        public Rectangle SourceRectangle => Frame.GetSourceRectangle(Texture.Value);

        protected void NextFrame() {
            Frame.CurrentRow = (byte)((Frame.CurrentRow + 1) % Frame.RowCount);
        }

        public abstract Color GetColor(Color backgroundColor);

        public abstract void Update(int frameCount);

        protected void SetPositionInWorldBasedOnScreenSpace(Vector2 actualWorldSpace) {
            Vector2 vector = actualWorldSpace - Main.Camera.Center;
            Vector2 position = Main.Camera.Center + vector * (Depth / 3f);
            Position = position;
        }

        public abstract Vector2 GetDrawPosition();

        public virtual void Draw(SpriteBatch spriteBatch, float depthScale, float minDepth, float maxDepth) {
            CommonDraw(spriteBatch, depthScale, minDepth, maxDepth);
        }

        public void CommonDraw(SpriteBatch spriteBatch, float depthScale, float minDepth, float maxDepth) {
            if (!(Depth <= minDepth) && !(Depth > maxDepth)) {
                Vector2 drawPositionByDepth = GetDrawPositionByDepth();
                Color color = GetColor(Main.ColorOfTheSkies) * Main.atmo;
                Vector2 origin = SourceRectangle.Size() / 2f;
                float scale = depthScale / Depth;
                spriteBatch.Draw(Texture.Value, drawPositionByDepth - Main.Camera.UnscaledPosition, SourceRectangle, color, Rotation, origin, scale, Effects, 0f);
            }
        }

        internal Vector2 GetDrawPositionByDepth() => (GetDrawPosition() - Main.Camera.Center) * new Vector2(1f / Depth, 0.9f / Depth) + Main.Camera.Center;

        internal float Helper_GetOpacityWithAccountingForOceanWaterLine() {
            Vector2 vector = GetDrawPositionByDepth() - Main.Camera.UnscaledPosition;
            int num = SourceRectangle.Height / 2;
            float t = vector.Y + num;
            float yScreenPosition = AmbientSkyDrawCache.Instance.OceanLineInfo.YScreenPosition;
            float lerpValue = Utils.GetLerpValue(yScreenPosition - 10f, yScreenPosition - 2f, t, clamped: true);
            lerpValue *= AmbientSkyDrawCache.Instance.OceanLineInfo.OceanOpacity;
            return 1f - lerpValue;
        }
    }

    private class FadingSkyEntity : SkyEntity {
        protected int LifeTime;
        protected Vector2 Velocity;
        protected int FramingSpeed;
        protected int TimeEntitySpawnedIn;
        protected float Opacity;
        protected float BrightnessLerper;
        protected float FinalOpacityMultiplier;
        protected float OpacityNormalizedTimeToFadeIn;
        protected float OpacityNormalizedTimeToFadeOut;
        protected int FrameOffset;

        public FadingSkyEntity() {
            Opacity = 0f;
            TimeEntitySpawnedIn = -1;
            BrightnessLerper = 0f;
            FinalOpacityMultiplier = 1f;
            OpacityNormalizedTimeToFadeIn = 0.1f;
            OpacityNormalizedTimeToFadeOut = 0.9f;
        }

        public override void Update(int frameCount) {
            if (!IsMovementDone(frameCount)) {
                UpdateOpacity(frameCount);
                if ((frameCount + FrameOffset) % FramingSpeed == 0)
                    NextFrame();

                UpdateVelocity(frameCount);
                Position += Velocity;
            }
        }

        public virtual void UpdateVelocity(int frameCount) {
        }

        private void UpdateOpacity(int frameCount) {
            int num = frameCount - TimeEntitySpawnedIn;
            if (num >= LifeTime * OpacityNormalizedTimeToFadeOut)
                Opacity = Utils.GetLerpValue(LifeTime, LifeTime * OpacityNormalizedTimeToFadeOut, num, clamped: true);
            else
                Opacity = Utils.GetLerpValue(0f, LifeTime * OpacityNormalizedTimeToFadeIn, num, clamped: true);
        }

        private bool IsMovementDone(int frameCount) {
            if (TimeEntitySpawnedIn == -1)
                TimeEntitySpawnedIn = frameCount;

            if (frameCount - TimeEntitySpawnedIn >= LifeTime) {
                IsActive = false;
                return true;
            }

            return false;
        }

        public override Color GetColor(Color backgroundColor) => Color.Lerp(backgroundColor, Color.White, BrightnessLerper) * Opacity * FinalOpacityMultiplier * Helper_GetOpacityWithAccountingForOceanWaterLine();

        public void StartFadingOut(int currentFrameCount) {
            int num = (int)(LifeTime * OpacityNormalizedTimeToFadeOut);
            int num2 = currentFrameCount - num;
            if (num2 < TimeEntitySpawnedIn)
                TimeEntitySpawnedIn = num2;
        }

        public override Vector2 GetDrawPosition() => Position;
    }

    private class BackwoodsBirdsPackSkyEntity : FadingSkyEntity {
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

    private delegate SkyEntity EntityFactoryMethod(Player player, int seed);

    private bool _isActive;
    private readonly SlotVector<SkyEntity> _entities = new SlotVector<SkyEntity>(500);
    private int _frameCounter;

    public override void Activate(Vector2 position, params object[] args) {
        _isActive = true;
    }

    public override void Deactivate(params object[] args) {
        _isActive = false;
    }

    private bool AnActiveSkyConflictsWithAmbience() {
        if (!SkyManager.Instance["MonolithMoonLord"].IsActive())
            return SkyManager.Instance["MoonLord"].IsActive();

        return true;
    }

    public override void Update(GameTime gameTime) {
        if (Main.gamePaused)
            return;

        _frameCounter++;
        if (Main.netMode != 2 && AnActiveSkyConflictsWithAmbience() && SkyManager.Instance["CustomAmbience"].IsActive())
            SkyManager.Instance.Deactivate("CustomAmbience");

        foreach (SlotVector<SkyEntity>.ItemPair item in _entities) {
            SkyEntity value = item.Value;
            value.Update(_frameCounter);
            if (!value.IsActive) {
                _entities.Remove(item.Id);
                if (Main.netMode != 2 && _entities.Count == 0 && SkyManager.Instance["CustomAmbience"].IsActive())
                    SkyManager.Instance.Deactivate("CustomAmbience");
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {
        if (Main.gameMenu && Main.netMode == 0 && SkyManager.Instance["CustomAmbience"].IsActive()) {
            _entities.Clear();
            SkyManager.Instance.Deactivate("CustomAmbience");
        }

        foreach (SlotVector<SkyEntity>.ItemPair item in _entities) {
            item.Value.Draw(spriteBatch, 3f, minDepth, maxDepth);
        }
    }

    public override bool IsActive() => _isActive;

    public override void Reset() {
    }

    public void Spawn(Player player, CustomSkyEntityType type, int seed) {
        FastRandom random = new FastRandom(seed);
        switch (type) {
            case CustomSkyEntityType.BackwoodsBirdsV:
                _entities.Add(new BackwoodsBirdsPackSkyEntity(player, random));
                break;
        }

        if (Main.netMode != 2 && !AnActiveSkyConflictsWithAmbience() && !SkyManager.Instance["CustomAmbience"].IsActive())
            SkyManager.Instance.Activate("CustomAmbience", default);
    }
}
