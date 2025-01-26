using RoA.Content.Items.Materials;
using RoA.Core.Utility;

using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class CustomRecipes : ModSystem {
    public override void PostAddRecipes() {
        // potions overhaul
        RecipeHelper.NewRecipe(ItemID.EndurancePotion, new int[] { ModContent.ItemType<Bonerose>() }, new int[] { 1 }, false, new int[] { ItemID.Blinkroot });
        RecipeHelper.NewRecipe(ItemID.FeatherfallPotion, new int[] { ModContent.ItemType<Cloudberry>() }, new int[] { 1 }, false, new int[] { ItemID.Daybloom, ItemID.Blinkroot });
        RecipeHelper.NewRecipe(ItemID.GravitationPotion, new int[] { ModContent.ItemType<Bonerose>(), ModContent.ItemType<MiracleMint>() }, new int[] { 1, 1 }, false, new int[] { ItemID.Deathweed, ItemID.Blinkroot });
        RecipeHelper.NewRecipe(ItemID.LifeforcePotion, new int[] { ItemID.Daybloom, ModContent.ItemType<Bonerose>() }, new int[] { 1, 1 }, false, new int[] { ItemID.Moonglow, ItemID.Waterleaf });
        RecipeHelper.NewRecipe(ItemID.MagicPowerPotion, new int[] { ItemID.Fireblossom, ModContent.ItemType<MiracleMint>() }, new int[] { 1, 1 }, false, new int[] { ItemID.Moonglow, ItemID.Deathweed });
        RecipeHelper.NewRecipe(ItemID.ManaRegenerationPotion, new int[] { ModContent.ItemType<MiracleMint>() }, new int[] { 1 }, false, new int[] { ItemID.Daybloom });
        RecipeHelper.NewRecipe(ItemID.ThornsPotion, new int[] { ModContent.ItemType<Bonerose>() }, new int[] { 1 }, false, new int[] { ItemID.WormTooth });
        RecipeHelper.NewRecipe(ItemID.WormholePotion, new int[] { ModContent.ItemType<MiracleMint>() }, new int[] { 1 }, false, new int[] { ItemID.Blinkroot });
        
        // items
        RecipeHelper.NewRecipe(ItemID.WarAxeoftheNight, new int[] { ItemID.ShadowScale }, new int[] { 5 }, false);
        RecipeHelper.NewRecipe(ItemID.BloodLustCluster, new int[] { ItemID.TissueSample }, new int[] { 5 }, false);
    }
}
