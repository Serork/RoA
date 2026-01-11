using RoA.Common.Tiles;
using RoA.Core;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Biomes.Tarpit;

sealed class TarBiome : ModBiome {
    public override string MapBackground => ResourceManager.TarBiomeTextures + "TarMapBG";

    public static bool BiomeShouldBeActive => ModContent.GetInstance<TileCount>().TarpitTiles >= 100;

    public override int Music => -1;

    public override bool IsBiomeActive(Player player) {
        bool isInBiome = BiomeShouldBeActive;
        return isInBiome;
    }
}
