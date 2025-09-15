using RoA.Content.Items.Miscellaneous;
using RoA.Content.Tiles.Danger;
using RoA.Core.Defaults;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class Test : ModItem {
    public override string Texture => ItemLoader.GetItem(ModContent.ItemType<AltarPlacer>()).Texture;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<GrimrockStalactite>());
    }
}

sealed class Test2 : ModItem {
    public override string Texture => ItemLoader.GetItem(ModContent.ItemType<AltarPlacer>()).Texture;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<IceStalactite>());
    }
}

sealed class Test3 : ModItem {
    public override string Texture => ItemLoader.GetItem(ModContent.ItemType<AltarPlacer>()).Texture;

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<StoneStalactite>());
    }
}
