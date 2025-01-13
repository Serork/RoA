using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class LothorPoison : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White.MultiplyRGB(lightColor);

    public override void OnSpawn(Dust dust) => UpdateType = DustID.PoisonStaff;
}