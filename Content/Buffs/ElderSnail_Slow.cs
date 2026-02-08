using RoA.Common.Sets;
using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class ElderSnailSlow : ModBuff {
    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;

        BuffID.Sets.LongerExpertDebuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.GetCommon().ElderSnailSlow = true;
    }
}
