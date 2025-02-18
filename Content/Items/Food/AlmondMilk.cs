using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Food;

sealed class AlmondMilk : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 5;

        //ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.DrinkParticleColors[Item.type] = new Color[3] {
            new(221, 223, 227),
            new(214, 205, 197),
            new(206, 192, 180)
        };
        ItemID.Sets.IsFood[Type] = true;
    }

    public override void SetDefaults() {
        Item.DefaultToFood(24, 42, 26, 36000, useGulpSound: true);
        Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(0, 1));
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<Almond>(), 2)
            .AddIngredient(ItemID.Bottle)
            .AddTile(TileID.CookingPots)
            .Register();
    }
}
