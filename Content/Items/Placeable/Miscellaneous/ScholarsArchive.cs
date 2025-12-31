using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class ScholarsArchive : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Miscellaneous.ScholarsArchive>());
        Item.maxStack = Item.CommonMaxStack;
    }
}
