using Microsoft.Xna.Framework.Graphics;

using RoA.Core.Utility;

using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader;

namespace RoA.Common.VisualEffects;

sealed class VisualEffectDrawing : ILoadable {
    public void Load(Mod mod) {
        LoadHooks();
    }

    public void Unload() { }

    private void LoadHooks() {
        On_Main.DrawNPCs += Main_DrawNPCs;
        On_Main.DrawProjectiles += Main_DrawProjectiles;
        On_LegacyPlayerRenderer.DrawPlayers += On_LegacyPlayerRenderer_DrawPlayers;
        On_Main.DrawDust += Main_DrawDust;
    }

    private static void Main_DrawProjectiles(On_Main.orig_DrawProjectiles orig, Main self) {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
        VisualEffectSystem.GetLayer(VisualEffectLayer.BEHINDPROJS).Draw(Main.spriteBatch);
        Main.spriteBatch.End();

        orig(self);
    }

    private static void Main_DrawNPCs(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles) {
        ParticleRendererSettings particleSettings = new();
        if (!behindTiles) {
            DrawBehindNPCs(ref particleSettings);
        }
        else {
            DrawBehindTilesBehindNPCs();
        }

        orig(self, behindTiles);

        if (!behindTiles) {
            DrawAboveNPCs(ref particleSettings);
        }
        else {
            DrawBehindTilesAboveNPCs();
        }
    }

    private static void DrawBehindTilesBehindNPCs() {
        VisualEffectSystem.GetLayer(VisualEffectLayer.BEHINDTILESBEHINDNPCS).Draw(Main.spriteBatch);
    }

    private static void DrawBehindTilesAboveNPCs() {
    }

    private static void DrawBehindNPCs(ref ParticleRendererSettings particleSettings) {
        VisualEffectSystem.GetLayer(VisualEffectLayer.BEHINDNPCS).Draw(Main.spriteBatch);
    }

    private static void DrawAboveNPCs(ref ParticleRendererSettings particleSettings) {
        VisualEffectSystem.GetLayer(VisualEffectLayer.ABOVENPCS).Draw(Main.spriteBatch);
    }

    private void On_LegacyPlayerRenderer_DrawPlayers(On_LegacyPlayerRenderer.orig_DrawPlayers orig, LegacyPlayerRenderer self, Camera camera, System.Collections.Generic.IEnumerable<Player> players) {
        Main.spriteBatch.BeginWorld(shader: false);
        VisualEffectSystem.GetLayer(VisualEffectLayer.BEHINDPLAYERS).Draw(Main.spriteBatch);
        Main.spriteBatch.End();

        orig(self, camera, players);
    }

    private static void Main_DrawDust(On_Main.orig_DrawDust orig, Main self) {
        Main.spriteBatch.BeginWorld(shader: false);
        VisualEffectSystem.GetLayer(VisualEffectLayer.ABOVEPLAYERS).Draw(Main.spriteBatch);
        Main.spriteBatch.End();

        orig(self);

        Main.spriteBatch.BeginWorld(shader: false);
        VisualEffectSystem.GetLayer(VisualEffectLayer.ABOVEDUSTS).Draw(Main.spriteBatch);
        Main.spriteBatch.End();
    }
}