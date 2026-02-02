using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class SunflowerBuff : ModBuff {
    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.moveSpeed += 0.2f;
        player.lifeRegen += 2;
    }
}
