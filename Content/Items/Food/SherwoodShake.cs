using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
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

        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 0, 40, 0);
    }
}
