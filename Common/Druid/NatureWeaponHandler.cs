﻿using Microsoft.Xna.Framework;

using RoA.Common.Druid.Wreath;
using RoA.Content;
using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Druid;

sealed partial class NatureWeaponHandler : GlobalItem {
    private ushort _basePotentialDamage;
    private float _fillingRate = 1f;

    public float FillingRate => _fillingRate;

    public float GetFillingRate(Player player) => _fillingRate * player.GetModPlayer<DruidStats>().DruidDamageExtraIncreaseValueMultiplier;
    public bool HasPotentialDamage() => _basePotentialDamage > 0;

    public static ushort GetFinalBaseDamage(Item item, Player player) => (ushort)player.GetTotalDamage(DruidClass.NatureDamage).ApplyTo(item.damage);

    public static ushort GetBasePotentialDamage(Item item, Player player) {
        //Main.NewText(player.GetModPlayer<NatureWeaponStats>().DruidPotentialDamageMultiplier);
        return (ushort)(item.GetGlobalItem<NatureWeaponHandler>()._basePotentialDamage * player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier);
    }
    public static ushort GetPotentialDamage(Item item, Player player) => (ushort)(GetBasePotentialDamage(item, player) - GetFinalBaseDamage(item, player));

    public static int GetExtraPotentialDamage(Player player, Item item) {
        float progress = MathHelper.Clamp(GetWreathStats(player).Progress, 0f, 1f);
        return (int)(progress * GetPotentialDamage(item, player)) + (progress > 0.01f ? 1 : 0);
    }

    public static int GetExtraDamage(Item item, Player player) => Math.Min(GetExtraPotentialDamage(player, item), GetPotentialDamage(item, player));

    public static int GetNatureDamage(Item item, Player player) => GetFinalBaseDamage(item, player) + GetExtraDamage(item, player);

    public static WreathHandler GetWreathStats(Player player) => player.GetModPlayer<WreathHandler>();

    public static void SetPotentialDamage(Item item, ushort potentialDamage) => item.GetGlobalItem<NatureWeaponHandler>()._basePotentialDamage = (ushort)Math.Max(potentialDamage, item.damage);

    public static void SetFillingRate(Item item, float fillingRate) {
        NatureWeaponHandler self = item.GetGlobalItem<NatureWeaponHandler>();
        self._fillingRate = Math.Clamp(fillingRate, 0f, 2f);
    }

    public override bool InstancePerEntity => true;

    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
        if (!item.IsADruidicWeapon()) {
            return;
        }

        if (GetBasePotentialDamage(item, player) <= 0) {
            return;
        }

        int extraDamage = GetExtraDamage(item, player);
        damage.Flat += extraDamage;
        damage.Flat = Math.Min(item.damage + GetBasePotentialDamage(item, player), damage.Flat);
    }
}
