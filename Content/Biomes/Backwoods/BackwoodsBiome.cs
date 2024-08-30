using RoA.Common.Tiles;
using RoA.Core;

using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace RoA.Content.Biomes.Backwoods;

sealed class BackwoodsBiome : ModBiome {
    public static float TransitionSpeed => 0.05f;

    public static BackwoodsBiome Instance => ModContent.GetInstance<BackwoodsBiome>();

    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;

    public override void SpecialVisuals(Player player, bool isActive) => player.ManageSpecialBiomeVisuals(RoA.BackwoodsSky, player.InModBiome(ModContent.GetInstance<BackwoodsBiome>()), player.Center);

    public override bool IsBiomeActive(Player player) {
        bool isInBiome = ModContent.GetInstance<TileCount>().BackwoodsTiles >= 1000;
        return isInBiome;
    }

    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>(RoA.ModName +  "/BackwoodsBackgroundSurface");

    public override int Music => Main.dayTime ? MusicLoader.GetMusicSlot(ResourceManager.Music + "ThicketDay") : MusicLoader.GetMusicSlot(ResourceManager.Music + "ThicketNight");

    public override string MapBackground => ResourceManager.BackwoodsTextures + "DruidBiomeMapBG";

    public override string BackgroundPath => MapBackground;

    //public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>(RoA.ModName + "/BackwoodsBackgroundUnderground");

    public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Normal;

    public override string BestiaryIcon => ResourceManager.Textures + "BackwoodsBestiaryIcon";

    public override ModWaterStyle WaterStyle => ModContent.Find<ModWaterStyle>(RoA.ModName + "/DruidBiomeWaterStyle");
}