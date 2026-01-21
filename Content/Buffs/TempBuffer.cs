using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class TempBuffer : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.buffTime[buffIndex]++;
    }
}
