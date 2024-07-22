using Terraria;

namespace RoA.Core.Utility;

static class PlayerExtensions {
    public static Item GetSelectedItem(this Player player) => player.inventory[player.selectedItem];
}
