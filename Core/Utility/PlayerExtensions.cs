using Terraria;

namespace RoA.Core.Utility;

static class PlayerExtensions {
    public static Item GetSelectedItem(this Player player) => player.inventory[player.selectedItem];

    public static bool IsHoldingNatureWeapon(this Player player) {
        Item selectedItem = player.GetSelectedItem();
        if (!selectedItem.IsADruidicWeapon()) {
            return false;
        }

        return true;
    }
}
