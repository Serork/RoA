using RoA.Core;

using Terraria.ModLoader;

namespace RoA.Content.Biomes.Backwoods;

[Autoload(Side = ModSide.Client)]
sealed class BackwoodsBackgroundUnderground : ModUndergroundBackgroundStyle {
    public override void FillTextureArray(int[] textureSlots) {
        for (int i = 0; i < textureSlots.Length; i++) { 
            textureSlots[i] = BackgroundTextureLoader.GetBackgroundSlot(ResourceManager.BackgroundTextures + "DruidBiomeBackgroundCave");
        }
    }
}