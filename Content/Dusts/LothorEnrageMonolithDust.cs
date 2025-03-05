using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

class LothorEnrageMonolithDust : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = 1;
}