using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class Elderwood : ModItem {
	public override void SetStaticDefaults() {
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
		ItemID.Sets.SortingPriorityMaterials[Type] = ItemID.Wood;

		ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.Wood;
    }

	public override void SetDefaults() {
        Item.useStyle = 1;
        Item.useTurn = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.autoReuse = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.width = 24;
        Item.height = 22;

        Item.createTile = ModContent.TileType<Tiles.Crafting.Elderwood>();
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<Furniture.ElderwoodPlatform>(2)
			.Register();

		CreateRecipe()
			.AddIngredient<Walls.ElderwoodWall>(4)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}