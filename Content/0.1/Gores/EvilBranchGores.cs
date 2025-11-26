using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class EvilBranchGore23 : EvilBranchGore11 { }
class EvilBranchGore22 : EvilBranchGore11 { }
class EvilBranchGore21 : EvilBranchGore11 { }

class EvilBranchGore13 : EvilBranchGore11 { }
class EvilBranchGore12 : EvilBranchGore11 { }
class EvilBranchGore11 : ModGore {
    public override bool Update(Gore gore) {
        GoreHelper.FadeOutOverTime(gore);

        return base.Update(gore);
    }
}