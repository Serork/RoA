using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodChandelier : ModItem {
	public override void SetDefaults() {
		int width = 30; int height = 28;
		Item.Size = new Vector2(width, height);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
		Item.value = 3000;
        Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodChandelier>();
    }

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<Crafting.Elderwood>(4)
			.AddIngredient(ItemID.Torch, 4)
			.AddIngredient(ItemID.Chain)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
