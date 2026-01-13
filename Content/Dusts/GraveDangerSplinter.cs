using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class GraveDangerSplinter : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Bone;
}
