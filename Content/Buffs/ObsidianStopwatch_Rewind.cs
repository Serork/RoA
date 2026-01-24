using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Rewind : ModBuff {
    public override void Update(Player player, ref int buffIndex) {
        player.lifeRegen += 2;
        player.statDefense += 4;
        player.GetAttackSpeed(DamageClass.Melee) += 0.1f;
        player.GetDamage(DamageClass.Generic) += 0.1f;
        player.GetCritChance(DamageClass.Generic) += 2;
        /*
        meleeDamage += 0.1f;
        meleeCrit += 2;
        rangedDamage += 0.1f;
        rangedCrit += 2;
        magicDamage += 0.1f;
        magicCrit += 2;
        */
        player.pickSpeed -= 0.15f;
        /*
        minionDamage += 0.1f;
        */
        player.GetKnockback(DamageClass.Summon).Base += 0.5f;
    }
}
