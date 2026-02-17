using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class MercuriumDust2 : ModDust {
    public override void SetStaticDefaults() {
        UpdateType = DustID.Platinum;
    }
}
