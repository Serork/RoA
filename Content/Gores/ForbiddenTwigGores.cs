using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class ForbiddenTwig4 : ForbiddenTwig1 { }

class ForbiddenTwig3 : ForbiddenTwig1 { }

class ForbiddenTwig2 : ForbiddenTwig1 { }

class ForbiddenTwig1 : ModGore {
    public override bool Update(Gore gore) {
        GoreHelper.FadeOutOverTime(gore);

        return base.Update(gore);
    }
}
