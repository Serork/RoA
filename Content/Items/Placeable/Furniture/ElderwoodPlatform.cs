using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodPlatform : ModItem {
	public override void SetDefaults() {
		int width = 24; int height = 16;
		Item.Size = new Vector2(width, height);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
		Item.value = 0;
        //Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodPlatform>();
	}

	//	public override void AddRecipes() {
	//		CreateRecipe(2)
	//			.AddIngredient<Elderwood>(1)
	//			.Register();
	//	}
}
