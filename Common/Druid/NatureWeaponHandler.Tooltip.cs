﻿using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed partial class NatureWeaponHandler : GlobalItem {
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!item.IsADruidicWeapon()) {
            return;
        }

        int index = tooltips.FindIndex(tooltip => tooltip.Name.Contains("Damage"));
        if (index != -1) {
            string tag, tooltip;
            if (HasPotentialDamage()) {
                int extraDamage = GetExtraDamage(item);
                if (extraDamage > 0) {
                    string damageTooltip = tooltips[index].Text;
                    string[] damageTooltipWords = damageTooltip.Split(' ');
                    string damage = GetNatureDamage(item).ToString();
                    tooltips[index].Text = string.Concat(damage, $"(+{extraDamage}) ", damageTooltip.AsSpan(damage.Length).Trim());
                }
                tag = "PotentialDamage";
                string potentialDamage = _basePotentialDamage.ToString();
                tooltip = potentialDamage.AddSpace() + Language.GetOrRegister("Mods.RoA.Items.Tooltips.PotentialDamage").Value;
                tooltips.Insert(index + 1, new(Mod, tag, tooltip));
                index++;
            }

            tag = "FillingRate";
            int fillingRate = (int)(_fillingRate * 100);
            byte tooltipValue = (byte)(fillingRate / 25 + 1); 
            tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.FillingRate{tooltipValue}").Value;
            tooltips.Insert(index + 2, new(Mod, tag, tooltip));
        }
    }
}