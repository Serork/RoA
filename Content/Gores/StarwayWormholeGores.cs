using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class StarwayWormholeGore4 : StarwayWormholeGore1 { }
class StarwayWormholeGore3 : StarwayWormholeGore1 { }
class StarwayWormholeGore2 : StarwayWormholeGore1 { }

class StarwayWormholeGore1 : ModGore {
    public override bool Update(Gore gore) {
        GoreHelper.FadeOutOverTime(gore);
        GoreHelper.FadeOutOverTime(gore);

        return base.Update(gore);
    }
}
