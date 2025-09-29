using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class HallowBlessing : ModBuff {
    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.lifeRegen += 5;
        player.endurance += 0.2f;
    }
}
