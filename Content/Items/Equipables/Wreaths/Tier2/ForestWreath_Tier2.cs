using Microsoft.Xna.Framework;

using RoA.Common.Druid;

using Terraria;
using Terraria.ID;

namespace RoA.Content.Items.Equipables.Wreaths.Tier2;

sealed class ForestWreathTier2 : WreathItem {
    protected override void SafeSetDefaults() {
        int width = 30; int height = 28;
        Item.Size = new Vector2(width, height);

        Item.maxStack = 1;
        Item.rare = ItemRarityID.Green;

        Item.value = Item.sellPrice(0, 0, 75, 0);
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        //float value = 0.1f * player.GetWreathHandler().ActualProgress4;
        //player.endurance += value;

        //if (player.GetWreathHandler().IsFull1) {
        //    player.statLifeMax2 += 40;
        //}

        DruidStats.ApplyUpTo5ReducedDamageTaken(player);

        DruidStats.Apply40MaximumLifeWhenCharged(player);
    }
}
