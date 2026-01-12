using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class FallenLeavesBranchDust : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.WoodFurniture;
}
