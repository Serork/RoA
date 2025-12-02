using RoA.Common.Druid;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Buffs;

sealed class Willpower : ModBuff {
    public override void SetStaticDefaults() {
        // DisplayName.SetDefault("Willpower");
        // Description.SetDefault("Wreath is charged 10% faster");
    }

    public override void Update(Player player, ref int buffIndex) => player.GetModPlayer<DruidStats>().WreathChargeRateMultiplier += 0.15f;
}