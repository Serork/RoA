using RoA.Common.Configs;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;

using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed partial class NatureWeaponHandler : GlobalItem {
    private static string GetLocalizedText(string name) => Language.GetOrRegister("Mods.RoA.Items.Tooltips." + name).Value;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!item.IsANatureWeapon()) {
            return;
        }

        int index = tooltips.FindIndex(tooltip => tooltip.Name.Contains("Damage") || tooltip.Name.Contains("Damage_Druid"));
        if (index != -1) {
            Player player = Main.LocalPlayer;
            string tag, tooltip;
            string keyword = GetLocalizedText("PotentialKeyWord");
            if (HasPotentialDamage()) {
                var config = ModContent.GetInstance<RoAClientConfig>();
                string damageTooltip = tooltips[index].Text;
                string natureDamageText = Language.GetText("Mods.RoA.DamageClasses.DruidClass.DisplayName").Value;
                int extraDamage = GetExtraDamage(item, player);
                switch (Main.gameMenu || Main.InGameUI.IsVisible ? DamageTooltipOptionConfigElement2.modifying : config.DamageTooltipOption) {
                    case RoAClientConfig.DamageTooltipOptions.Option1:
                        tooltips[index].Text = $"{GetFinalBaseDamage(item, player)}-{GetBasePotentialDamage(item, player)} {natureDamageText}";
                        break;
                    case RoAClientConfig.DamageTooltipOptions.Option2:
                        tooltips[index].Text = $"{GetNatureDamage(item, player)} ({GetFinalBaseDamage(item, player)}-{GetBasePotentialDamage(item, player)}) {natureDamageText}";
                        break;
                    case RoAClientConfig.DamageTooltipOptions.Option3:
                        int maxExtraDamage = GetPotentialDamage(item, player);
                        float progress = (float)extraDamage / maxExtraDamage;
                        tooltips[index].Text =
                            $"{GetNatureDamage(item, player)}/{GetBasePotentialDamage(item, player)} {natureDamageText}" +
                            " " + $"({(int)(progress * 100f)}% {GetLocalizedText("DruidToolTipOption3")})";
                        break;
                    case RoAClientConfig.DamageTooltipOptions.Option4:
                        tooltips[index].Text = $"{GetFinalBaseDamage(item, player)} {GetLocalizedText("DruidToolTipOption4_1")}";
                        tag = "PotentialDamage";
                        tooltip = $"{GetBasePotentialDamage(item, player)} {GetLocalizedText("PotentialDamage")}";
                        tooltips.Insert(index + 1, new(Mod, tag, tooltip));
                        index++;
                        break;
                    case RoAClientConfig.DamageTooltipOptions.Option5:
                        string[] damageTooltipWords = damageTooltip.Split(' ');
                        string damage = GetNatureDamage(item, player).ToString();
                        if (extraDamage > 0) {
                            tooltips[index].Text = string.Concat(damage, $"(+{extraDamage}) ", damageTooltip.AsSpan(damage.Length).Trim());
                        }
                        else {
                            string extra = " ";
                            if (Main.gameMenu || Main.InGameUI.IsVisible) {
                                extra = "(+0) ";
                            }
                            tooltips[index].Text = $"{damage}{extra}{natureDamageText}";
                        }
                        tag = "PotentialDamage";
                        string potentialDamage = GetBasePotentialDamage(item, player).ToString();
                        tooltip = $"{GetBasePotentialDamage(item, player)} {GetLocalizedText("PotentialDamage")}";
                        tooltips.Insert(index + 1, new(Mod, tag, tooltip));
                        index++;
                        break;
                }
                if (Main.gameMenu || Main.InGameUI.IsVisible ? BooleanElement2.Value2 : config.DamageScaling) {
                    tag = "DruidDamageTip";
                    tooltip = GetLocalizedText("DruidToolTipOption1");
                    tooltips.Insert(index + 1, new(Mod, tag, tooltip));
                }
            }
            if (Main.gameMenu) {
                return;
            }
            int speedIndex = tooltips.FindIndex(tooltip => tooltip.Name.Contains("Speed"));
            if (speedIndex != -1 && item.useAnimation > 0 && !item.accessory) {
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
                        int percentage = (int)(extraUseSpeed / (float)maxUseSpeed * 100f);
                        tooltip += $" (+{percentage}%)";
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
            byte tooltipValue = (byte)Math.Clamp(fillingRate / 10, 1, 7);
            tooltip = Language.GetOrRegister($"Mods.RoA.Items.Tooltips.FillingRate{tooltipValue}").Value;
            tooltips.Insert(knockbackIndex + 1, new(Mod, tag, tooltip));
        }
    }
}
