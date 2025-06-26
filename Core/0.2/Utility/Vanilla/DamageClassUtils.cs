using RoA.Core.Utility.Extensions;

using System;
using System.Collections.Generic;

using Terraria.ModLoader;

namespace RoA.Core.Utility.Vanilla;

static class DamageClassUtils {
    public static bool IsNotGeneric(DamageClass damageClass) {
        return !(
            damageClass == DamageClass.Default ||
            damageClass == DamageClass.Generic ||
            damageClass == DamageClass.MagicSummonHybrid);
    }

    public static bool IsVanilla(DamageClass damageClass) {
        return damageClass == DamageClass.Magic ||
               damageClass == DamageClass.Melee ||
               damageClass == DamageClass.Ranged ||
               damageClass == DamageClass.Summon ||
               damageClass == DamageClass.Throwing;
    }

    public static IEnumerable<DamageClass> GetDamagesClasses(Predicate<DamageClass>? check = null) {
        for (int i = 0; i < DamageClassLoader.DamageClassCount; i++) {
            DamageClass damageClass = DamageClassLoader.GetDamageClass(i);
            if (check == null || check(damageClass)) {
                yield return damageClass;
            }
        }
    }

    public static IEnumerable<DamageClass> GetNotGenericDamagesClasses() => GetDamagesClasses(IsNotGeneric);

    public static IEnumerable<DamageClass> GetVanillaNotGenericDamagesClasses() => GetDamagesClasses((damageClass) => IsVanilla(damageClass) && IsNotGeneric(damageClass));
}
