using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Woodbinder : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = ModContent.DustType<Woodbinder>();
}