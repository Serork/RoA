using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Taproot1 : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Corruption;
}

sealed class Taproot2 : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Corruption;
}