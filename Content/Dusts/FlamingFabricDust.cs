using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class FlamingFabricDust : ModDust {
    public override void SetStaticDefaults() => UpdateType = 78;
}