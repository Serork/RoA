using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Food;

sealed class SherwoodShake : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 5;

        //ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.DrinkParticleColors[Item.type] = new Color[3] {
            new(185, 27, 69),
            new(217, 232, 171),
            new(144, 222, 159)
        };
        ItemID.Sets.IsFood[Type] = true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Food;
    }

    public override void SetDefaults() {
        Item.DefaultToFood(14, 34, 26, 72000, useGulpSound: true);
        Item.SetShopValues(ItemRarityColor.Green2, Item.buyPrice(0, 2));
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<Pistachio>())
            .AddIngredient(ItemID.Cherry)
            .AddIngredient(ItemID.Bottle)
            .AddTile(TileID.CookingPots)
            .Register();
    }
}
