using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class ForestWreath : BaseWreathItem {
	protected override void SafeSetDefaults() {
		int width = 20; int height = width;
		Item.Size = new Vector2(width, height);

		Item.maxStack = 1;
		Item.value = Item.sellPrice(gold: 1);
		Item.rare = ItemRarityID.Blue;
    }

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ModContent.ItemType<TwigWreath>())
	//		.AddIngredient(ItemID.Daybloom, 5)
	//		.Register();
	//}
}