using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class QuillStand : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Miscellaneous.QuillStand>());
        Item.maxStack = Item.CommonMaxStack;
    }
}
