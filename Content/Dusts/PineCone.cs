using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class PineCone : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = 53;
}