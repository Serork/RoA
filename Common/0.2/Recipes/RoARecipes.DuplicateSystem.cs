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

            Dictionary<ushort, HashSet<Recipe>> recipesByItemId = new();
            foreach (ushort sourceItemId in duplicator.SourceItemTypes) {
                bool IsIngredientSource(Item ingredient) => ingredient.type == sourceItemId;
                for (int i = 0; i < Main.recipe.Length; i++) {
                    Recipe recipe = Main.recipe[i];
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
            }
        }
    }
}
