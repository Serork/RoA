using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

static class ProjectileHooks {
    public interface IDrawLikeHeldItem {
        DrawData Draw(ref Color lightColor);
    }

    private sealed class DrawProjectile : ILoadable {
        public void Load(Mod mod) {
            On_PlayerDrawLayers.DrawPlayer_28_ArmOverItem += On_PlayerDrawLayers_DrawPlayer_28_ArmOverItem; ;
        }

        private void On_PlayerDrawLayers_DrawPlayer_28_ArmOverItem(On_PlayerDrawLayers.orig_DrawPlayer_28_ArmOverItem orig, ref PlayerDrawSet drawinfo) {
            orig(ref drawinfo);

            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.ModProjectile == null) {
                    continue;
                }
                if (projectile.ModProjectile is IDrawLikeHeldItem draw) {
                    Color color = Lighting.GetColor(projectile.Center.ToTileCoordinates());
                    drawinfo.DrawDataCache.Add(draw.Draw(ref color));
                }
            }
        }

        public void Unload() { }
    }
}
