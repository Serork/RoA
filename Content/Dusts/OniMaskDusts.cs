using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

class OniMaskDust1 : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = DustID.Shadewood;
}

sealed class OniMaskDust2 : OniMaskDust1 { }