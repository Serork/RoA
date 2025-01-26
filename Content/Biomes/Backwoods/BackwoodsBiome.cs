using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RoA.Common;
using RoA.Common.Tiles;
using RoA.Common.WorldEvents;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Tiles.Crafting;
using RoA.Core;

using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace RoA.Content.Biomes.Backwoods;

sealed class LightColorFix : ILoadable {
    void ILoadable.Load(Mod mod) {
        On_Main.DrawMapFullscreenBackground += On_Main_DrawMapFullscreenBackground;
    }

    private void On_Main_DrawMapFullscreenBackground(On_Main.orig_DrawMapFullscreenBackground orig, Vector2 screenPosition, int screenWidth, int screenHeight) {
        if (Main.LocalPlayer.InModBiome<BackwoodsBiome>()) {
            Texture2D mapBGAsset = ModContent.Request<Texture2D>(ResourceManager.BackwoodsTextures + "DruidBiomeMapBG").Value;
            Color color = Main.ColorOfTheSkies * 3f;
            if ((double)screenPosition.Y > Main.worldSurface * 16.0) {
                color = Color.White;
            }
            //else {
            //    color = Main.ColorOfTheSkies;
            //    MapBGAsset = ((!Main.player[Main.myPlayer].ZoneDesert) ? (Texture2D)ModContent.Request<Texture2D>("TheConfectionRebirth/Biomes/ConfectionBiomeMapBackground") : ((Main.player[Main.myPlayer].ZoneSnow) ? (Texture2D)ModContent.Request<Texture2D>("TheConfectionRebirth/Biomes/ConfectionIceBiomeMapBackground") : (Texture2D)ModContent.Request<Texture2D>("TheConfectionRebirth/Biomes/ConfectionDesertBiomeMapBackground")));
            //}
            Main.spriteBatch.Draw(mapBGAsset, new Rectangle(0, 0, screenWidth, screenHeight), color);
            mapBGAsset = ModContent.Request<Texture2D>(ResourceManager.BackwoodsTextures + "DruidBiomeMapBG_Glow").Value;
            Main.spriteBatch.Draw(mapBGAsset, new Rectangle(0, 0, screenWidth, screenHeight), Color.White * 0.5f);
        }
        else {
            orig.Invoke(screenPosition, screenWidth, screenHeight);
        }
    }

    void ILoadable.Unload() { }
}

sealed partial class BackwoodsBiome : ModBiome {
    public static float TransitionSpeed => 0.05f;

    public static bool IsActiveForFogEffect => ModContent.GetInstance<TileCount>().BackwoodsTiles > 650;

    public static bool BiomeShouldBeActive => ModContent.GetInstance<TileCount>().BackwoodsTiles >= 1000;

    public static BackwoodsBiome Instance => ModContent.GetInstance<BackwoodsBiome>();

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override int BiomeTorchItemType => ModContent.ItemType<Items.Placeable.Crafting.Elderwood>();

    public override void SpecialVisuals(Player player, bool isActive) => player.ManageSpecialBiomeVisuals(ShaderLoader.BackwoodsSky, player.InModBiome<BackwoodsBiome>(), player.Center);

    public override bool IsBiomeActive(Player player) {
        bool isInBiome = BiomeShouldBeActive;
        return isInBiome;
    }

    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>(RoA.ModName +  "/BackwoodsBackgroundSurface");

    public override int Music => BackwoodsFogHandler.IsFogActive ? MusicLoader.GetMusicSlot(ResourceManager.Music + "Fog") : /*Main.IsItDay() ? MusicLoader.GetMusicSlot(ResourceManager.Music + "ThicketDay") :*/ MusicLoader.GetMusicSlot(ResourceManager.Music + "ThicketNight");

    public override string MapBackground => ResourceManager.BackwoodsTextures + "DruidBiomeMapBG";

    public override string BackgroundPath => MapBackground;

    //public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>(RoA.ModName + "/BackwoodsBackgroundUnderground");

    public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

    public override string BestiaryIcon => ResourceManager.Textures + "BackwoodsBestiaryIcon";

    public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>(RoA.ModName + "/DruidBiomeWaterStyle");
}