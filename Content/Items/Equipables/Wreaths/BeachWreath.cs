using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths;

sealed class BeachWreath : BaseWreathItem {
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
