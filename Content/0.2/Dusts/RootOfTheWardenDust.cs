using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class RootOfTheWardenDust : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.WoodFurniture;
}
