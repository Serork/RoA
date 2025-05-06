using System;

using RoA.Common.Druid;
using RoA.Content.Items;
using RoA.Content.Projectiles.Friendly;
using RoA.Core.Defaults;

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
            if (message == "MakeItemDruidicWeapon") {
                if (args[1] is not Item item) {
                    throw new Exception($"{args[1]} is not Item");
                }
                CrossmodNatureContent.RegisterNatureItem(item);
                item.MakeItemDruidicWeapon();
                return success;
            }
            if (message == "SetDruidicWeaponPotentialDamage") {
                if (args[1] is not Item item) {
                    throw new Exception($"{args[1]} is not Item");
                }
                if (!CrossmodNatureContent.IsItemNature(item)) {
                    throw new Exception($"Item is not nature");
                }
                if (args[2] is not ushort damage) {
                    throw new Exception($"{args[2]} is not ushort");
                }
                NatureWeaponHandler.SetPotentialDamage(item, damage);
                return success;
            }
            if (message == "SetDruidicWeaponFillingRate") {
                if (args[1] is not Item item) {
                    throw new Exception($"{args[1]} is not Item");
                }
                if (!CrossmodNatureContent.IsItemNature(item)) {
                    throw new Exception($"Item is not nature");
                }
                if (args[2] is not float fillingRate) {
                    throw new Exception($"{args[2]} is not float");
                }
                NatureWeaponHandler.SetFillingRate(item, fillingRate);
                return success;
            }
            if (message == "MakeProjectileDruidicDamageable") {
                if (args[1] is not Projectile projectile) {
                    throw new Exception($"{args[1]} is not projectile");
                }
                CrossmodNatureContent.RegisterNatureProjectile(projectile);
                projectile.MakeProjectileDruidicDamageable();
                return success;
            }
            if (message == "SetDruidicProjectileValues") {
                if (args[1] is not Projectile projectile) {
                    throw new Exception($"{args[1]} is not projectile");
                }
                if (!CrossmodNatureContent.IsProjectileNature(projectile)) {
                    throw new Exception($"Projectile is not nature");
                }
                bool? shouldChargeWreathOnDamage = args[2] as bool?;
                bool? shouldApplyAttachedNatureWeaponCurrentDamage = args[3] as bool?;
                float? wreathFillingFine = args[4] as float?;
                DruidicProjectile.NatureProjectileSetValues(projectile, shouldChargeWreathOnDamage ?? true, shouldApplyAttachedNatureWeaponCurrentDamage ?? true, wreathFillingFine ?? 0f);
                return success;
            }
            if (message == "SetAttachedItemToDruidicProjectile") {
                if (args[1] is not Projectile projectile) {
                    throw new Exception($"{args[1]} is not Projectile");
                }
                if (args[2] is not Item item) {
                    throw new Exception($"{args[2]} is not Item");
                }
                DruidicProjectile.NatureProjectileSetItem(projectile, item);
                return success;
            }
            if (message == "GetAttachedNatureWeaponToDruidicProjectile") {
                if (args[1] is not Projectile projectile) {
                    throw new Exception($"{args[1]} is not projectile");
                }
                if (!CrossmodNatureContent.IsProjectileNature(projectile)) {
                    throw new Exception($"Projectile is not nature");
                }
                return projectile.GetGlobalProjectile<CrossmodNatureProjectileHandler>().AttachedNatureWeapon;
            }
            if (message == "GetAttachedNatureWeaponToDruidicProjectile") {
                if (args[1] is not Item item) {
                    throw new Exception($"{args[1]} is not Item");
                }
                if (args[2] is not Player player) {
                    throw new Exception($"{args[2]} is not Player");
                }
                return NatureWeaponHandler.GetBasePotentialDamage(item, player);
            }
        }
        catch (Exception e) {
            riseOfAges.Logger.Error($"Call Error: {e.StackTrace} {e.Message}");
        }
        return null;
    }
}
