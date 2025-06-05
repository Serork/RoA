using RoA.Common.Druid;
using RoA.Core.Defaults;

using Terraria;

namespace RoA.Content.Items.Equipables.Wreaths.Tier3;

sealed class BeachWreathTier3 : WreathItem {
    protected override void SafeSetDefaults() {
        Item.SetSize(width: 30, height: 28);

        DefaultsToTier3Wreath();
    }

    public override void UpdateAccessory(Player player, bool hideVisual) {
        DruidStats.ApplyUpTo10ReducedDamageTaken(player);

        DruidStats.Apply15MovementSpeedWhenCharged(player);

        RandomlyGrowCorals();
    }

    private void RandomlyGrowCorals() { }
}
