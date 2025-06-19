using Terraria;

namespace RoA.Core.Utility.Vanilla;

static class PlayerUtils {
    public static bool UpdateEquips_CanItemGrantBenefits(int itemSlot, Item item) {
        switch (itemSlot) {
            default:
                return true;
            case 0:
                return item.headSlot > -1;
            case 1:
                return item.bodySlot > -1;
            case 2:
                return item.legSlot > -1;
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
                return item.accessory;
        }
    }
}
