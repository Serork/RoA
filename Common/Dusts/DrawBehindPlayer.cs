using RoA.Core.Utility;

using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Common.Dusts;

sealed class DrawBehindPlayer : ILoadable {
    void ILoadable.Load(Mod mod) {
        On_LegacyPlayerRenderer.DrawPlayers += On_LegacyPlayerRenderer_DrawPlayers;
    }

    void ILoadable.Unload() {
    }

    private void On_LegacyPlayerRenderer_DrawPlayers(On_LegacyPlayerRenderer.orig_DrawPlayers orig, LegacyPlayerRenderer self, Camera camera, System.Collections.Generic.IEnumerable<Player> players) {
        Main.spriteBatch.BeginWorld(shader: false);
        for (int i = 0; i < Main.maxDustToDraw; i++) {
            Dust dust = Main.dust[i];
            if (!dust.active)
                continue;
            ModDust modDust = DustLoader.GetDust(dust.type);
            if (modDust != null && modDust is IDrawDustPrePlayer drawDustPostPlayer) {
                drawDustPostPlayer.DrawPrePlayer(dust);
            }
        }
        Main.spriteBatch.End();

        orig(self, camera, players);
    }
}
