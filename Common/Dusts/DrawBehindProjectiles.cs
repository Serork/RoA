using RoA.Core.Utility;

using Terraria;
using Terraria.ModLoader;

namespace RoA.Common.Dusts;

sealed class DrawBehindProjectile : ILoadable {
    void ILoadable.Load(Mod mod) {
        On_Main.DrawProjectiles += On_Main_DrawProjectiles;
    }

    void ILoadable.Unload() {
    }

    private void On_Main_DrawProjectiles(On_Main.orig_DrawProjectiles orig, Main self) {
        Main.spriteBatch.BeginWorld(shader: false);
        for (int i = 0; i < Main.maxDustToDraw; i++) {
            Dust dust = Main.dust[i];
            if (!dust.active)
                continue;
            ModDust modDust = DustLoader.GetDust(dust.type);
            if (modDust != null && modDust is IDrawDustPreProjectiles drawDustPreProjectiles) {
                drawDustPreProjectiles.DrawPreProjectiles(dust);
            }
        }
        Main.spriteBatch.End();

        orig(self);
    }
}
