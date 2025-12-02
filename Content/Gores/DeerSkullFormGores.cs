using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Content.Gores;

class DeerSkullFormGore3 : DeerSkullFormGore1 { }

class DeerSkullFormGore2 : DeerSkullFormGore1 { }

class DeerSkullFormGore1 : ModGore {
    public override bool Update(Gore gore) {
        GoreHelper.FadeOutOverTime(gore);

        return base.Update(gore);
    }
}
