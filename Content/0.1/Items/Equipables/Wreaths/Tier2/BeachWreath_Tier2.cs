using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths.Tier2;

sealed class BeachWreathTier2 : WreathItem {
    protected override void SafeSetDefaults() {
        int width = 30; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Green;

        Item.value = Item.sellPrice(0, 0, 75, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        //WreathHandler handler = player.GetWreathHandler();
        //float value = 0.1f * handler.ActualProgress4;
        //player.endurance += value;
        //if (player.GetWreathHandler().IsFull1) {
        //    player.moveSpeed += 0.15f;
        //}

        DruidStats.ApplyUpTo5ReducedDamageTaken(player);

        DruidStats.Apply15MovementSpeedWhenCharged(player);
    }
}
