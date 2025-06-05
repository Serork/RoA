using RoA.Common.Druid;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Resilience : ModBuff {
    public override void SetStaticDefaults() {
        //DisplayName.SetDefault("Resilience");
        //Description.SetDefault("Wreath takes longer to start discharging");
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<DruidStats>().DischargeTimeDecreaseMultiplier -= 0.8f;
}