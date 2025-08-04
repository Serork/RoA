using Terraria;
using Terraria.ModLoader;

namespace RoA.Core.Defaults;

static partial class ItemDefaults {
    public static void SetWeaponValues(this Item item, int dmg, float knockback, int bonusCritChance = 0, DamageClass? damageClass = null) {
        item.damage = dmg;
        item.knockBack = knockback;
        item.crit = bonusCritChance;
        if (damageClass != null) {
            item.DamageType = damageClass;
        }
    }
}
