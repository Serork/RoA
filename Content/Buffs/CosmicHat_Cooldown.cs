using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class CosmicHat_Cooldown : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
    }
}