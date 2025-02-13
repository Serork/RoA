using RoA.Content.Items.Placeable.Furniture;

using System.Collections.Generic;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Content.Items.Miscellaneous;

sealed class BackwoodsKey : ModItem {
	public override void SetStaticDefaults() {
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<BackwoodsStoneChest>();
        ItemID.Sets.UsesCursedByPlanteraTooltip[Type] = true;
    }

	public override void SetDefaults() {
        Item.width = 18;
        Item.height = 28;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = 8;
    }
}