using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class AerialConcussion : ModDust {
    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        dust.velocity.Y *= 0.98f;
        dust.velocity.X *= 0.98f;

        dust.scale *= 0.98f;
        if (dust.scale < 0.1f) {
            dust.active = false;
        }

        return false;
    }
}
