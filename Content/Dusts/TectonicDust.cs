using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class TectonicDust : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = 25;
}