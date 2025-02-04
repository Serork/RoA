using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Special.Lothor;

sealed class LothorRelic : ModItem {
    public override void SetDefaults() {
        // Vanilla has many useful methods like these, use them! This substitutes setting Item.createTile and Item.placeStyle as well as setting a few values that are common across all placeable items
        // The place style (here by default 0) is important if you decide to have more than one relic share the same tile type (more on that in the tiles' code)
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Decorations.LothorRelic>(), 0);

        Item.width = 38;
        Item.height = 48;
        Item.rare = ItemRarityID.Master;
        Item.master = true; // This makes sure that "Master" displays in the tooltip, as the rarity only changes the item name color
        Item.value = Item.buyPrice(0, 5);
    }
}
