using RoA.Core;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class Bonerose : ModItem {
    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 25;

        //ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
    }

    public override void SetDefaults() {
        Item.SetSize(20, 24);

        Item.SetDefaultOthers(Item.sellPrice(copper: 20));

        Item.SetDefaultToStackable(Item.CommonMaxStack);

        Item.value = Item.sellPrice(0, 0, 0, 50);
    }
}
