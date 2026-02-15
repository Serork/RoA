using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Common.World;

sealed class FilamentSky : CustomSky {
    private struct Meteor {
        public Vector2 Position;
        public float Depth;
        public int FrameCounter;
        public float Scale;
        public float StartX;
    }

    private UnifiedRandom _random = new UnifiedRandom();
    private Asset<Texture2D> _planetTexture;
    private Asset<Texture2D> _bgTexture;
    private Asset<Texture2D> _meteorTexture;
    private bool _isActive;
    private float _fadeOpacity;

    public override void OnLoad() {
        _planetTexture = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "FilamentPlanet");
        _bgTexture = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "FilamentBackground");
    }

    public override void Update(GameTime gameTime) {
        if (_isActive)
            _fadeOpacity = Math.Min(1f, 0.01f + _fadeOpacity);
        else
            _fadeOpacity = Math.Max(0f, _fadeOpacity - 0.01f);
    }

    public override Color OnTileColor(Color inColor) => new Color(Vector4.Lerp(inColor.ToVector4(), Vector4.One, _fadeOpacity * 0.5f));

    public static Color FilterColor => new(160, 148, 57);
    public static float FilterOpacity => 0.65f;

    public static Color BeforePlanetGradientColor => new Color(251, 232, 193);
    public static Color PlanetColor => Color.White;
    public static Color AfterPlanetGradientColor => new Color(233, 206, 83) * 0.5f;

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {
        if (maxDepth >= float.MaxValue && minDepth < float.MaxValue) {
            // black background
            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * _fadeOpacity);

            // gradient before planet
            spriteBatch.Draw(_bgTexture.Value, new Rectangle(0, Math.Max(0, (int)((Main.worldSurface * 16.0 - (double)Main.screenPosition.Y - 2400.0) * 0.10000000149011612)), Main.screenWidth, Main.screenHeight), 
                BeforePlanetGradientColor * Math.Min(1f, (Main.screenPosition.Y - 800f) / 1000f * _fadeOpacity));

            // planet
            Vector2 vector = new Vector2(Main.screenWidth >> 1, Main.screenHeight >> 1);
            Vector2 vector2 = 0.01f * (new Vector2((float)Main.maxTilesX * 8f, (float)Main.worldSurface / 2f) - Main.screenPosition);
            Vector2 positionOffset = new Vector2(200f, -200f);
            spriteBatch.Draw(_planetTexture.Value, vector + positionOffset + vector2, null,
                PlanetColor.MultiplyAlpha(1f) * 0.9f * _fadeOpacity, 0f, new Vector2(_planetTexture.Width() >> 1, _planetTexture.Height() >> 1), 1f, SpriteEffects.None, 1f);
            
            // gradient after planet
            spriteBatch.Draw(_bgTexture.Value, new Rectangle(0, Math.Max(0, (int)((Main.worldSurface * 16.0 - (double)Main.screenPosition.Y - 2400.0) * 0.10000000149011612)), Main.screenWidth, Main.screenHeight),
       AfterPlanetGradientColor.MultiplyAlpha(1f) * Math.Min(1f, (Main.screenPosition.Y - 800f) / 1000f * _fadeOpacity));
        }
    }

    public override float GetCloudAlpha() => (1f - _fadeOpacity) * 0.3f + 0.7f;

    public override void Activate(Vector2 position, params object[] args) {
        _fadeOpacity = 0.002f;
        _isActive = true;
    }

    private int SortMethod(Meteor meteor1, Meteor meteor2) => meteor2.Depth.CompareTo(meteor1.Depth);

    public override void Deactivate(params object[] args) {
        _isActive = false;
    }

    public override void Reset() {
        _isActive = false;
    }

    public override bool IsActive() {
        if (!_isActive)
            return _fadeOpacity > 0.001f;

        return true;
    }
}

