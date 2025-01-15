using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class DryadBloodPotion : ModItem {
	public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Dryad Blood Potion");
        //Tooltip.SetDefault("Increases resistance to damaging debuffs");
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[4] {
            new Color(239, 221, 145),
            new Color(214, 192, 71),
            new Color(206, 209, 70),
            new Color(161, 165, 44)
        };
    }

	public override void SetDefaults() {
        int width = 20; int height = 30;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Green;

        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;

        Item.UseSound = SoundID.Item3;
        Item.consumable = true;

        Item.buffType = ModContent.BuffType<DryadBlood>();
		Item.buffTime = 3600 * 8;
	}

	public override void AddRecipes() {
		CreateRecipe()
			.AddIngredient(ItemID.BottledWater)
			.AddIngredient<Materials.Bonerose>()
			.AddIngredient(ItemID.Waterleaf)
			.AddIngredient<Materials.Galipot>()
			.AddTile(TileID.Bottles)
			.Register();
	}
}