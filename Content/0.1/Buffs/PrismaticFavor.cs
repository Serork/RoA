using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class PrismaticFavor : ModBuff {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Bloodlust");
        // Description.SetDefault("You restore some health on deadly hits");
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<PrismaticFavorPlayer>().IsEffectActive = true;

    internal class PrismaticFavorPlayer : ModPlayer {
        public bool IsEffectActive;

        public override void ResetEffects() => IsEffectActive = false;

        public override void PostUpdate() {
            if (!IsEffectActive) {
                return;
            }

            float totalDamage = 0f;
            int invalidDamageClassCount = 5;
            static bool isValid(DamageClass damageClass) {
                return damageClass is not null && !(
                    damageClass == DamageClass.Default ||
                    damageClass == DamageClass.Generic ||
                    damageClass == DamageClass.MeleeNoSpeed ||
                    damageClass == DamageClass.SummonMeleeSpeed ||
                    damageClass == DamageClass.MagicSummonHybrid);
            }
            for (int i = 0; i < DamageClassLoader.DamageClassCount; i++) {
                DamageClass damageClass = DamageClassLoader.GetDamageClass(i);
                if (!isValid(damageClass)) {
                    continue;
                }
                float value = Player.GetTotalDamage(damageClass).ApplyTo(1f);
                totalDamage += value - 1f;
            }
            for (int i = 0; i < DamageClassLoader.DamageClassCount; i++) {
                DamageClass damageClass = DamageClassLoader.GetDamageClass(i);
                if (!isValid(damageClass)) {
                    continue;
                }
                float addDamage = totalDamage;
                float value = Player.GetTotalDamage(damageClass).ApplyTo(1f);
                addDamage -= value - 1f;
                float genericBonus = (Player.GetTotalDamage(DamageClass.Generic).ApplyTo(1f) - 1f) * (DamageClassLoader.DamageClassCount - invalidDamageClassCount - 1);
                addDamage -= genericBonus;
                Player.GetDamage(damageClass) += addDamage;
            }
        }
    }
}