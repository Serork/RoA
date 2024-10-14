using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class SnowWreath2 : BaseWreathItem {
	protected override void SafeSetDefaults() {
		int width = 20; int height = width;
		Item.Size = new Vector2(width, height);

		Item.maxStack = 1;
		Item.value = Item.sellPrice(gold: 1, silver: 50);
		Item.rare = ItemRarityID.Green;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ModContent.ItemType<SnowWreath>())
	//		.AddIngredient(ModContent.ItemType<Cloudberry>())
	//		.AddIngredient(ModContent.ItemType<NaturesHeart>())
	//		.AddTile(ModContent.TileType<OvergrownAltar>())
	//		.Register();
	//}
}
