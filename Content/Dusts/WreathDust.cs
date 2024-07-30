using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class WreathDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) {
        Color color = dust.color;
        return color;
    }
    
    public override void OnSpawn(Dust dust) => UpdateType = DustID.FireworksRGB;
}