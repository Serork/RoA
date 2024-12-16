﻿using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodSofa : ModItem {
	public override void SetDefaults() {
		int width = 32; int height = 20;
		Item.Size = new Vector2(width, height);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
		Item.value = Item.sellPrice(copper: 60);
        //Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodSofa>();
    }

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient<Elderwood>(5)
	//		.AddIngredient(ItemID.Silk, 2)
	//		.AddTile(TileID.Sawmill)
	//		.Register();
	//}
}
