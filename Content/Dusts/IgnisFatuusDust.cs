using Microsoft.Xna.Framework;

using RoA.Core.Utility;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class IgnisFatuusDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White with { A = 200 } * 0.75f;

    public override bool Update(Dust dust) {
        dust.BasicDust();

        dust.frame.X = 10 * (int)dust.customData;
        dust.frame.Y = 10 * Main.rand.Next(3);

        return false;
    }
}
