using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class BrightstonePotion : ModItem {
	public override void SetStaticDefaults() {
		// DisplayName.SetDefault("Brightstone Potion");
		// Tooltip.SetDefault("Causes you to emit lumps of light");
		Item.ResearchUnlockCount = 20;
		ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
			new Color(255, 165, 0),
			new Color(255, 215, 0),
			new Color(255, 225, 0)
		};
	}

	public override void SetDefaults() {
        int width = 18; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Green;

        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;

        Item.UseSound = SoundID.Item3;
        Item.consumable = true;

        Item.buffType = ModContent.BuffType<Brightstone>();
		Item.buffTime = 3600 * 10;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ItemID.BottledWater)
	//		.AddIngredient(ItemID.Sunflower)
	//		.AddIngredient(ItemID.Moonglow)
	//		.AddIngredient<Galipot>()
	//		.AddTile(TileID.Bottles)
	//		.Register();
	//}
}