using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Crystallized : ModBuff {
    public override void SetStaticDefaults() {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
        Main.buffNoTimeDisplay[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.noItems = true;
        player.cursed = true;
    }
}
