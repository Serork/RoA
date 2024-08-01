using RoA.Common.Druid.Wreath;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed class NatureWeaponHandler : GlobalItem {
    private ushort _basePotentialDamage;

    public int GetExtraDamage(Player player) => (int)(GetWreathStats(player).Progress * _basePotentialDamage);

    public bool HasPotentialDamage() => _basePotentialDamage > 0;

    public static WreathHandler GetWreathStats(Player player) => player.GetModPlayer<WreathHandler>();

    public static void SetPotentialDamage(Item item, ushort potentialDamage) => item.GetGlobalItem<NatureWeaponHandler>()._basePotentialDamage = (ushort)Math.Max(potentialDamage, item.damage);

    public override bool InstancePerEntity => true;

    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
        if (!player.IsHoldingNatureWeapon()) {
            return;
        }

        damage.Flat += GetExtraDamage(player);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!item.IsADruidicWeapon()) {
            return;
        }

        if (!HasPotentialDamage()) {
            return;
        }

        int index = tooltips.FindIndex(tooltip => tooltip.Name.Contains("Damage"));
        if (index != -1) {
            int extraDamage = GetExtraDamage(Main.LocalPlayer);
            if (extraDamage > 0) {
                string damageTooltip = tooltips[index].Text;
                string[] damageTooltipWords = damageTooltip.Split(' ');
                string damage = (item.damage + extraDamage).ToString();
                tooltips[index].Text = string.Concat(damage, $"(+{extraDamage}) ", damageTooltip.AsSpan(damage.Length).Trim());
            }
            string tag = "PotentialDamage";
            string potentialDamage = _basePotentialDamage.ToString();
            string tooltip = potentialDamage.PadRight(potentialDamage.Length + 1) + Language.GetOrRegister("Mods.RoA.Items.Tooltips.PotentialDamage").Value;
            tooltips.Insert(index + 1, new(Mod, tag, tooltip));
        }
    }
}
