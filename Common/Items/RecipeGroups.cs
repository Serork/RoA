using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using RoA.Content.Items.Placeable.Crafting;
using RoA.Content.Items.Materials;

namespace RoA.Common.Items;

sealed class RecipeGroups : ModSystem {
    public override void AddRecipeGroups() {
        RecipeGroup.recipeGroups[RecipeGroupID.Wood].ValidItems.Add(ModContent.ItemType<Elderwood>());

        RecipeGroup.recipeGroups[RecipeGroupID.Fruit].ValidItems.Add(ModContent.ItemType<Almond>());
        RecipeGroup.recipeGroups[RecipeGroupID.Fruit].ValidItems.Add(ModContent.ItemType<Pistachio>());
    }
}
