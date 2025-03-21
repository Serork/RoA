using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Root : ModBuff {
    public override void SetStaticDefaults() {
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex) {
        player.velocity.X *= 0.5f;
        player.controlLeft = player.controlRight = player.controlUp = player.controlDown = false;
        player.controlJump = false;
        //player.controlUseItem = false;
    }
}