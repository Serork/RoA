using RoA.Common.Configs;
using RoA.Content.Items.Materials;
using RoA.Core.Utility;

using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Recipes;

sealed class CustomVanillaRecipes : ModSystem {
    public override void PostAddRecipes() {
        // fragment items

        if (ModContent.GetInstance<RoAServerConfig>().ChangeLunarPillarLogic) {
            // celestial sigil
            {
                var celestialSigil = Main.recipe.First(x => x.HasResult(ItemID.CelestialSigil));
                celestialSigil.DisableRecipe();
                Recipe celestialSigilNewRecipe = Recipe.Create(ItemID.CelestialSigil, 1);
                celestialSigilNewRecipe
                    .AddIngredient(ItemID.FragmentSolar, 10)
                    .AddIngredient(ItemID.FragmentVortex, 10)
                    .AddIngredient(ItemID.FragmentNebula, 10)
                    .AddIngredient(ItemID.FragmentStardust, 10)
                    .AddIngredient(ModContent.ItemType<FilamentFragment>(), 10)
                    .AddTile(TileID.LunarCraftingStation)
                    .Register();
            }
            // lunar hook
            {
                var lunakHook = Main.recipe.First(x => x.HasResult(ItemID.LunarHook));
                lunakHook.DisableRecipe();
                Recipe lunarHookNewRecipe = Recipe.Create(ItemID.LunarHook, 1);
                lunarHookNewRecipe
                    .AddIngredient(ItemID.FragmentSolar, 5)
                    .AddIngredient(ItemID.FragmentVortex, 5)
                    .AddIngredient(ItemID.FragmentNebula, 5)
                    .AddIngredient(ItemID.FragmentStardust, 5)
                    .AddIngredient(ModContent.ItemType<FilamentFragment>(), 5)
                    .AddTile(TileID.LunarCraftingStation)
                    .Register();
            }
            // fragments
            {
                var nebulaFragment = Main.recipe.First(x => x.HasResult(ItemID.FragmentNebula));
                nebulaFragment.DisableRecipe();
                Recipe nebulaFragmentNewRecipe = Recipe.Create(ItemID.FragmentNebula, 1);
                nebulaFragmentNewRecipe
                    .AddIngredient(ItemID.FragmentSolar, 1)
                    .AddIngredient(ItemID.FragmentVortex, 1)
                    .AddIngredient(ItemID.FragmentStardust, 1)
                    .AddIngredient(ModContent.ItemType<FilamentFragment>(), 1)
                    .AddTile(TileID.LunarCraftingStation)
                    .Register();
                var solarFragment = Main.recipe.First(x => x.HasResult(ItemID.FragmentSolar));
                solarFragment.DisableRecipe();
                Recipe solarFragmentNewRecipe = Recipe.Create(ItemID.FragmentSolar, 1);
                solarFragmentNewRecipe
                    .AddIngredient(ItemID.FragmentVortex, 1)
                    .AddIngredient(ItemID.FragmentNebula, 1)
                    .AddIngredient(ItemID.FragmentStardust, 1)
                    .AddIngredient(ModContent.ItemType<FilamentFragment>(), 1)
                    .AddTile(TileID.LunarCraftingStation)
                    .Register();
                var vortexFragment = Main.recipe.First(x => x.HasResult(ItemID.FragmentVortex));
                vortexFragment.DisableRecipe();
                Recipe vortexFragmentNewRecipe = Recipe.Create(ItemID.FragmentVortex, 1);
                vortexFragmentNewRecipe
                    .AddIngredient(ItemID.FragmentSolar, 1)
                    .AddIngredient(ItemID.FragmentNebula, 1)
                    .AddIngredient(ItemID.FragmentStardust, 1)
                    .AddIngredient(ModContent.ItemType<FilamentFragment>(), 1)
                    .AddTile(TileID.LunarCraftingStation)
                    .Register();
                var stardustFragment = Main.recipe.First(x => x.HasResult(ItemID.FragmentStardust));
                stardustFragment.DisableRecipe();
                Recipe stardustFragmentNewRecipe = Recipe.Create(ItemID.FragmentStardust, 1);
                stardustFragmentNewRecipe
                    .AddIngredient(ItemID.FragmentSolar, 1)
                    .AddIngredient(ItemID.FragmentVortex, 1)
                    .AddIngredient(ItemID.FragmentNebula, 1)
                    .AddIngredient(ModContent.ItemType<FilamentFragment>(), 1)
                    .AddTile(TileID.LunarCraftingStation)
                    .Register();
            }
        }

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
