using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;
namespace RoA.Content.Dusts;

sealed class Smoke : ModDust, IPostLiquidDraw {
    public override bool Update(Dust dust) {
        DustHelper.BasicDust(dust, onlyScale: true);

        dust.velocity.Y *= 0.98f;
        dust.velocity.X *= 0.98f;
        if (dust.customData != null && dust.customData is float) {
            float num69 = (float)dust.customData;
            dust.velocity.Y += num69;
        }

        dust.position += dust.velocity;

        return false;
    }

    public override bool PreDraw(Dust dust) => false;
}
