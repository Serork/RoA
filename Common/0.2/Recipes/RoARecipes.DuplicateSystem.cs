using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Recipes;

sealed partial class RecipeDuplicationSystem : ModSystem {
    public override void AddRecipeGroups() {
        foreach (ModItem item in ModContent.GetContent<ModItem>()) {
            if (item is not IRecipeDuplicatorItem duplicator) {
                continue;
            }

            int duplicateItemType = item.Type;
            ushort[] sourceItemTypes = duplicator.SourceItemTypes;
            foreach (ushort sourceItemId in sourceItemTypes) {
                foreach (RecipeGroup recipeGroup in RecipeGroup.recipeGroups.Values) {
                    if (recipeGroup.ContainsItem(sourceItemId) && !recipeGroup.ValidItems.Contains(duplicateItemType)) {
                        recipeGroup.ValidItems.Add(duplicateItemType);
                    }
                }
            }
        }
    }

    public override void PostAddRecipes() {
        foreach (ModItem item in ModContent.GetContent<ModItem>()) {
            if (item is not IRecipeDuplicatorItem duplicator) {
                continue;
            }

            int duplicateItemType = item.Type;
            Dictionary<ushort, HashSet<Recipe>> recipesByItemId = [];
            ushort[] sourceItemTypes = duplicator.SourceItemTypes;
            foreach (ushort sourceItemId in sourceItemTypes) {
                bool IsIngredientSource(Item ingredient) => ingredient.type == sourceItemId;
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
            }
        }
    }
}
