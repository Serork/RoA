using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class TreeDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => Color.Lerp(Color.White, lightColor, 0.8f);

    public override void OnSpawn(Dust dust) => UpdateType = DustID.Demonite;
}