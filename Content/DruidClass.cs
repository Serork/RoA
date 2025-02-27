using System.Runtime.CompilerServices;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content;

sealed class DruidClass : DamageClass {
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
