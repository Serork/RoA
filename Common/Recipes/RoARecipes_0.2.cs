using RoA.Content.Items.Equipables.Wreaths.Tier2;
using RoA.Content.Items.Equipables.Wreaths.Tier3;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Recipes;

sealed partial class RoARecipes : ModSystem {
    private static void V02Recipes() {
        V02AddWreaths();
    }

    private static void V02AddWreaths() {
        Recipe.Create(ModContent.ItemType<ForestWreathTier3>())
            .AddIngredient<ForestWreathTier2>(1)
            .AddIngredient(ItemID.CrystalShard, 5)
            .AddTile(TileID.MythrilAnvil)
            .SortAfter(GetAddedWreathRecipe<ForestWreathTier2>())
            .AddOnCraftCallback(CompleteWreathCraftAchievement)
            .Register();

        Recipe.Create(ModContent.ItemType<JungleWreathTier3>())
            .AddIngredient<JungleWreathTier2>(1)
            .AddIngredient(ItemID.CrystalShard, 5)
            .AddTile(TileID.MythrilAnvil)
            .SortAfter(GetAddedWreathRecipe<JungleWreathTier2>())
            .AddOnCraftCallback(CompleteWreathCraftAchievement)
            .Register();

        Recipe.Create(ModContent.ItemType<BeachWreathTier3>())
            .AddIngredient<BeachWreathTier2>(1)
            .AddIngredient(ItemID.CrystalShard, 5)
            .AddTile(TileID.MythrilAnvil)
            .SortAfter(GetAddedWreathRecipe<BeachWreathTier2>())
            .AddOnCraftCallback(CompleteWreathCraftAchievement)
            .Register();

        Recipe.Create(ModContent.ItemType<SnowWreathTier3>())
            .AddIngredient<SnowWreathTier2>(1)
            .AddIngredient(ItemID.CrystalShard, 5)
            .AddTile(TileID.MythrilAnvil)
            .SortAfter(GetAddedWreathRecipe<SnowWreathTier2>())
            .AddOnCraftCallback(CompleteWreathCraftAchievement)
            .Register();
    }
}