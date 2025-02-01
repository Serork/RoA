using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class Pistachio : ModItem {
	public override void SetStaticDefaults() {
        //Item.ResearchUnlockCount = 25;

        //ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.FoodParticleColors[Item.type] = new Color[3] {
            new(191, 151, 87),
            new(164, 119, 48),
            new(116, 164, 69)
        };
		ItemID.Sets.IsFood[Type] = true;

        ItemID.Sets.ShimmerTransformToItem[Type] = 5342;
    }

	public override void SetDefaults() {
        Item.DefaultToFood(22, 27, 26, 18000);
        Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(0, 1));
    }
}
