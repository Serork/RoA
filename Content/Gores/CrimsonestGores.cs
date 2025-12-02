using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class CrimsonestGore5 : CrimsonestGore1 { }

class CrimsonestGore4 : CrimsonestGore1 { }

class CrimsonestGore3 : CrimsonestGore1 { }

class CrimsonestGore2 : CrimsonestGore1 { }

class CrimsonestGore1 : ModGore {

    public override bool Update(Gore gore) {
        GoreHelper.FadeOutOverTime(gore);

        return base.Update(gore);
    }
}
