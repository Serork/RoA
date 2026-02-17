using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

sealed class Hemorrhage2 : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.ScourgeOfTheCorruptor;
}