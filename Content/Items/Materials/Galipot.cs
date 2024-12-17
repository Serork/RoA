using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Items.Materials;

sealed class Galipot : ModItem {
	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 100;
		ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
	}

	public override void SetDefaults() {
		int width = 20; int height = 18;
		Item.Size = new Vector2(width, height);

		Item.value = Item.sellPrice(silver: 5);
		Item.rare = ItemRarityID.White;
		Item.maxStack = Item.CommonMaxStack;
	}
}