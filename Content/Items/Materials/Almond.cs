using Microsoft.Xna.Framework;

using RoA.Content.Buffs;

using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class Almond : ModItem {
	public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 5;

        //ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;

        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
        ItemID.Sets.FoodParticleColors[Item.type] = new Color[3] {
            new(191, 115, 63),
            new(227, 146, 90),
            new(171, 86, 43)
        };
		ItemID.Sets.IsFood[Type] = true;

        ItemID.Sets.ShimmerTransformToItem[Type] = 5342;
    }

	public override void SetDefaults() {
        Item.DefaultToFood(24, 25, 26, 18000);
        Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(0, 1));
    }
}
