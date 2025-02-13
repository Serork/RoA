using RoA.Content.Items.Miscellaneous;
using RoA.Content.Tiles.Furniture;

using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class BackwoodsStoneChest : ModItem {
    public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BackwoodsKey>();
    }

    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<BackwoodsDungeonChest>(), 1);
        Item.SetShopValues(ItemRarityColor.White0, Item.buyPrice(0, 0, 25));
        Item.maxStack = Item.CommonMaxStack;
        Item.width = 32;
        Item.height = 32;
    }
}