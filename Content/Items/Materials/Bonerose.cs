using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

using RoA.Core;

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
    }
}
