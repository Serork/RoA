using Terraria.ModLoader;
using Terraria;
using Terraria.ID;

using RoA.Core;

namespace RoA.Content.Items.Miscellaneous;

sealed class LuminousFlower : ModItem {
	public override void SetDefaults() {
		Item.SetSize(26, 30);
		Item.SetDefaultOthers(Item.sellPrice(gold: 3, silver: 50), ItemRarityID.Blue);
	}
}