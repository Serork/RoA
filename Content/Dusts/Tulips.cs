using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class ExoticTulip : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Sand;
}

sealed class SweetTulip : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Sand;
}

sealed class WeepingTulip : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Sand;
}
