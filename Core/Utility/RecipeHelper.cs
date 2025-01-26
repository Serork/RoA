using System.Collections.Generic;
using System.Linq;

using Terraria;

namespace RoA.Core.Utility;

public static class RecipeHelper {
    public static Item RecipeItem(Recipe recipe, int itemID) {
        for (int i = 0; i < recipe.requiredItem.Count; i++) {
            Item item = recipe.requiredItem[i];
            if (item.type == itemID)
                return item;
        }
        return null;
    }

    public static void NewRecipe(int itemID, int[] newItemsIDs, int[] newItemsStacks, bool removeIngrediens = false, int[] removeItemsIDs = null, bool removeTile = false, int[] removeTilesIDs = null, int[] newTilesIDs = null) {
        IEnumerable<Recipe> itemRecipes = Main.recipe.Where(r => itemID.Equals(r.createItem.type));
        foreach (Recipe recipe in itemRecipes) {
            if (removeIngrediens)
                for (int i = 0; i < recipe.requiredItem.Count; i++)
                    recipe.RemoveIngredient(recipe.requiredItem[i]);
            if (removeTile)
                for (int i = 0; i < recipe.requiredTile.Count; i++)
                    recipe.RemoveTile(recipe.requiredTile[i]);
            if (removeItemsIDs != null)
                for (int i = 0; i < removeItemsIDs.Length; i++)
                    recipe.RemoveIngredient(removeItemsIDs[i]);
            if (removeTilesIDs != null)
                for (int i = 0; i < removeTilesIDs.Length; i++)
                    recipe.RemoveTile(removeTilesIDs[i]);
            for (int i = 0; i < newItemsIDs.Length; i++)
                recipe.AddIngredient(newItemsIDs[i], newItemsStacks[i]);
            if (newTilesIDs != null)
                for (int i = 0; i < newTilesIDs.Length; i++)
                    recipe.AddTile(newItemsIDs[i]);
        }
    }
}
