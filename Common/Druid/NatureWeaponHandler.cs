using Microsoft.Xna.Framework;

using Newtonsoft.Json.Linq;

using RoA.Common.Druid.Wreath;
using RoA.Content;
using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Content.Prefixes;
using RoA.Core.Utility;

using System;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

using static System.Net.Mime.MediaTypeNames;

namespace RoA.Common.Druid;

sealed partial class NatureWeaponHandler : GlobalItem {
    private ushort _basePotentialDamage;
    private float _fillingRate = 1f;
    private ushort _basePotentialUseSpeed;

    internal DruidicPrefix ActivePrefix { get; set; }

    public float FillingRate => _fillingRate;
    public ushort PotentialDamage => _basePotentialDamage;

    public override bool InstancePerEntity => true;

    public override void PreReforge(Item item) {
        if (!item.IsADruidicWeapon()) {
            return;
        }

        ActivePrefix = null;
    }

    public override int ChoosePrefix(Item item, UnifiedRandom rand) {
        if (item.IsADruidicWeapon() && !item.accessory) {
            int result = DruidicPrefix.DruidicPrefixes.ElementAt(rand.Next(0, DruidicPrefix.DruidicPrefixes.Count)).Key;
            while (item.ModItem is not BaseClawsItem && DruidicPrefix.DruidicPrefixes[result]._forClaws) {
                result = DruidicPrefix.DruidicPrefixes.ElementAt(rand.Next(0, DruidicPrefix.DruidicPrefixes.Count)).Key;
            }
            while (item.ModItem is BaseClawsItem && !DruidicPrefix.DruidicPrefixes[result]._forClaws && !DruidicPrefix.DruidicPrefixes[result]._vanillaAdapted) {
                result = DruidicPrefix.DruidicPrefixes.ElementAt(rand.Next(0, DruidicPrefix.DruidicPrefixes.Count)).Key;
            }
            return result;
        }

        return base.ChoosePrefix(item, rand);
    }

    public static float GetSize(Item item) {
        NatureWeaponHandler handler = item.GetGlobalItem<NatureWeaponHandler>();
        DruidicPrefix activePrefix = handler.ActivePrefix;
        float result = item.scale;
        if (activePrefix != null) {
            result *= activePrefix._druidSize;
        }
        return result;
    }

    public static float GetFillingRate(Item item) {
        NatureWeaponHandler handler = item.GetGlobalItem<NatureWeaponHandler>();
        DruidicPrefix activePrefix = handler.ActivePrefix;
        float result = handler._fillingRate;
        if (activePrefix != null) {
            result *= activePrefix._fillingRateMult;
        }
        return result;
    }
    public bool HasPotentialDamage() => _basePotentialDamage > 0;
    public bool HasPotentialUseSpeed() => _basePotentialUseSpeed > 0;

    public static int GetItemDamage(Item item, Player player) {
        int result = item.damage;
        NatureWeaponHandler handler = item.GetGlobalItem<NatureWeaponHandler>();
        DruidicPrefix activePrefix = handler.ActivePrefix;
        if (activePrefix != null) {
            result += activePrefix._druidDamage;
            bool flag = Main.gameMenu || Main.InGameUI.IsVisible;
            result += flag ? 0 : player.GetModPlayer<DruidStats>().DruidBaseDamage;
            if (handler.HasPotentialDamage()) {
                result = (int)(result * activePrefix._druidDamageMult);
            }
        }
        return result;
    }

    public static int GetItemUseTime(Item item) {
        int result = item.useTime;
        NatureWeaponHandler handler = item.GetGlobalItem<NatureWeaponHandler>();
        DruidicPrefix activePrefix = handler.ActivePrefix;
        if (activePrefix != null) {
            result = (int)(result / activePrefix._druidSpeedMult);
        }
        return result;
    }

    public static ushort GetFinalBaseDamage(Item item, Player player) 
        => (ushort)(Main.gameMenu || Main.InGameUI.IsVisible ? GetItemDamage(item, player) : player.GetTotalDamage(DruidClass.NatureDamage).ApplyTo(GetItemDamage(item, player)));
    public static ushort GetFinalUseTime(Item item, Player player) => (ushort)(GetItemUseTime(item) / player.GetTotalAttackSpeed(DruidClass.NatureDamage));

    public static ushort GetBasePotentialDamage(Item item, Player player) {
        NatureWeaponHandler handler = item.GetGlobalItem<NatureWeaponHandler>();
        ushort baseDamage = handler._basePotentialDamage;
        DruidicPrefix activePrefix = handler.ActivePrefix;
        var stats = player.GetModPlayer<DruidStats>();
        bool flag = activePrefix != null;
        bool flag2 = Main.gameMenu || Main.InGameUI.IsVisible;
        baseDamage += (ushort)(flag2 ? 0 : stats.DruidPotentialDamage);
        if (flag) {
            baseDamage += activePrefix._potentialDamage;
        }
        ushort result = (ushort)(baseDamage * (flag2 ? 1f : stats.DruidPotentialDamageMultiplier));
        if (flag) {
            result = (ushort)(result * activePrefix._potentialDamageMult);
        }
        result = (ushort)(result * (flag2 ? 1f : player.GetTotalDamage(DamageClass.Generic).ApplyTo(1f)));
        return (ushort)result;
    }
    public static ushort GetPotentialDamage(Item item, Player player) => (ushort)Math.Max(0, GetBasePotentialDamage(item, player) - GetFinalBaseDamage(item, player));

    public static ushort GetBasePotentialUseSpeed(Item item, Player player) {
        NatureWeaponHandler handler = item.GetGlobalItem<NatureWeaponHandler>();
        ushort baseSpeed = handler._basePotentialUseSpeed;
        DruidicPrefix activePrefix = handler.ActivePrefix;
        bool flag = activePrefix != null;
        ushort result = (ushort)(baseSpeed / (Main.gameMenu || Main.InGameUI.IsVisible ? 1f : player.GetModPlayer<DruidStats>().DruidPotentialUseTimeMultiplier));
        if (flag) {
            result = (ushort)(result / activePrefix._potentialDruidSpeedMult);
        }
        return (ushort)result;
    }

    public static ushort GetPotentialUseSpeed(Item item, Player player) => (ushort)Math.Max(0, GetFinalUseTime(item, player) - GetBasePotentialUseSpeed(item, player));

    public static ushort GetExtraPotentialDamage(Player player, Item item) {
        float progress = MathHelper.Clamp(Main.gameMenu || Main.InGameUI.IsVisible ? 0f : GetWreathStats(player).Progress, 0f, 1f);
        return (ushort)((ushort)(progress * GetPotentialDamage(item, player)) + (progress > 0.01f ? 1 : 0));
    }
    public static ushort GetExtraPotentialUseSpeed(Player player, Item item) {
        float progress = MathHelper.Clamp(Main.gameMenu || Main.InGameUI.IsVisible ? 0f : GetWreathStats(player).Progress, 0f, 1f);
        return (ushort)((ushort)(progress * GetPotentialUseSpeed(item, player)) + (progress > 0.01f ? 1 : 0));
    }

    public static ushort GetExtraDamage(Item item, Player player) => Math.Min(GetExtraPotentialDamage(player, item), GetPotentialDamage(item, player));
    public static ushort GetExtraUseSpeed(Item item, Player player) => Math.Min(GetExtraPotentialUseSpeed(player, item), GetPotentialUseSpeed(item, player));

    public static ushort GetNatureDamage(Item item, Player player) => !item.GetGlobalItem<NatureWeaponHandler>().HasPotentialDamage() ? GetFinalBaseDamage(item, player) : (ushort)(GetFinalBaseDamage(item, player) + GetExtraDamage(item, player));
    public static ushort GetUseSpeed(Item item, Player player) => !item.GetGlobalItem<NatureWeaponHandler>().HasPotentialUseSpeed() ? GetFinalUseTime(item, player) : (ushort)(GetFinalUseTime(item, player) - GetExtraUseSpeed(item, player));
    public static ushort GetUseSpeedForClaws(Item item, Player player) =>
        (ushort)(GetUseSpeed(item, player) - (Math.Min(player.GetAttackSpeed(DamageClass.Melee), 2f) - 1f) * GetUseSpeed(item, player) * 0.5f);

    public static WreathHandler GetWreathStats(Player player) => player.GetModPlayer<WreathHandler>();

    public static void SetPotentialDamage(Item item, ushort potentialDamage) => item.GetGlobalItem<NatureWeaponHandler>()._basePotentialDamage = (ushort)Math.Max(potentialDamage, GetItemDamage(item, Main.LocalPlayer));
    public static void SetPotentialUseSpeed(Item item, ushort potentialUseTime) => item.GetGlobalItem<NatureWeaponHandler>()._basePotentialUseSpeed = (ushort)Math.Min(potentialUseTime, GetItemUseTime(item));

    public static void SetFillingRate(Item item, float fillingRate) {
        NatureWeaponHandler self = item.GetGlobalItem<NatureWeaponHandler>();
        self._fillingRate = Math.Clamp(fillingRate, 0f, 2f);
    }

    public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback) {
        if (!item.IsADruidicWeapon()) {
            return;
        }

        if (ActivePrefix != null) {
            knockback.Flat += ActivePrefix._druidKnockback;
            knockback *= ActivePrefix._druidKnockbackMult;
        }
    }

    public override void ModifyWeaponCrit(Item item, Player player, ref float crit) {
        if (!item.IsADruidicWeapon()) {
            return;
        }

        if (ActivePrefix != null) {
            crit += ActivePrefix._druidCrit;
        }
    }

    public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage) {
        if (!item.IsADruidicWeapon()) {
            return;
        }

        if (ActivePrefix != null) {
            damage.Flat += ActivePrefix._druidDamage;
        }

        if (GetBasePotentialDamage(item, player) > 0) {
            int extraDamage = GetExtraDamage(item, player);
            damage.Flat += extraDamage;
            damage.Flat = Math.Min(GetItemDamage(item, player) + GetBasePotentialDamage(item, player), damage.Flat);
        }

        bool claws = item.ModItem is BaseClawsItem;
        if (ActivePrefix != null) {
            if (HasPotentialDamage()) {
                damage *= ActivePrefix._druidDamageMult;
            }
            else {
                if (ActivePrefix._vanillaAdapted) {
                    float modifier = 1f + (ActivePrefix._druidDamageMult + ActivePrefix._potentialDamageMult - 2f) / (ActivePrefix._forClaws || claws ? 2f : 4f);
                    damage *= modifier;
                }
                else {
                    damage *= (ActivePrefix._druidDamageMult + ActivePrefix._potentialDamageMult) / 2f;
                }
            }
        }
    }

    public override bool? UseItem(Item item, Player player) {
        if (item.IsADruidicWeapon() && ActivePrefix != null && ActivePrefix._shouldApplyTipsy && player.ItemAnimationJustStarted) {
            player.AddBuff(BuffID.Tipsy, 300);

            return base.UseItem(item, player);
        }

        return base.UseItem(item, player);
    }

    public override float UseSpeedMultiplier(Item item, Player player) {
        if (!item.IsADruidicWeapon()) {
            return base.UseSpeedMultiplier(item, player);
        }

        float mult = 1f;
        if (GetBasePotentialUseSpeed(item, player) > 0) {
            float result = 1f + GetExtraUseSpeed(item, player) / GetItemUseTime(item);
            return result / mult;
        }

        if (ActivePrefix != null) {
            mult *= ActivePrefix._druidSpeedMult;
        }
        return base.UseSpeedMultiplier(item, player) * mult;
    }

    public static float GetUseSpeedMultiplier(Item item, Player player) => item.ModItem.UseSpeedMultiplier(player);
}
