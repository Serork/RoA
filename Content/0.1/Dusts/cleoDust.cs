using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class cleoDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.Lerp(lightColor, new Color(255, 255, 255, 0), 0.85f);

    public override void OnSpawn(Dust dust) => UpdateType = 107;
}