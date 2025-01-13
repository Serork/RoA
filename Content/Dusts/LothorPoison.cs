using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class LothorPoison : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color result = new Color(255, 255, 255, 0)/*.MultiplyRGB(lightColor)*/ * (float)(1f - (float)dust.alpha / 255);
        return result;
    }

    public override void OnSpawn(Dust dust) => UpdateType = DustID.PoisonStaff;
}