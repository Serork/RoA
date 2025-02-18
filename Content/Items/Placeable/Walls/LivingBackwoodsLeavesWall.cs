using RoA.Content.Items.Placeable.Crafting;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Walls;

sealed class LivingBackwoodsLeavesWall : ModItem {
	public override void SetStaticDefaults () {
		Item.ResearchUnlockCount = 400;
	}
	
	public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 7;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.width = 24;
        Item.height = 24;
        Item.createWall = ModContent.WallType<Tiles.Walls.LivingBackwoodsLeavesWall>();
	}

	public override void AddRecipes() {
		CreateRecipe(4)
			.AddIngredient<Elderwood>()
			.AddTile(TileID.LivingLoom)
			.Register();
	}
}
