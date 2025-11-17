using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class ManaCrystalShard : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => base.GetAlpha(dust, lightColor);

    public override bool Update(Dust dust) {
        dust.BasicDust();

        return false;
    }
}
