using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class BeachWreath : WreathItem {
    protected override void SafeSetDefaults() {
        int width = 30; int height = 26;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Blue;

        Item.value = Item.sellPrice(0, 0, 50, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        float value = 0.05f * player.GetModPlayer<WreathHandler>().ActualProgress4;
        player.endurance += value;
		
		if (player.GetModPlayer<WreathHandler>().IsFull1) {
            player.moveSpeed += 0.075f;
        }
    }
}

sealed class BeachWreath2 : WreathItem {
    protected override void SafeSetDefaults() {
        int width = 30; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Green;

        Item.value = Item.sellPrice(0, 0, 75, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        WreathHandler handler = player.GetModPlayer<WreathHandler>();
        float value = 0.1f * handler.ActualProgress4;
        player.endurance += value;
        if (player.GetModPlayer<WreathHandler>().IsFull1) {
            player.moveSpeed += 0.15f;
        }
    }
}
