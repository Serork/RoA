using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common.Tiles;
using RoA.Content.Menus;
using RoA.Core.Utility;
using RoA.Core.Utility.Vanilla;

using System;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace RoA.Content.Backgrounds;

sealed class BackwoodsSky : CustomSky {
    private float _opacity;

    public override void Update(GameTime gameTime) {
        //if (_skyActive && _opacity < 1f) {
        //    _opacity += 0.02f;
        //}
        //else if (!_skyActive && _opacity > 0f) {
        //    _opacity -= 0.02f;
        //}

        Player player = Main.LocalPlayer;
        float globalOpacity = MathUtils.Clamp01(ModContent.GetInstance<TileCount>().BackwoodsTiles / 1000f);
        if (player.ZoneTowerNebula || player.ZoneTowerSolar || player.ZoneTowerVortex || player.ZoneTowerStardust || player.GetCommon().ZoneFilament) {
            globalOpacity = 0f;
        }
        _opacity = Helper.Approach(_opacity, globalOpacity, 0.02f);
    }

    public override Color OnTileColor(Color inColor) {
        float amt = _opacity * .3f;
        return inColor.MultiplyRGB(new Color(1f - amt, 1f - amt, 1f - amt));
    }

    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth) {
        if (maxDepth >= 3E+38f && minDepth < 3E+38f && !Main.gameMenu) {
            Texture2D skyTexture = BackwoodsMenuBG.SkyTexture.Value;
            spriteBatch.Draw(skyTexture,
                new Rectangle(-(int)(Main.screenWidth * 0.1f), -(int)(Main.screenHeight * 0.1f), (int)(Main.screenWidth * 1.2f), (int)(Main.screenHeight * 1.2f)),
                Main.ColorOfTheSkies * 0.95f * Math.Min(1f, (Main.screenPosition.Y - 800f) / 1000f * _opacity));
            spriteBatch.Draw(skyTexture,
                new Rectangle(-(int)(Main.screenWidth * 0.1f), -(int)(Main.screenHeight * 0.1f), (int)(Main.screenWidth * 1.2f), (int)(Main.screenHeight * 1.2f)),
                Color.Black.MultiplyRGB(Main.ColorOfTheSkies) * 0.15f * Math.Min(1f, (Main.screenPosition.Y - 800f) / 1000f * _opacity));
        }
    }

    public override float GetCloudAlpha() => 1f - _opacity;

    public override void Activate(Vector2 position, params object[] args) {
    }

    public override void Deactivate(params object[] args) { }

    public override void Reset() { }

    public override bool IsActive() => _opacity > 0f;
}
