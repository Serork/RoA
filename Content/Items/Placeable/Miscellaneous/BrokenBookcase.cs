using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Miscellaneous;

sealed class BrokenBookcase : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Miscellaneous.BrokenBookcase>());
        Item.SetShopValues(ItemRarityColor.White0, 300);
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 24;
        Item.height = 32;
    }
}
