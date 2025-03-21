using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class TwigWreath : BaseWreathItem {   
    protected override void SafeSetDefaults() {
		int width = 20; int height = width; 
		Item.Size = new Vector2(width, height);

		Item.maxStack = 1;
		Item.rare = ItemRarityID.White;

        Item.value = Item.sellPrice(0, 0, 50, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        if (player.GetModPlayer<WreathHandler>().IsFull1) {
            player.endurance += 0.1f;
        }
    }
}
