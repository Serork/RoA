using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class CactiCasterDust : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.PurpleTorch;
}