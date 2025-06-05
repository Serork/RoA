using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class BackwoodsCampfire : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Crafting.BackwoodsCampfire>(), 0);
    }
}
