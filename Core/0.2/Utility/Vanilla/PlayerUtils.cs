using RoA.Common.Druid.Forms;
using RoA.Common.Druid.Wreath;
using RoA.Common.Players;

using Terraria;

namespace RoA.Core.Utility.Vanilla;

static class PlayerUtils {
    public static BaseFormHandler GetFormHandler(this Player player) => player.GetModPlayer<BaseFormHandler>();
    public static WreathHandler GetWreathHandler(this Player player) => player.GetModPlayer<WreathHandler>();

    public static PlayerCommon GetCommon(this Player player) => player.GetModPlayer<PlayerCommon>();

    public static bool UpdateEquips_CanItemGrantBenefits(int itemSlot, Item item) {
        return itemSlot switch {
            0 => item.headSlot > -1,
            1 => item.bodySlot > -1,
            2 => item.legSlot > -1,
            3 or 4 or 5 or 6 or 7 or 8 or 9 => item.accessory,
            _ => true,
        };
    }
}
