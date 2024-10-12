using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class BloodlustPotion : ModItem {
	public override void SetStaticDefaults () {
		// DisplayName.SetDefault("Bloodlust Potion");
		// Tooltip.SetDefault("Restores life on deadly hits");

		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId [Type] = 20;
		ItemID.Sets.DrinkParticleColors [Type] = new Color [3] {
			new Color(255, 140, 10),
			new Color(255, 140, 10),
			new Color(255, 69, 20)
		};
	}

	public override void SetDefaults () {
		int width = 18; int height = 30;
		Item.Size = new Vector2(width, height);

		Item.maxStack = 20;
		Item.rare = ItemRarityID.Green;

		Item.useTime = Item.useAnimation = 15;
		Item.useStyle = ItemUseStyleID.DrinkLiquid;

		Item.UseSound = SoundID.Item3;
		Item.consumable = true;

		Item.buffType = ModContent.BuffType<Bloodlust>();
		Item.buffTime = 3600 / 2;
	}

	//public override void AddRecipes () {
	//	CreateRecipe()
	//		.AddIngredient(ItemID.BottledWater)
	//		.AddIngredient(ItemID.Deathweed)
	//		.AddIngredient<Bonerose>()
	//		.AddIngredient(ItemID.Fireblossom)
	//		.AddTile(TileID.Bottles)
	//		.Register();
	//}
}