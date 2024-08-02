using RoA.Common.Druid.Wreath;
using RoA.Content;
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

    public ushort GetPotentialDamage(Item item) => (ushort)(_basePotentialDamage - item.damage);

    public int GetExtraDamage(Player player, Item item) {
        float progress = GetWreathStats(player).Progress;
        return (int)(progress * GetPotentialDamage(item)) + (progress > 0.01f ? 1 : 0);
    }

    public bool HasPotentialDamage() => _basePotentialDamage > 0;

    public bool CanItemBeHandled(Item item) => item.IsADruidicWeapon() && HasPotentialDamage();

    public static WreathHandler GetWreathStats(Player player) => player.GetModPlayer<WreathHandler>();

    public static void SetPotentialDamage(Item item, ushort potentialDamage) => item.GetGlobalItem<NatureWeaponHandler>()._basePotentialDamage = (ushort)Math.Max(potentialDamage, item.damage);

    public override bool InstancePerEntity => true;

    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
        if (!CanItemBeHandled(item)) {
            return;
        }

        int extraDamage = GetExtraDamage(player, item);
        damage.Flat += extraDamage;
        damage.Flat = Math.Min(item.damage + _basePotentialDamage, damage.Flat);
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        if (!CanItemBeHandled(item)) {
            return;
        }

        int index = tooltips.FindIndex(tooltip => tooltip.Name.Contains("Damage"));
        if (index != -1) {
            int extraDamage = Math.Min(GetExtraDamage(Main.LocalPlayer, item), GetPotentialDamage(item));
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
