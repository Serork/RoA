using RoA.Content.Tiles.Miscellaneous;

using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class BackwoodsPylon : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<BackwoodsPylonTile>());
        Item.SetShopValues(ItemRarityColor.Blue1, Terraria.Item.sellPrice(gold: 2));
    }
}
