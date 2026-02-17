using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Woodbinder : ModDust {
    public override void SetStaticDefaults() => UpdateType = ModContent.DustType<Woodbinder>();
}