using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class FallenLeavesBranchGore3 : FallenLeavesBranchGore1 { }

class FallenLeavesBranchGore2 : FallenLeavesBranchGore1 { }

class FallenLeavesBranchGore1 : ModGore {
    public override bool Update(Gore gore) {
        for (int i = 0; i < 3; i++) {
            GoreHelper.FadeOutOverTime(gore);
        }

        return base.Update(gore);
    }
}
