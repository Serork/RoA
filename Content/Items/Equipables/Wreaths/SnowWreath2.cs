using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class SnowWreath2 : BaseWreathItem {
	protected override void SafeSetDefaults() {
		int width = 30; int height = 28;
		Item.Size = new Vector2(width, height);

		Item.maxStack = 1;
		Item.rare = ItemRarityID.Green;

        Item.value = Item.sellPrice(0, 0, 75, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        WreathHandler handler = player.GetModPlayer<WreathHandler>();
        if (!handler.IsEmpty2) {
            player.endurance += 0.1f;
        }
        if (handler.IsFull) {
            player.GetCritChance(DruidClass.NatureDamage) += 2;
        }
    }
}
