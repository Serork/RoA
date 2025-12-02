using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
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

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Food;
    }

    public override void SetDefaults() {
        Item.DefaultToFood(24, 42, 26, 36000, useGulpSound: true);

        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 20, 0);
    }
}
