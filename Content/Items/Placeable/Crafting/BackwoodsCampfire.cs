using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Placeable.Crafting;

sealed class BackwoodsCampfire : ModItem {
    public override void SetDefaults() {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Crafting.BackwoodsCampfire>(), 0);
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddRecipeGroup(RecipeGroupID.Wood, 10)
            .AddIngredient<ElderTorch>(5)
            .SortAfterFirstRecipesOf(ItemID.JungleCampfire)
            .Register();
    }
}
