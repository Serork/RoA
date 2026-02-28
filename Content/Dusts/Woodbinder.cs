using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Woodbinder : ModDust {
    public override void SetStaticDefaults() {

    }

    public override bool Update(Dust dust) {
        dust.BasicDust();

        if (dust.customData != null && dust.customData is Player) {
            Player player9 = (Player)dust.customData;
            dust.position += player9.position - player9.oldPosition;
        }

        return false;
    }
}