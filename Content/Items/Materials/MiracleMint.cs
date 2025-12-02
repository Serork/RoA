using RoA.Core.Defaults;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class MiracleMint : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 25;

        //ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
    }

    public override void SetDefaults() {
        Item.SetSizeValues(20, 22);

        Item.SetOtherValues(Item.sellPrice(copper: 20));

        Item.SetDefaultsToStackable(Item.CommonMaxStack);

        Item.value = Item.sellPrice(0, 0, 0, 20);
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.AlchemyPlants;
    }
}
