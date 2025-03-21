using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class RavencallersCloakBuff : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
        Main.debuff[Type] = true;
    }
}