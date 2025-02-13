using RoA.Content.Items.Placeable.Furniture;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class BackwoodsKey : ModItem {
	public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BackwoodsStoneChest>();
    }

	public override void SetDefaults() {
        Item.width = 18;
        Item.height = 28;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = 8;
    }
}