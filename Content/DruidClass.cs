using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Content.Prefixes;
using RoA.Core.Utility;

using System;
using System.Collections.Generic;
using System.Linq;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content;

sealed class DruidClass : DamageClass {
    public override void Load() {
        On_WorldGen.GiveItemGoodPrefixes += On_WorldGen_GiveItemGoodPrefixes;
    }

    private static Dictionary<int, DruidicPrefix> GoodPrefixIdsForDruidWeapon1 => DruidicPrefix.DruidicPrefixes.Where(x => DruidicPrefix.BestNotClaws.Contains(x.Value.Name)).ToDictionary();
    private static Dictionary<int, DruidicPrefix> GoodPrefixIdsForDruidWeapon2 => DruidicPrefix.DruidicPrefixes.Where(x => DruidicPrefix.BestClaws.Contains(x.Value.Name)).ToDictionary();

    private void On_WorldGen_GiveItemGoodPrefixes(On_WorldGen.orig_GiveItemGoodPrefixes orig, Item item) {
        orig(item);

        if (item.IsADruidicWeapon()) {
            if (item.ModItem is not BaseClawsItem) {
                PrefixItemFromOptions(item, GoodPrefixIdsForDruidWeapon1);
            }
            else {
                PrefixItemFromOptions(item, GoodPrefixIdsForDruidWeapon2);
            }
        }
    }

    private static void PrefixItemFromOptions(Item item, Dictionary<int, DruidicPrefix> options) {
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

    public static DruidClass NatureDamage => ModContent.GetInstance<DruidClass>();

    public override bool UseStandardCritCalcs => true;

    public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) => damageClass == Generic ? StatInheritanceData.Full : StatInheritanceData.None;

    public override bool GetEffectInheritance(DamageClass damageClass) => false;
}
