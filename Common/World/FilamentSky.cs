using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;
using RoA.Core.Utility;
using RoA.Core.Utility.Extensions;

using System;

using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace RoA.Common.World;

sealed class FilamentSky : CustomSky {
    private struct Beam {
        public Vector2 Position;
        public float Depth;
        public float SinOffset;
        public float AlphaFrequency;
        public float AlphaAmplitude;
        public float ClipX;
        public float Opacity;
        public float ScaleX;
    }

    private UnifiedRandom _random = new UnifiedRandom();
    private Asset<Texture2D> _planetTexture;
    private Asset<Texture2D> _bgTexture;
    private bool _isActive;
    private float _fadeOpacity;

    private Asset<Texture2D> _beamTexture;
    private Beam[] _beams;

    public override void OnLoad() {
        _planetTexture = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "FilamentPlanet");
        _bgTexture = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "FilamentBackground");
        _beamTexture = ModContent.Request<Texture2D>(ResourceManager.BackgroundTextures + "FilamentBeam");
    }

    public override void Update(GameTime gameTime) {
        if (_isActive)
            _fadeOpacity = Math.Min(1f, 0.01f + _fadeOpacity);
        else
            _fadeOpacity = Math.Max(0f, _fadeOpacity - 0.01f);

        for (int i = 0; i < _beams.Length; i++) {
            _beams[i].ClipX = Helper.Approach(_beams[i].ClipX, 1f, 0.01f);
            if (_beams[i].ClipX >= 1f) {
                _beams[i].ClipX = 0f;
            }
            _beams[i].Opacity = MathF.Min(_fadeOpacity, _beams[i].Opacity);
        }
    }

    public override Color OnTileColor(Color inColor) => new Color(Vector4.Lerp(inColor.ToVector4(), Vector4.One, _fadeOpacity * 0.5f));

    public static float DarkenFactor => 0.03f;

    public static Color FilterColor => new Color(155, 160, 57).ModifyRGB(1f - DarkenFactor);
    public static float FilterOpacity => 0.65f;

    public static Color BeforePlanetGradientColor => new Color(251, 242, 193).ModifyRGB(1f - DarkenFactor);
    public static Color PlanetColor => Color.White.ModifyRGB(1f - DarkenFactor);
    public static Color AfterPlanetGradientColor => new Color(233, 231, 83).ModifyRGB(1f - DarkenFactor) * 0.5f;

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

        int num = -1;
        int num2 = 0;
        for (int i = 0; i < _beams.Length; i++) {
            float depth = _beams[i].Depth;
            if (num == -1 && depth < maxDepth)
                num = i;

            if (depth <= minDepth)
                break;

            num2 = i;
        }

        if (num == -1)
            return;

        float num3 = Math.Min(1f, (Main.screenPosition.Y - 1000f) / 1000f);
        Vector2 vector3 = Main.screenPosition + new Vector2(Main.screenWidth >> 1, Main.screenHeight >> 1);
        Rectangle rectangle = new Rectangle(-1000, -1000, 4000, 4000);
        for (int j = num; j < num2; j++) {
            Vector2 vector4 = new Vector2(1f / _beams[j].Depth, 1.1f / _beams[j].Depth);
            Vector2 position = (_beams[j].Position - vector3) * vector4 + vector3 - Main.screenPosition;
            if (rectangle.Contains((int)position.X, (int)position.Y)) {
                _beams[j].Opacity = Helper.Approach(_beams[j].Opacity, 1f * _fadeOpacity, 0.01f);

            }
            else {
                _beams[j].Opacity = Helper.Approach(_beams[j].Opacity, 0f, 0.01f);
            }
            float value = (float)Math.Sin(_beams[j].AlphaFrequency * TimeSystem.TimeForVisualEffects * 0.25f + _beams[j].SinOffset) * _beams[j].AlphaAmplitude + _beams[j].AlphaAmplitude;
            float num4 = (float)Math.Sin(_beams[j].AlphaFrequency * TimeSystem.TimeForVisualEffects * 5f + _beams[j].SinOffset) * 0.1f - 0.1f;
            value = MathHelper.Clamp(value, 0.5f, 1f);
            Texture2D value2 = _beamTexture.Value;
            Color color = Color.White;
            color = Color.Lerp(color, Color.Yellow, 0.5f);
            color = Color.Lerp(color, Color.LightYellow, 0.5f);
            color.A = 0;
            Vector2 scale = new Vector2(vector4.X * 0.5f + 0.5f) * (value * 0.1f + 0.9f);
            scale.Y *= _beams[j].ScaleX;
            float rotation = _beams[j].SinOffset;
            int attempts2 = 50;
            int attempts = attempts2;
            Vector2 position2 = position;
            int width = value2.Width / 2;
            Rectangle bounds = new Rectangle((int)((_beams[j].ClipX * width) + _beams[j].Depth * width) % width, 0, width, value2.Height);
            Vector2 origin = bounds.LeftCenter();
            Color color2 = color * num3 * value * 0.8f * (1f - num4) * 0.45f * _beams[j].Opacity;
            while (attempts-- > 0) {
                spriteBatch.Draw(value2, position2, bounds, color2, rotation, origin, scale, SpriteEffects.None, 0f);
                position2 += Vector2.UnitX.RotatedBy(rotation) * width * new Vector2(scale.X);
            }
            attempts = attempts2;
            position2 = position;
            while (attempts-- > 0) {
                position2 -= Vector2.UnitX.RotatedBy(rotation) * width * new Vector2(scale.X);
                spriteBatch.Draw(value2, position2, bounds, color2, rotation, origin, scale, SpriteEffects.None, 0f);
            }
        }
    }

    public override float GetCloudAlpha() => (1f - _fadeOpacity) * 0.3f + 0.7f;

    public override void Activate(Vector2 position, params object[] args) {
        _fadeOpacity = 0.002f;
        _isActive = true;

        int num = 10;
        int num2 = 10;
        _beams = new Beam[num * num2];
        int num3 = 0;
        for (int i = 0; i < num; i++) {
            float num4 = (float)i / (float)num;
            for (int j = 0; j < num2; j++) {
                float num5 = (float)j / (float)num2;
                _beams[num3].Position.X = num4 * (float)Main.maxTilesX * 16f;
                _beams[num3].Position.Y = num5 * ((float)Main.worldSurface * 16f + 2000f) - 1000f;
                _beams[num3].Depth = MathF.Max(0f, _random.NextFloat(0.25f, 1f) * 8f);
                _beams[num3].SinOffset = _random.NextFloat() * 6.28f;
                _beams[num3].AlphaAmplitude = _random.NextFloat() * 5f;
                _beams[num3].AlphaFrequency = _random.NextFloat() + 1f;
                _beams[num3].ScaleX = _random.NextFloat(0.85f, 1f);
                num3++;
            }
        }

        Array.Sort(_beams, SortMethod);
    }

    private int SortMethod(Beam meteor1, Beam meteor2) => meteor2.Depth.CompareTo(meteor1.Depth);

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

