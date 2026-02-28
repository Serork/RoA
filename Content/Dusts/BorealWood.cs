using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class BorealWood : ModDust {
    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust);

        dust.noGravity = true;

        if (dust.customData != null && dust.customData is Player) {
            Player player9 = (Player)dust.customData;
            dust.position += player9.position - player9.oldPosition;
        }

        return false;
    }
}
