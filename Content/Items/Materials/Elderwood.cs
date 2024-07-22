using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

using RoA.Core;

namespace RoA.Content.Items.Materials;

sealed class Elderwood : ModItem {
	public override void SetStaticDefaults() {
		Item.ResearchUnlockCount = 100;

		ItemID.Sets.SortingPriorityMaterials[Item.type] = ItemID.Wood;
	}

	public override void SetDefaults() {
		Item.SetSize(24, 22);

		Item.SetUsableValues(ItemUseStyleID.Swing, 15, 10, useTurn: true, autoReuse: true);

		Item.rare = ItemRarityID.White;

		Item.SetDefaultToStackable(999);

		//Item.createTile = ModContent.TileType<Tiles.Crafting.Elderwood>();
	}
}