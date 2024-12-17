﻿using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodSink : ModItem {
	public override void SetDefaults() {
		int width = 32; int height = 30;
		Item.Size = new Vector2(width, height);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
		Item.value = 300;
        Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodSink>();
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient<Elderwood>(6)
	//		.AddIngredient(ItemID.WaterBucket)
	//		.AddTile(TileID.WorkBenches)
	//		.Register();
	//}
}
