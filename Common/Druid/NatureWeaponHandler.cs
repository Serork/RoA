using Microsoft.Xna.Framework;

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
    private ushort _basePotentialUseSpeed;

    public float FillingRate => _fillingRate;

    public float GetFillingRate(Player player) => _fillingRate * player.GetModPlayer<DruidStats>().DruidDamageExtraIncreaseValueMultiplier;
    public bool HasPotentialDamage() => _basePotentialDamage > 0;
    public bool HasPotentialUseSpeed() => _basePotentialUseSpeed > 0;

    public static ushort GetFinalBaseDamage(Item item, Player player) => (ushort)player.GetTotalDamage(DruidClass.NatureDamage).ApplyTo(item.damage);
    public static ushort GetFinalUseTime(Item item, Player player) => (ushort)(item.useTime - (player.GetTotalAttackSpeed(DruidClass.NatureDamage) - 1f) * item.useTime);

    public static ushort GetBasePotentialDamage(Item item, Player player) {
        return (ushort)(item.GetGlobalItem<NatureWeaponHandler>()._basePotentialDamage * player.GetModPlayer<DruidStats>().DruidPotentialDamageMultiplier);
    }
    public static ushort GetPotentialDamage(Item item, Player player) => (ushort)(GetBasePotentialDamage(item, player) - GetFinalBaseDamage(item, player));

    public static ushort GetBasePotentialUseSpeed(Item item, Player player) {
        return (ushort)(item.GetGlobalItem<NatureWeaponHandler>()._basePotentialUseSpeed * player.GetModPlayer<DruidStats>().DruidPotentialUseTimeMultiplier);
    }
    public static ushort GetPotentialUseSpeed(Item item, Player player) => (ushort)(GetFinalUseTime(item, player) - GetBasePotentialUseSpeed(item, player));

    public static ushort GetExtraPotentialDamage(Player player, Item item) {
        float progress = MathHelper.Clamp(GetWreathStats(player).Progress, 0f, 1f);
        return (ushort)((ushort)(progress * GetPotentialDamage(item, player)) + (progress > 0.01f ? 1 : 0));
    }
    public static ushort GetExtraPotentialUseSpeed(Player player, Item item) {
        float progress = MathHelper.Clamp(GetWreathStats(player).Progress, 0f, 1f);
        return (ushort)((ushort)(progress * GetPotentialUseSpeed(item, player)) + (progress > 0.01f ? 1 : 0));
    }

    public static ushort GetExtraDamage(Item item, Player player) => Math.Min(GetExtraPotentialDamage(player, item), GetPotentialDamage(item, player));
    public static ushort GetExtraUseSpeed(Item item, Player player) => Math.Min(GetExtraPotentialUseSpeed(player, item), GetPotentialUseSpeed(item, player));

    public static ushort GetNatureDamage(Item item, Player player) => !item.GetGlobalItem<NatureWeaponHandler>().HasPotentialDamage() ? GetFinalBaseDamage(item, player) : (ushort)(GetFinalBaseDamage(item, player) + GetExtraDamage(item, player));
    public static ushort GetUseSpeed(Item item, Player player) => !item.GetGlobalItem<NatureWeaponHandler>().HasPotentialUseSpeed() ? GetFinalUseTime(item, player) : (ushort)(GetFinalUseTime(item, player) - GetExtraUseSpeed(item, player));

    public static WreathHandler GetWreathStats(Player player) => player.GetModPlayer<WreathHandler>();

    public static void SetPotentialDamage(Item item, ushort potentialDamage) => item.GetGlobalItem<NatureWeaponHandler>()._basePotentialDamage = (ushort)Math.Max(potentialDamage, item.damage);
    public static void SetPotentialUseSpeed(Item item, ushort potentialUseTime) => item.GetGlobalItem<NatureWeaponHandler>()._basePotentialUseSpeed = (ushort)Math.Min(potentialUseTime, item.useTime);

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

    public override float UseSpeedMultiplier(Item item, Player player) {
        if (!item.IsADruidicWeapon()) {
            return base.UseSpeedMultiplier(item, player);
        }

        if (GetBasePotentialUseSpeed(item, player) <= 0) {
            return base.UseSpeedMultiplier(item, player);
        }

        return 1f + GetExtraUseSpeed(item, player) / item.useTime;
    }

    public static float GetUseSpeedMultiplier(Item item, Player player) => item.ModItem.UseSpeedMultiplier(player);
}
