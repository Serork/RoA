using RoA.Common.Druid;
using RoA.Content.Items;
using RoA.Content.Projectiles.Friendly;
using RoA.Core.Defaults;

using System;

using Terraria;

namespace RoA.Common.Crossmod;

public static class DruidModCalls {
    public static object Call(params object[] args) {
        RoA riseOfAges = RoA.Instance;
        Array.Resize(ref args, 5);
        string success = "Success";
        try {
            string message = args[0] as string;
            if (message == "MakeItemNature") {
                Item item = args[1] as Item;
                CrossmodNatureContent.RegisterNatureItem(item);
                return success;
            }
            if (message == "MakeItemNatureWeapon") {
                if (args[1] is not Item item) {
                    throw new Exception($"{args[1]} is not Item");
                }
                CrossmodNatureContent.RegisterNatureItem(item);
                item.MakeItemNatureWeapon();
                return success;
            }
            if (message == "SetNatureWeaponValues") {
                if (args[1] is not Item item) {
                    throw new Exception($"{args[1]} is not Item");
                }
                if (!CrossmodNatureContent.IsItemNature(item)) {
                    throw new Exception($"Item is not nature");
                }
                if (args[2] is not ushort potentialDamage) {
                    throw new Exception($"{args[2]} is not ushort");
                }
                float? fillingRateModifier = args[3] as float?;
                NatureWeaponHandler.SetPotentialDamage(item, potentialDamage);
                NatureWeaponHandler.SetFillingRateModifier(item, fillingRateModifier ?? 1f);
                return success;
            }
            if (message == "GetNatureWeaponBaseDamage") {
                if (args[1] is not Item item) {
                    throw new Exception($"{args[1]} is not Item");
                }
                if (args[2] is not Player player) {
                    throw new Exception($"{args[2]} is not Player");
                }
                return NatureWeaponHandler.GetFinalBaseDamage(item, player);
            }
            if (message == "GetNatureWeaponBasePotentialDamage") {
                if (args[1] is not Item item) {
                    throw new Exception($"{args[1]} is not Item");
                }
                if (args[2] is not Player player) {
                    throw new Exception($"{args[2]} is not Player");
                }
                return NatureWeaponHandler.GetBasePotentialDamage(item, player);
            }
            if (message == "GetNatureWeaponCurrentDamage") {
                if (args[1] is not Item item) {
                    throw new Exception($"{args[1]} is not Item");
                }
                if (args[2] is not Player player) {
                    throw new Exception($"{args[2]} is not Player");
                }
                return NatureWeaponHandler.GetNatureDamage(item, player);
            }
            if (message == "MakeProjectileNature") {
                if (args[1] is not Projectile projectile) {
                    throw new Exception($"{args[1]} is not projectile");
                }
                CrossmodNatureContent.MakeProjectileNature(projectile);
                return success;
            }
            if (message == "SetNatureProjectileValues") {
                if (args[1] is not Projectile projectile) {
                    throw new Exception($"{args[1]} is not projectile");
                }
                if (!CrossmodNatureContent.IsProjectileNature(projectile)) {
                    CrossmodNatureContent.MakeProjectileNature(projectile);
                }
                bool? shouldChargeWreathOnDamage = args[2] as bool?;
                bool? shouldApplyAttachedNatureWeaponCurrentDamage = args[3] as bool?;
                float? wreathFillingFine = args[4] as float?;
                NatureProjectile.SetNatureValues(projectile, shouldChargeWreathOnDamage ?? true, shouldApplyAttachedNatureWeaponCurrentDamage ?? true, wreathFillingFine ?? 0f);
                return success;
            }
            if (message == "SetAttachedNatureWeaponToNatureProjectile") {
                if (args[1] is not Projectile projectile) {
                    throw new Exception($"{args[1]} is not Projectile");
                }
                if (args[2] is not Item item) {
                    throw new Exception($"{args[2]} is not Item");
                }
                NatureProjectile.NatureProjectileSetItem(projectile, item);
                return success;
            }
            if (message == "GetAttachedNatureWeaponToNatureProjectile") {
                if (args[1] is not Projectile projectile) {
                    throw new Exception($"{args[1]} is not projectile");
                }
                if (!CrossmodNatureContent.IsProjectileNature(projectile)) {
                    CrossmodNatureContent.MakeProjectileNature(projectile);
                }
                return projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>().AttachedNatureWeapon;
            }
        }
        catch (Exception e) {
            riseOfAges.Logger.Error($"Call Error: {e.StackTrace} {e.Message}");
        }
        return null;
    }
}
