using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class WiseDefense : ModBuff {
    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;

        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
}
