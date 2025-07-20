using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

sealed class DruidBiomeWaterDroplet : ModGore {
    public override void SetStaticDefaults() {
        ChildSafety.SafeGore[Type] = true;
        GoreID.Sets.LiquidDroplet[Type] = true;

        UpdateType = GoreID.WaterDrip;
    }
}
