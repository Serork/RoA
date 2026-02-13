using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Core;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Menus;

sealed class BackwoodsMenu : ModMenu {
    public override string DisplayName => Language.GetTextValue("Mods.RoA.Biomes.BackwoodsBiome.DisplayName");

    public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>(ResourceManager.Textures + "RiseofAgesLogoAnimation");

    public override int Music => MusicLoader.GetMusicSlot(RoA.MusicMod, ResourceManager.Music + "ThicketNight");

    public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<BackwoodsMenuBG>();

    internal static float GlobalOpacity { get; set; }

    public int _frame;
    public int _frameCounter;

    public override void OnDeselected()
        => GlobalOpacity = 0f;

    public override void OnSelected()
        => GlobalOpacity = 1f;

    public override void Update(bool isOnTitleScreen) {
        if (Main.netMode == NetmodeID.SinglePlayer) {
            Main.time = 7500;
            Main.dayTime = true;
        }
    }

    public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor) {
    }

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
        Texture2D texture = (Texture2D)Logo;
        logoDrawCenter += new Vector2(0f, 15f);
        logoScale *= 1.05f;
        drawColor = Color.White;
        int frameHeight = texture.Height / 48;
        Rectangle frameRect = new Rectangle(0, _frame * frameHeight, texture.Width, frameHeight);
        Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);
        spriteBatch.Draw(texture, logoDrawCenter, frameRect, drawColor, logoRotation, drawOrigin, logoScale, SpriteEffects.None, default);

        if (_frameCounter++ % 5 == 0) _frame++;
        if (_frame > 47) _frame = 0;

        return false;
    }
}