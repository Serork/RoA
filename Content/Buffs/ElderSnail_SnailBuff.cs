using RoA.Core.Utility.Vanilla;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;
sealed class SnailBuff : ModBuff {
    public override void Update(Player player, ref int buffIndex) {
        ushort defenseIncrease = (ushort)(10 + (player.GetCommon().DefenseLastTick - 10) / 10);
        player.statDefense += defenseIncrease;
    }
}
