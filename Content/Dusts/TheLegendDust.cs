using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class TheLegendDust : ModDust {
    public override void OnSpawn(Dust dust) {
        UpdateType = DustID.Shadewood;
        dust.scale *= 0.75f;
    }
}
