using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class WillpowerPotion : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Willpower Potion");
		//Tooltip.SetDefault("Increases Wreath charge speed");
		CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(221, 82, 24),
            new Color(248, 127, 63),
            new Color(255, 168, 82)
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

        Item.buffType = ModContent.BuffType<Willpower>();
		Item.buffTime = 3600 * 6;
	}

	//public override void AddRecipes() {
	//	CreateRecipe()
	//		.AddIngredient(ItemID.BottledWater)
	//		.AddIngredient<MiracleMint>()
	//		.AddIngredient(ItemID.Shiverthorn)
	//		.AddIngredient<Galipot>()
	//		.AddTile(TileID.Bottles)
	//		.Register();
	//}
}