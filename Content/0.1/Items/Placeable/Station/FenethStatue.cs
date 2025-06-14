using RoA.Core.Defaults;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Station;

sealed class FenethStatue : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Dryad Statue");
        // Tooltip.SetDefault("'Lost and forsaken'");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.SetSizeValues(36, 40);

        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Station.FenethStatue>());

        Item.SetDefaultsToStackable(1);

        Item.rare = ItemRarityID.LightRed;
    }
}