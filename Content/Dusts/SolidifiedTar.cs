using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class SolidifiedTar : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = DustID.Dirt;
}
