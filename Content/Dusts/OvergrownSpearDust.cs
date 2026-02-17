using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class OvergrownSpearDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.Lerp(lightColor, new Color(255, 255, 255, 0) * 0.9f, 0.75f);

    public override void SetStaticDefaults() => UpdateType = 107;
}