using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodToilet : ModItem {
	public override void SetDefaults() {
		int width = 32; int height = 18;
		Item.Size = new Vector2(width, height);

        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.ElderwoodToilet>(), 0);
        Item.maxStack = Item.CommonMaxStack;
        Item.value = 150;
    }

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient<Elderwood>(10)
	//		.Register();
	//}
}
