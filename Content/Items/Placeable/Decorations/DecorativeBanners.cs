using RoA.Core.Defaults;

using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class Equality : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(12, 28);

        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.DecorativeBanners>(), (int)Tiles.Decorations.DecorativeBanners.StyleID.Equality);
    }
}

sealed class Riot : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(12, 28);

        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.DecorativeBanners>(), (int)Tiles.Decorations.DecorativeBanners.StyleID.Riot);
    }
}

sealed class SmolderingTapestry : ModItem {
    public override void SetDefaults() {
        Item.SetSizeValues(12, 28);

        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.DecorativeBanners>(), (int)Tiles.Decorations.DecorativeBanners.StyleID.SmolderingTapestry);
    }
}
