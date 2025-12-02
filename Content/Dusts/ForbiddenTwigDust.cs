using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class ForbiddenTwigDust : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.WoodFurniture;
}
