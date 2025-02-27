using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class BeachWreath2 : BaseWreathItem {
	protected override void SafeSetDefaults() {
		int width = 30; int height = 28;
		Item.Size = new Vector2(width, height);

		Item.maxStack = 1;
		Item.value = Item.sellPrice(gold: 1, silver: 50);
		Item.rare = ItemRarityID.Green;
	}

    public override void UpdateAccessory(Player player, bool hideVisual) {

    }
}
