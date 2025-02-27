using Microsoft.Xna.Framework;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Potions;

sealed class WeaknessPotion : ModItem {
	public override void SetStaticDefaults() {
		//DisplayName.SetDefault("Weakness Potion");
		//Tooltip.SetDefault("Throw this to make someone weak");
		Item.ResearchUnlockCount = 20;
	}

	public override void SetDefaults() {
        int width = 22; int height = 22;
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

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.BuffPotion;
    }

    public override void AddRecipes() {
		CreateRecipe(3)
			.AddIngredient(ItemID.BottledWater, 3)
            .AddIngredient<Materials.Bonerose>()
            .AddIngredient<Materials.MiracleMint>()
            .AddIngredient(ItemID.RottenChunk)
			.AddTile(TileID.Bottles)
			.Register();

        CreateRecipe(3)
            .AddIngredient(ItemID.BottledWater, 3)
            .AddIngredient<Materials.Bonerose>()
            .AddIngredient<Materials.MiracleMint>()
            .AddIngredient(ItemID.Vertebrae)
            .AddTile(TileID.Bottles)
            .Register();
    }
}