using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

public class MercuriumOre : ModDust {
    public override void OnSpawn(Dust dust) => UpdateType = DustID.Platinum;
}