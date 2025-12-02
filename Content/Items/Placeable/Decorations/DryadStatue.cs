using RoA.Core.Defaults;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Decorations;

sealed class DryadStatue : ModItem {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Dryad Statue");
        // Tooltip.SetDefault("'Lost and forsaken'");
        Item.ResearchUnlockCount = 1;
    }

    public override void SetDefaults() {
        Item.SetSizeValues(28, 42);

        Item.SetDefaultsToStackable(Item.CommonMaxStack);

        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Ambient.DryadStatue>());

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 0, 60);
    }
}