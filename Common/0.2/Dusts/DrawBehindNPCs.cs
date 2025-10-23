using RoA.Core;
using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Dusts;
sealed class DrawBehindNPCs : IInitializer {
    void ILoadable.Load(Mod mod) {
        On_Main.DrawNPCs += On_Main_DrawNPCs;
    }

    private void On_Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        for (int i = 0; i < Main.maxDustToDraw; i++) {
            Dust dust = Main.dust[i];
            if (!dust.active)
                continue;
            ModDust modDust = DustLoader.GetDust(dust.type);
            if (modDust != null && modDust is IDrawDustPreNPCs drawDustPreNPCs) {
                drawDustPreNPCs.DrawPreNPCs(dust);
            }
        }

        orig(self, behindTiles);
    }
}
