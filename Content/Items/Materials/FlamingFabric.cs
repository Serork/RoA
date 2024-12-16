using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class FlamingFabric : ModItem {
	public override void SetStaticDefaults() {
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
		ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
	}

	public override void SetDefaults() {
		int width = 26; int height = 24;
		Item.Size = new Vector2(width, height);

		Item.value = Item.sellPrice(silver: 35);
		Item.rare = ItemRarityID.Green;
		Item.maxStack = Item.CommonMaxStack;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ItemID.Cobweb, 5)
	//		.AddIngredient(ItemID.Fireblossom)
	//		.AddIngredient(ItemID.Hellstone)
	//		.AddTile(TileID.Loom)
	//		.Register();
	//}
}