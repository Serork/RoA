using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Recipes;

sealed partial class RecipeDuplicationSystem : ModSystem {
    public override void PostAddRecipes() {
        foreach (ModItem item in ModContent.GetContent<ModItem>()) {
            if (item is not IRecipeDuplicatorItem duplicator) {
                continue;
            }

            int duplicateItemType = item.Type;
            Dictionary<ushort, HashSet<Recipe>> recipesByItemId = [];
            foreach (ushort sourceItemId in duplicator.SourceItemTypes) {
                bool IsIngredientSource(Item ingredient) => ingredient.type == sourceItemId;

                // duplicate recipes
                foreach (Recipe recipe in Main.recipe) {
                    if (recipe.requiredItem.Any(IsIngredientSource)) {
                        if (!recipesByItemId.TryGetValue(sourceItemId, out HashSet<Recipe>? recipes)) {
                            recipes = [];
                            recipesByItemId[sourceItemId] = recipes;
                        }
                        recipes.Add(recipe);
                    }
                }
                if (recipesByItemId.TryGetValue(sourceItemId, out HashSet<Recipe>? sourceRecipes)) {
                    foreach (Recipe originalRecipe in sourceRecipes) {
                        Recipe newRecipe = originalRecipe.Clone();
                        int ingredientIndex = newRecipe.requiredItem.FindIndex(IsIngredientSource);
                        newRecipe.requiredItem[ingredientIndex].SetDefaults(item.Type);
                        newRecipe.Register();
                    }
                }

                // add to recipe groups
                foreach (RecipeGroup recipeGroup in RecipeGroup.recipeGroups.Values) {
                    if (recipeGroup.ContainsItem(sourceItemId)) {
                        recipeGroup.ValidItems.Add(duplicateItemType);
                    }
                }
            }
        }
    }
}
