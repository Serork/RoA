using RoA.Common;
using RoA.Common.Tiles;
using RoA.Common.WorldEvents;
using RoA.Core;

using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace RoA.Content.Biomes.Backwoods;

sealed class BackwoodsBiome : ModBiome {
    public static float TransitionSpeed => 0.05f;

    public static bool IsActiveForFogEffect => ModContent.GetInstance<TileCount>().BackwoodsTiles > 650;

    public static BackwoodsBiome Instance => ModContent.GetInstance<BackwoodsBiome>();

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override void SpecialVisuals(Player player, bool isActive) => player.ManageSpecialBiomeVisuals(ShaderLoader.BackwoodsSky, player.InModBiome<BackwoodsBiome>(), player.Center);

    public override bool IsBiomeActive(Player player) {
        bool isInBiome = ModContent.GetInstance<TileCount>().BackwoodsTiles >= 1000;
        return isInBiome;
    }

    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>(RoA.ModName +  "/BackwoodsBackgroundSurface");

    public override int Music => BackwoodsFogHandler.IsFogActive ? MusicLoader.GetMusicSlot(ResourceManager.Music + "Fog") : Main.dayTime ? MusicLoader.GetMusicSlot(ResourceManager.Music + "ThicketDay") : MusicLoader.GetMusicSlot(ResourceManager.Music + "ThicketNight");

    public override string MapBackground => ResourceManager.BackwoodsTextures + "DruidBiomeMapBG";

    public override string BackgroundPath => MapBackground;

    //public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>(RoA.ModName + "/BackwoodsBackgroundUnderground");

    public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

    public override string BestiaryIcon => ResourceManager.Textures + "BackwoodsBestiaryIcon";

    public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>(RoA.ModName + "/DruidBiomeWaterStyle");
}