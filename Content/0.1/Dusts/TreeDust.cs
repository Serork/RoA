using Microsoft.Xna.Framework;

using RoA.Common.WorldEvents;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class TreeDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor)
        => Color.Lerp(Color.Lerp(lightColor, Color.White * 0.9f, BackwoodsFogHandler.Opacity > 0f ? MathHelper.Clamp(BackwoodsFogHandler.Opacity * 1.35f, 0f, 1f) : 0f), lightColor, 0.8f);

    public override void OnSpawn(Dust dust) => UpdateType = DustID.Demonite;
}