using RoA.Core;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodChest2 : ModItem {
	public override void SetDefaults() {
		Item.SetSize(32, 32);

		Item.SetDefaultToUsable(ItemUseStyleID.Swing, 10, 15, useTurn: true, autoReuse: true);

        Item.SetDefaultToStackable(Terraria.Item.CommonMaxStack);

        Item.value = 500;

        Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodChest2>();
	}

	public override void AddRecipes() {
		//CreateRecipe()
		//	.AddIngredient<Materials.Elderwood>(8)
		//	.AddRecipeGroup(RecipeGroupID.IronBar, 2)
		//	.AddTile(TileID.WorkBenches)
		//	.Register();
	}
}