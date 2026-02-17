using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class DaikatanaDust : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.BlueTorch;
}