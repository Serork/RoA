using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

sealed class CactiGore : ModGore {
    public override bool Update(Gore gore) {
        GoreHelper.FadeOutOverTime(gore);

        return base.Update(gore);
    }
}
