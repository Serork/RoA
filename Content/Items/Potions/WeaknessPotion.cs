using Microsoft.Xna.Framework;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class WeaknessPotion : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Weakness Potion");
		//Tooltip.SetDefault("Throw this to make someone weak");
	}

	public override void SetDefaults() {
        int width = 18; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Green;

        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.UseSound = SoundID.Item1;

        Item.consumable = true;
        Item.noUseGraphic = true;
        Item.noMelee = true;

        Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.WeaknessPotion>();
		Item.shootSpeed = 10f;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ItemID.BottledWater)
	//		.AddIngredient(ItemID.Deathweed)
	//		.AddIngredient(ItemID.Shiverthorn)
	//		.AddIngredient<Bonerose>()
	//		.AddTile(TileID.Bottles)
	//		.Register();
	//}
}