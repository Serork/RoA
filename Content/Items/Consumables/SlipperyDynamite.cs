using Microsoft.Xna.Framework;

using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Consumables;

sealed class SlipperyDynamite : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Slippery Dynamite");
		//Tooltip.SetDefault("A large explosion that will destroy most tiles\nSlips through solid tiles");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
	}

	public override void SetDefaults() {
		int width = 8; int height = 28;
		Item.Size = new Vector2(width, height);

		Item.damage = 0;
		Item.rare = ItemRarityID.White;

		Item.maxStack = 99;
		Item.noUseGraphic = true;

		Item.UseSound = SoundID.Item1;
		Item.value = Item.sellPrice(silver: 4);

		Item.useStyle = ItemUseStyleID.Swing;
		Item.useTime = Item.useAnimation = 40;

		Item.noMelee = true;
		Item.consumable = true;
		Item.autoReuse = false;

		Item.shoot = ModContent.ProjectileType<Projectiles.Friendly.Miscellaneous.SlipperyDynamite>();
		Item.shootSpeed = 4f;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ItemID.Dynamite)
	//		.AddIngredient<Galipot>()
	//		.Register();
	//}
}
