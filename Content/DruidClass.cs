using RoA.Content.Items.Weapons.Druidic.Claws;
using RoA.Content.Prefixes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content;

sealed class DruidClass : DamageClass {
    public override void Load() {
        On_WorldGen.GiveItemGoodPrefixes += On_WorldGen_GiveItemGoodPrefixes;
    }

    private static IEnumerable<DruidicPrefix> GoodPrefixIdsForDruidWeapon1 = DruidicPrefix.DruidicPrefixes.Values.Where(x => DruidicPrefix.BestNotClaws.Contains(x.Name));
    private static IEnumerable<DruidicPrefix> GoodPrefixIdsForDruidWeapon2 = DruidicPrefix.DruidicPrefixes.Values.Where(x => DruidicPrefix.BestClaws.Contains(x.Name));

    private void On_WorldGen_GiveItemGoodPrefixes(On_WorldGen.orig_GiveItemGoodPrefixes orig, Item item) {
        orig(item);

        if (item.DamageType == NatureDamage) {
            if (item.ModItem is not BaseClawsItem) {
                PrefixItemFromOptions(item, GoodPrefixIdsForDruidWeapon1.ToList());
            }
            else {
                PrefixItemFromOptions(item, GoodPrefixIdsForDruidWeapon2.ToList());
            }
        }
    }

    private static void PrefixItemFromOptions(Item item, List<DruidicPrefix> options) {
        int prefix = item.prefix;
        if (!item.Prefix(-3))
            return;

        var list = options;
        while (list.Count > 0) {
            int index = WorldGen.genRand.Next(list.Count);
            int num = DruidicPrefix.DruidicPrefixes.ElementAt(index).Key;
            item.Prefix(num);
            if (item.prefix == num) {
                Console.WriteLine(DruidicPrefix.DruidicPrefixes.ElementAt(index));
                return;
            }

            list.RemoveAt(index);
        }

        item.Prefix(prefix);
    }

    private sealed class ResearchSorting : GlobalItem {
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
