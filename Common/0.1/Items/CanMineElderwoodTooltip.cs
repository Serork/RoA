using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.Items;

sealed class CanMineElderwoodTooltip : GlobalItem {
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (item.pick is 65 or 70 && item.rare < ItemRarityID.LightRed) {
            TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
            if (tt != null) {
                tt.Text = Language.GetText("Mods.RoA.Vanilla.CanMineElderwood").Value;
            }
        }
        if (item.axe is 15 && item.rare < ItemRarityID.LightRed) {
            TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
            string text = Language.GetText("Mods.RoA.Vanilla.CanChopElderwood").Value;
            if (tt != null) {
                tt.Text = text;
            }
            else {
                tooltips.Add(new(Mod, "CanChopElderwoodTooltipLine", text));
            }
        }
    }
}
