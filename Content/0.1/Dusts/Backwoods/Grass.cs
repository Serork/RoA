using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts.Backwoods;

sealed class Grass : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = DustID.Grass;
}