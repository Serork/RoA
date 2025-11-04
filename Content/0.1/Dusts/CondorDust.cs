using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class CondorDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.White * 0.9f * (dust.alpha / 255f);

    public override void OnSpawn(Dust dust) => UpdateType = DustID.Enchanted_Pink;
}