
using Microsoft.Xna.Framework;

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

    public static bool IsLocal(this Player player) => Main.myPlayer == player.whoAmI;

    public static void SetCompositeBothArms(this Player player, float armRotation, Player.CompositeArmStretchAmount compositeArmStretchAmount = Player.CompositeArmStretchAmount.Full) {
        player.SetCompositeArmBack(true, compositeArmStretchAmount, armRotation);
        player.SetCompositeArmFront(true, compositeArmStretchAmount, armRotation);
    }

    public static Vector2 GetViableMousePosition(this Player player) {
        Vector2 result = Main.MouseWorld;
        player.LimitPointToPlayerReachableArea(ref result);
        return result;
    }
}
