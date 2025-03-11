using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ReLogic.Content;

using RoA.Common;
using RoA.Core;

using System;

using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Menus;

sealed class BackwoodsMenu : ModMenu {
    public override string DisplayName => Language.GetTextValue("Mods.RoA.Biomes.BackwoodsBiome.DisplayName");

    public override Asset<Texture2D> Logo => ModContent.Request<Texture2D>(ResourceManager.Textures + "RiseofAgesLogo");

    public override int Music => MusicLoader.GetMusicSlot(ResourceManager.Music + "ThicketNight");

    public override ModSurfaceBackgroundStyle MenuBackgroundStyle => ModContent.GetInstance<BackwoodsMenuBG>();

    internal static float GlobalOpacity { get; set; }

    public override void OnDeselected()
        => GlobalOpacity = 0f;

    public override void OnSelected()
        => GlobalOpacity = 1f;

    public override void Update(bool isOnTitleScreen) {
        if (Main.netMode == NetmodeID.SinglePlayer) {
            Main.time = 5000;
            Main.dayTime = true;
        }
    }

    public override void PostDrawLogo(SpriteBatch spriteBatch, Vector2 logoDrawCenter, float logoRotation, float logoScale, Color drawColor) {
    }

    public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor) {
        logoDrawCenter += new Vector2(0f, 15f);
        logoScale *= 1f;
        drawColor = Color.White * 0.85f;
        return true;
    }
}