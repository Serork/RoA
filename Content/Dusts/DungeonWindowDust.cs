using Microsoft.Xna.Framework;

using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class DungeonWindowDust : ModDust {
    public override Color? GetAlpha(Dust dust, Color lightColor) => base.GetAlpha(dust, lightColor) * Tiles.Decorations.DungeonWindow.OPACITY;

    public override void OnSpawn(Dust dust) => UpdateType = DustID.Glass;
}
