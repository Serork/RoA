﻿using RoA.Core;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodChest : ModItem {
	public override void SetDefaults() {
		Item.SetSize(28, 14);

		Item.SetDefaultToUsable(ItemUseStyleID.Swing, 10, 15, useTurn: true, autoReuse: true);

        Item.SetDefaultToStackable(99);

        Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodChest>();
	}

	public override void AddRecipes() {
		//CreateRecipe()
		//	.AddIngredient<Materials.Elderwood>(8)
		//	.AddRecipeGroup(RecipeGroupID.IronBar, 2)
		//	.AddTile(TileID.WorkBenches)
		//	.Register();
	}
}