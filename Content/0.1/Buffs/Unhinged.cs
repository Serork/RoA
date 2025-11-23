using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Unhinged : ModBuff {
    public override void Update(Player player, ref int buffIndex) {
        player.GetAttackSpeed(DamageClass.Generic) += 0.15f;
    }
}
