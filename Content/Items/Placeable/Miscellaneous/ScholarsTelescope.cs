using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class ScholarsTelescope : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Miscellaneous.ScholarsTelescope>());
        Item.maxStack = Item.CommonMaxStack;
    }
}

