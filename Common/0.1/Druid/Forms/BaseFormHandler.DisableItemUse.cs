using RoA.Common.Druid.Wreath;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Druid.Forms;

sealed class FormCanUseItem : GlobalItem {
    public override bool CanUseItem(Item item, Player player) {
        if (player.GetWreathHandler().StartSlowlyIncreasingUntilFull || player.GetFormHandler().IsInADruidicForm) {
            return false;
        }

        return base.CanUseItem(item, player);
    }
}