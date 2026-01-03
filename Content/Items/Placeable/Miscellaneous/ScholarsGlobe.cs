using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class ScholarsGlobe : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Miscellaneous.ScholarsGlobe>());
        Item.maxStack = Item.CommonMaxStack;
    }
}
