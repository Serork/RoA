using RoA.Common.Druid.Wreath;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Forms;

sealed class FormCanUseItem : GlobalItem {
    public override bool CanUseItem(Item item, Player player) {
        if (player.GetModPlayer<WreathHandler>().StartSlowlyIncreasingUntilFull ||
            player.GetModPlayer<BaseFormHandler>().IsInADruidicForm) {
            return false;
        }

        return base.CanUseItem(item, player);
    }
}