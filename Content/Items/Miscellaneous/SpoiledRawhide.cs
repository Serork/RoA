using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace RoA.Content.Items.Miscellaneous;

sealed class SpoiledRawhide : ModItem {
	public override void SetStaticDefaults() {
		// DisplayName.SetDefault("Spoiled Rawhide");
		// Tooltip.SetDefault("This hide wasn't treated fast enough");

		Item.ResearchUnlockCount = 50;
	}

	public override void SetDefaults() {
		int width = 18; int height = 20;
		Item.Size = new Vector2(width, height);

		Item.maxStack = 999;
		Item.rare = ItemRarityID.Gray;
	}
}