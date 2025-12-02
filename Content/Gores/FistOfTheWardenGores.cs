using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class RootOfTheWardenGore3 : FistOfTheWardenGore1 { }

class RootOfTheWardenGore2 : FistOfTheWardenGore1 { }

class RootOfTheWardenGore1 : FistOfTheWardenGore1 { }


class FistOfTheWardenGore4 : FistOfTheWardenGore1 { }

class FistOfTheWardenGore3 : FistOfTheWardenGore1 { }

class FistOfTheWardenGore2 : FistOfTheWardenGore1 { }

class FistOfTheWardenGore1 : ModGore {
    public override bool Update(Gore gore) {
        GoreHelper.FadeOutOverTime(gore);
        GoreHelper.FadeOutOverTime(gore);

        return base.Update(gore);
    }
}
