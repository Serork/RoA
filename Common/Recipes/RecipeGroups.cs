using RoA.Content.Items.Food;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Items.Placeable.Solid;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Recipes;

sealed class RecipeGroups : ModSystem {
    public override void AddRecipeGroups() {
        RecipeGroup.recipeGroups[RecipeGroupID.Wood].ValidItems.Add(ModContent.ItemType<Elderwood>());

        RecipeGroup.recipeGroups[RecipeGroupID.Fruit].ValidItems.Add(ModContent.ItemType<Almond>());
        RecipeGroup.recipeGroups[RecipeGroupID.Fruit].ValidItems.Add(ModContent.ItemType<Pistachio>());
    }
}
