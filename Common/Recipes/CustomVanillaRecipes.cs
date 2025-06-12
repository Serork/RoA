using RoA.Common.Configs;
using RoA.Content.Items.Materials;
using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Recipes;

sealed class CustomVanillaRecipes : ModSystem {
    public override void PostAddRecipes() {
        if (!ModContent.GetInstance<RoAServerConfig>().ChangeVanillaRecipes) {
            return;
        }

        for (int i = 0; i < Recipe.numRecipes; i++) {
            Recipe recipe = Main.recipe[i];
            if (recipe.HasResult(ItemID.Leather)) {
                recipe.DisableRecipe();
            }
        }

        // potions overhaul
        RecipeHelper.NewRecipe(ItemID.EndurancePotion, [ModContent.ItemType<Bonerose>()], [1], false, [ItemID.Blinkroot]);
        RecipeHelper.NewRecipe(ItemID.FeatherfallPotion, [ModContent.ItemType<Cloudberry>()], [1], false, [ItemID.Daybloom, ItemID.Blinkroot]);
        RecipeHelper.NewRecipe(ItemID.GravitationPotion, [ModContent.ItemType<Bonerose>(), ModContent.ItemType<MiracleMint>()], [1, 1], false, new int[] { ItemID.Deathweed, ItemID.Blinkroot });
        RecipeHelper.NewRecipe(ItemID.LifeforcePotion, [ItemID.Daybloom, ModContent.ItemType<Bonerose>()], [1, 1], false, [ItemID.Moonglow, ItemID.Waterleaf]);
        RecipeHelper.NewRecipe(ItemID.MagicPowerPotion, [ItemID.Fireblossom, ModContent.ItemType<MiracleMint>()], [1, 1], false, [ItemID.Moonglow, ItemID.Deathweed]);
        RecipeHelper.NewRecipe(ItemID.ManaRegenerationPotion, [ModContent.ItemType<MiracleMint>()], [1], false, [ItemID.Daybloom]);
        RecipeHelper.NewRecipe(ItemID.ThornsPotion, [ModContent.ItemType<Bonerose>()], [1], false, [ItemID.WormTooth]);
        RecipeHelper.NewRecipe(ItemID.WormholePotion, [ModContent.ItemType<MiracleMint>()], [1], false, [ItemID.Blinkroot]);

        //// items
        //RecipeHelper.NewRecipe(ItemID.WarAxeoftheNight, [ItemID.ShadowScale], [5], false);
        //RecipeHelper.NewRecipe(ItemID.BloodLustCluster, [ItemID.TissueSample], [5], false);

        // other herb related items
        RecipeHelper.NewRecipe(ItemID.GarlandHat, [ModContent.ItemType<MiracleMint>()], [1], false);
        RecipeHelper.NewRecipe(ItemID.GenderChangePotion, [ModContent.ItemType<MiracleMint>()], [1], false);
    }
}
