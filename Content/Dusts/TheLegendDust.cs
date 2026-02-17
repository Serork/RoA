using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class TheLegendDust : ModDust {
    public override void SetStaticDefaults() {
        UpdateType = DustID.Shadewood;
    }

    public override void OnSpawn(Dust dust) {
        dust.scale *= 0.75f;
    }
}
