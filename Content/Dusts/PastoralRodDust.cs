using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class PastoralRodDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => lightColor * (1f - (dust.alpha / 255f));

    public override void OnSpawn(Dust dust) => UpdateType = 262;
}