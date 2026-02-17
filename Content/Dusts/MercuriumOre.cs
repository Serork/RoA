using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

public class MercuriumOre : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Platinum;
}