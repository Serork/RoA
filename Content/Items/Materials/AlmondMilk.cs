using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class AlmondMilk : ModItem {
	public override void SetStaticDefaults() {
        //Item.ResearchUnlockCount = 25;

        //ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.FoodParticleColors[Item.type] = new Color[3] {
            new(191, 115, 63),
            new(227, 146, 90),
            new(171, 86, 43)
        };
		ItemID.Sets.IsFood[Type] = true;
    }

	public override void SetDefaults() {
        Item.DefaultToFood(24, 42, 26, 36000, useGulpSound: true);
        Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(0, 2));
    }

    public override void AddRecipes() {
        CreateRecipe()
            .AddIngredient(ModContent.ItemType<Almond>())
            .AddIngredient(ItemID.Bottle)
            .AddTile(TileID.CookingPots)
            .Register();
    }
}
