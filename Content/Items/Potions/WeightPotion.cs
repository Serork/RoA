using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class WeightPotion : ModItem {
	public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Weight Potion");
        //Tooltip.SetDefault("Negates fall damage\nAccelerates falling speed");
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[4] {
            new Color(144, 169, 175),
            new Color(87, 100, 122),
            new Color(85, 120, 129),
            new Color(104, 138, 146)
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

        Item.buffType = ModContent.BuffType<Heavy>();
		Item.buffTime = 3600 * 6;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ItemID.BottledWater)
	//		.AddIngredient<Cloudberry>()
	//		.AddIngredient(ItemID.Deathweed)
	//		.AddTile(TileID.Bottles)
	//		.Register();
	//}
}