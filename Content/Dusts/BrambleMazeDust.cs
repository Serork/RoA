using Terraria.ID;
using Terraria.ModLoader;

namespace RoA.Content.Dusts;

class BrambleMazeDust4 : ModDust { }
class BrambleMazeDust3 : ModDust { }
class BrambleMazeDust2 : ModDust { }

class BrambleMazeDust1 : ModDust {
    public override void SetStaticDefaults() => UpdateType = DustID.Dirt;
}
