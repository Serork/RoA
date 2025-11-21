using RoA.Content.Items.Weapons.Nature;
using RoA.Content.Prefixes;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Common.Items;

class DruidClass_Claws : DruidClass {
    public static new DruidClass_Claws Nature => ModContent.GetInstance<DruidClass_Claws>();

    public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) {
        if (damageClass == Melee)
            return new StatInheritanceData(attackSpeedInheritance: 1f);

        if (damageClass == Generic || damageClass == DruidClass.Nature)
            return StatInheritanceData.Full;

        return StatInheritanceData.None;
    }
}

class DruidClass : DamageClass {
    public override void Load() {
        On_WorldGen.GiveItemGoodPrefixes += On_WorldGen_GiveItemGoodPrefixes;
    }

    private static Dictionary<int, NaturePrefix> GoodPrefixIdsForDruidWeapon1 => NaturePrefix.NaturePrefixes.Where(x => NaturePrefix.BestNotClaws.Contains(x.Value.Name)).ToDictionary();
    private static Dictionary<int, NaturePrefix> GoodPrefixIdsForDruidWeapon2 => NaturePrefix.NaturePrefixes.Where(x => NaturePrefix.BestClaws.Contains(x.Value.Name)).ToDictionary();

    private void On_WorldGen_GiveItemGoodPrefixes(On_WorldGen.orig_GiveItemGoodPrefixes orig, Item item) {
        orig(item);

        if (item.IsANatureWeapon()) {
            if (item.ModItem is not ClawsBaseItem) {
                PrefixItemFromOptions(item, GoodPrefixIdsForDruidWeapon1);
            }
            else {
                PrefixItemFromOptions(item, GoodPrefixIdsForDruidWeapon2);
            }
        }
    }

    private static void PrefixItemFromOptions(Item item, Dictionary<int, NaturePrefix> options) {
        int prefix = item.prefix;

        var list = options;
        while (list.Count > 0) {
            var prefix2 = GoodPrefixIdsForDruidWeapon1.ElementAt(WorldGen.genRand.Next(0, GoodPrefixIdsForDruidWeapon1.Count));
            int index = prefix2.Key;
            int num = index;
            item.Prefix(num);
            if (item.prefix == num) {
                return;
            }

            list.Remove(index);
        }

        item.Prefix(prefix);
    }

    private class ResearchSorting : GlobalItem {
        public override void ModifyResearchSorting(Item item, ref ContentSamples.CreativeHelper.ItemGroup itemGroup) {
            if (item.DamageType.CountsAsClass<DruidClass>()) {
                itemGroup = (ContentSamples.CreativeHelper.ItemGroup)554;
            }
        }
    }

    public static DruidClass Nature => ModContent.GetInstance<DruidClass>();

    public override bool UseStandardCritCalcs => true;

    public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) => damageClass == Generic ? StatInheritanceData.Full : StatInheritanceData.None;

    public override bool GetEffectInheritance(DamageClass damageClass) => false;
}
