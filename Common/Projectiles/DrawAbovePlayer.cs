using Microsoft.Xna.Framework.Graphics;

using System;

using Terraria;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

sealed class DrawAbovePlayer : ILoadable {
    void ILoadable.Load(Mod mod) {
        On_LegacyPlayerRenderer.DrawPlayers += On_LegacyPlayerRenderer_DrawPlayers;
    }

    void ILoadable.Unload() {
    }

    private void On_LegacyPlayerRenderer_DrawPlayers(On_LegacyPlayerRenderer.orig_DrawPlayers orig, LegacyPlayerRenderer self, Camera camera, System.Collections.Generic.IEnumerable<Player> players) {
        orig(self, camera, players);

        SpriteBatch spriteBatch = Main.spriteBatch;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        for (int j = 0; j < 1000; j++) {
            if (Main.projectile[j].active && Main.projectile[j].type > 0 && !Main.projectile[j].hide && Main.projectile[j].ModProjectile is IDrawProjectileAbovePlayer drawProjectileAbovePlayer) {
                drawProjectileAbovePlayer.DrawAbovePlayer(Main.projectile[j]);
            }
        }
        spriteBatch.End();
    }
}