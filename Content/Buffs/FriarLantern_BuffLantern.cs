using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class BuffLantern : ModBuff {
    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.GetCommon().IsFriarLanternBuffEffectActive = true;

        player.aggro -= 400;
    }
}
