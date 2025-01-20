using Humanizer;

using Microsoft.Xna.Framework;

using RoA.Core;
using RoA.Core.Utility;
using RoA.Utilities;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed partial class NatureWeaponHandler : GlobalItem {
    private static string GetLocalizedText(string name) => Language.GetOrRegister("Mods.RoA.Items.Tooltips." + name).Value;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!item.IsADruidicWeapon()) {
            return;
        }

        int index = tooltips.FindIndex(tooltip => tooltip.Name.Contains("Damage"));
        if (index != -1) {
            string tag, tooltip;
            string keyword = GetLocalizedText("PotentialKeyWord");
            if (HasPotentialDamage()) {
                int extraDamage = GetExtraDamage(item, Main.LocalPlayer);
                if (extraDamage > 0) {
                    string damageTooltip = tooltips[index].Text;
                    string[] damageTooltipWords = damageTooltip.Split(' ');
                    string damage = GetNatureDamage(item, Main.LocalPlayer).ToString();
                    tooltips[index].Text = string.Concat(damage, $"(+{extraDamage}) ", damageTooltip.AsSpan(damage.Length).Trim());
                }
                tag = "PotentialDamage";
                string potentialDamage = GetBasePotentialDamage(item, Main.LocalPlayer).ToString();
                tooltip = potentialDamage.AddSpace() + GetLocalizedText("PotentialDamage");
                tooltips.Insert(index + 1, new(Mod, tag, tooltip));
                index++;
            }
            int speedIndex = tooltips.FindIndex(tooltip => tooltip.Name.Contains("Speed"));
            if (speedIndex != -1 && item.useAnimation > 0) {
                tooltips.RemoveAt(speedIndex);
                speedIndex -= 1;
                tag = "BaseSpeed";
                ushort useSpeed = (ushort)(GetUseSpeed(item, Main.LocalPlayer) * 2);
                useSpeed -= (ushort)(useSpeed / 3);
                if (useSpeed <= 8)
                    tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{8}").Value;
                else if (useSpeed <= 20)
                    tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{7}").Value;
                else if (useSpeed <= 25)
                    tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{6}").Value;
                else if (useSpeed <= 30)
                    tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{5}").Value;
                else if (useSpeed <= 35)
                    tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{4}").Value;
                else if (useSpeed <= 45)
                    tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{3}").Value;
                else if (useSpeed <= 55)
                    tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{2}").Value;
                else
                    tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{1}").Value;
                int extraUseSpeed = GetExtraUseSpeed(item, Main.LocalPlayer) * 2;
                extraUseSpeed -= (ushort)(extraUseSpeed / 3);
                if (HasPotentialUseSpeed()) {
                    if (extraUseSpeed > 0) {
                        int maxUseSpeed = (GetFinalUseTime(item, Main.LocalPlayer) * 2);
                        maxUseSpeed -= maxUseSpeed / 3;
                        int procent = (int)(extraUseSpeed / (float)maxUseSpeed * 100f);
                        tooltip += $" (+{procent}%)";
                    }
                }
                tooltips.Insert(speedIndex + 1, new(Mod, tag, tooltip));

                if (HasPotentialUseSpeed()) {
                    tag = "PotentialSpeed";
                    useSpeed = (ushort)(GetBasePotentialUseSpeed(item, Main.LocalPlayer) * 2);
                    useSpeed -= (ushort)(useSpeed / 3);
                    if (useSpeed <= 8)
                        tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{8}2").Value;
                    else if (useSpeed <= 20)
                        tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{7}2").Value;
                    else if (useSpeed <= 25)
                        tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{6}2").Value;
                    else if (useSpeed <= 30)
                        tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{5}2").Value;
                    else if (useSpeed <= 35)
                        tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{4}2").Value;
                    else if (useSpeed <= 45)
                        tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{3}2").Value;
                    else if (useSpeed <= 55)
                        tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{2}2").Value;
                    else
                        tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.AttackSpeed{1}2").Value;

                    tooltips.Insert(speedIndex + 2, new(Mod, tag, tooltip));
                }
            }
            int knockbackIndex = tooltips.FindIndex(tooltip => tooltip.Name.Contains("Knockback"));
            tag = "FillingRate";
            int fillingRate = (int)(GetFillingRate(item) * Main.LocalPlayer.GetModPlayer<DruidStats>().DruidDamageExtraIncreaseValueMultiplier * 100);
            byte tooltipValue = (byte)Math.Clamp(fillingRate / 20, 1, 7); 
            tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.FillingRate{tooltipValue}").Value;
            tooltips.Insert(knockbackIndex + 1, new(Mod, tag, tooltip));
        }
    }}
