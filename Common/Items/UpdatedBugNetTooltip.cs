﻿using System.Collections.Generic;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using System.Linq;

namespace RoA.Common.Items;

sealed class UpdatedBugNetTooltip : GlobalItem {
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (item.type == ItemID.GoldenBugNet) {
            TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
            if (tt != null) {
                tt.Text = Language.GetText("Mods.RoA.Vanilla.GoldenBugNetDesc").Value;
            }
            tt = tooltips.FirstOrDefault(x => x.Name == "Tooltip1" && x.Mod == "Terraria");
            tt?.Hide();
        }
        if (item.type == ItemID.FireproofBugNet) {
            TooltipLine tt = tooltips.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
            if (tt != null) {
                tt.Text = Language.GetText("Mods.RoA.Vanilla.LavaproofBugNetDesc").Value;
            }
        }
    }
}