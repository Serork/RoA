using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

using RoA.Core;

namespace RoA.Content.Items.Materials;

sealed class MiracleMint : ModItem {
	public override void SetStaticDefaults() {
		// DisplayName.SetDefault("Miracle Mint");
		Item.ResearchUnlockCount = 25;

		ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
	}

	public override void SetDefaults() {
        Item.SetSize(30, 22);

        Item.value = Item.sellPrice(copper: 20);
		Item.rare = ItemRarityID.White;

        Item.SetDefaultToStackable(999);
    }
}
