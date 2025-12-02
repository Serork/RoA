using RoA.Core;

using Terraria.ModLoader;

namespace RoA.Content.Biomes.Backwoods;

sealed class BackwoodsBackgroundUnderground : ModUndergroundBackgroundStyle {
    public override void FillTextureArray(int[] textureSlots) {
        for (int i = 0; i < textureSlots.Length; i++) {
            textureSlots[0] = BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "DruidBiomeBackgroundCave0");
            textureSlots[1] = BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "DruidBiomeBackgroundCave1");
            textureSlots[2] = BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "DruidBiomeBackgroundCave2");
            textureSlots[3] = BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "DruidBiomeBackgroundCave3");
        }
    }
}