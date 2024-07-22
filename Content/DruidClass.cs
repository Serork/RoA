using Terraria.ModLoader;

namespace RoA.Content;

sealed class DruidClass : DamageClass {
    public static DruidClass NatureDamage => ModContent.GetInstance<DruidClass>();  

    public override bool UseStandardCritCalcs => false;

    public override StatInheritanceData GetModifierInheritance(DamageClass damageClass) => damageClass == Generic ? StatInheritanceData.Full : StatInheritanceData.None;

    public override bool GetEffectInheritance(DamageClass damageClass) => false;
}
