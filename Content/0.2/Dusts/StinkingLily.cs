using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class StinkingLily : ModDust {
    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        dust.noGravity = true;

        return false;
    }
}
