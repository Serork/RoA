using RoA.Content.Items.Equipables.Wreaths;
using RoA.Content.Items.Materials;

using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Recipes;

sealed partial class RoARecipes : ModSystem {
    private static void V02Recipes() {
        V02AddWreaths();
    }

    private static void V02AddWreaths() {
        Recipe.Create(ModContent.ItemType<ForestWreathTier3>())
            .AddIngredient<ForestWreath2>(1)
            .AddIngredient(ItemID.CrystalShard, 5)
            .AddTile(TileID.MythrilAnvil)
            .SortAfter(GetAddedWreathRecipe<ForestWreath2>())
            .AddOnCraftCallback(CompleteWreathCraftAchievement)
            .Register();

        Recipe.Create(ModContent.ItemType<JungleWreathTier3>())
            .AddIngredient<JungleWreath2>(1)
            .AddIngredient(ItemID.CrystalShard, 5)
            .AddTile(TileID.MythrilAnvil)
            .SortAfter(GetAddedWreathRecipe<JungleWreath2>())
            .AddOnCraftCallback(CompleteWreathCraftAchievement)
            .Register();

        Recipe.Create(ModContent.ItemType<BeachWreathTier3>())
            .AddIngredient<BeachWreath2>(1)
            .AddIngredient(ItemID.CrystalShard, 5)
            .AddTile(TileID.MythrilAnvil)
            .SortAfter(GetAddedWreathRecipe<BeachWreath2>())
            .AddOnCraftCallback(CompleteWreathCraftAchievement)
            .Register();

        Recipe.Create(ModContent.ItemType<SnowWreathTier3>())
            .AddIngredient<SnowWreath2>(1)
            .AddIngredient(ItemID.CrystalShard, 5)
            .AddTile(TileID.MythrilAnvil)
            .SortAfter(GetAddedWreathRecipe<SnowWreath2>())
            .AddOnCraftCallback(CompleteWreathCraftAchievement)
            .Register();
    }
}