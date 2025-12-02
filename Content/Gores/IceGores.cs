using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class IceGore3 : IceGore1 { }

class IceGore2 : IceGore1 { }

class IceGore1 : ModGore {
    public override bool Update(Gore gore) {
        GoreHelper.FadeOutOverTime(gore);

        return base.Update(gore);
    }
}
