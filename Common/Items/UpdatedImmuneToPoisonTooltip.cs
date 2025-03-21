using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class UpdatedImmuneToPoisonTooltip : GlobalItem {
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (item.type == ItemID.Bezoar) {
            TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
            if (tt != null) {
                tt.Text = Language.GetText("Mods.RoA.Vanilla.BezoarDesc").Value;
            }
        }
        if (item.type == ItemID.MedicatedBandage) {
            TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
            if (tt != null) {
                tt.Text = Language.GetText("Mods.RoA.Vanilla.MedicatedBandageDesc").Value;
            }
        }
    }
}
