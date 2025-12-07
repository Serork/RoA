using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Tiles.Decorations;

sealed class DecorativeBanners : ModBannerTile {
    public enum StyleID : byte {
        SmolderingTapestry,
        Riot,
        Equality
    }

    public override void SetStaticDefaults() {
        base.SetStaticDefaults();

        TileID.Sets.GeneralPlacementTiles[Type] = false;
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
    }

    public override void NearbyEffects(int i, int j, bool closer) {
        
    }
}
