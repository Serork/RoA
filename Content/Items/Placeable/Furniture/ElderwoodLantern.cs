using Microsoft.Xna.Framework;

using RoA.Content.Items.Placeable.Crafting;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Furniture;

sealed class ElderwoodLantern : ModItem {
	public override void SetDefaults() {
		int width = 16; int height = 32;
		Item.Size = new Vector2(width, height);

        Item.maxStack = Terraria.Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = 1;
        Item.consumable = true;
        Item.value = 150;
        Item.createTile = ModContent.TileType<Tiles.Furniture.ElderwoodLantern>();
    }

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient<Crafting.Elderwood>(6)
			.AddIngredient(ItemID.Torch)
			.AddTile(TileID.WorkBenches)
			.Register();
	}
}
