using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class ResiliencePotion : ModItem {
	public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Resilience Potion");
        //Tooltip.SetDefault("Decreases Wreath discharge speed");
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[4] {
            new Color(108, 145, 210),
            new Color(30, 183, 210),
            new Color(9, 230, 197),
			new Color(135, 255, 203)
        };
    }

	public override void SetDefaults() {
        int width = 18; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 20;
        Item.rare = ItemRarityID.Green;

        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;

        Item.UseSound = SoundID.Item3;
        Item.consumable = true;

        Item.buffType = ModContent.BuffType<Resilience>();
		Item.buffTime = 3600 * 6;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ItemID.BottledWater)
	//		.AddIngredient<MiracleMint>()
	//		.AddIngredient(ItemID.Moonglow)
	//		.AddIngredient<Galipot>()
	//		.AddTile(TileID.Bottles)
	//		.Register();
	//}
}