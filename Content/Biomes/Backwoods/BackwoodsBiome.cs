using RoA.Common;
using RoA.Core;

using Terraria;
using Terraria.ModLoader;

namespace RiseofAges.Content.Biomes.Backwoods;

[Autoload(Side = ModSide.Client)]
sealed class BackwoodsBiome : ModBiome {
    public static float TransitionSpeed => 0.05f;

    public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

    public override void SpecialVisuals(Player player, bool isActive) => player.ManageSpecialBiomeVisuals(RoA.RoA.BackwoodsSky, player.InModBiome(ModContent.GetInstance<BackwoodsBiome>()), player.Center);

    public override bool IsBiomeActive(Player player) {
        bool isInBiome = ModContent.GetInstance<TileCount>().BackwoodsTiles >= 500;
        return isInBiome;
    }

    public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.Find<ModSurfaceBackgroundStyle>(RoA.RoA.ModName +  "/BackwoodsBackgroundSurface");

    public override int Music => Main.dayTime ? MusicLoader.GetMusicSlot(ResourceManager.Music + "ThicketDay") : MusicLoader.GetMusicSlot(ResourceManager.Music + "ThicketNight");
}