using RoA.Core.Utility;

using System;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Bloodlust : ModBuff {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Bloodlust");
        // Description.SetDefault("You restore some health on deadly hits");
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<BloodlustPlayer>().bloodlustEffect = true;
}

sealed class BloodlustPlayer : ModPlayer {
    public bool bloodlustEffect;

    public override void ResetEffects() => bloodlustEffect = false;

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
        if (bloodlustEffect && target.life <= 0 && target.CanActivateOnHitEffect()) {
            BloodlustHeal(Math.Max(1, (int)(Player.statLifeMax2 * Main.rand.NextFloat(0.005f, 0.011f))));
        }
    }

    private void BloodlustHeal(int damageDone) {
        int heal = damageDone;
        Player.statLife += heal;
        Player.statLife = Math.Min(Player.statLifeMax2, Player.statLife);
        Player.HealEffect(heal, true);
    }
}