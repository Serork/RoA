using Microsoft.Xna.Framework;

using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace RoA.Common.Projectiles;

static class ProjectileHooks {
    public interface IDrawLikeHeldItem {
        void Draw(ref Color lightColor, PlayerDrawSet drawinfo);
    }

    private class DrawProjectileInFrontOfHand : ILoadable {
        public void Load(Mod mod) {
            On_PlayerDrawLayers.DrawPlayer_11_Balloons += On_PlayerDrawLayers_DrawPlayer_11_Balloons;
        }

        private void On_PlayerDrawLayers_DrawPlayer_11_Balloons(On_PlayerDrawLayers.orig_DrawPlayer_11_Balloons orig, ref PlayerDrawSet drawinfo) {
            orig(ref drawinfo);

            foreach (Projectile projectile in Main.ActiveProjectiles) {
                if (projectile.ModProjectile == null) {
                    continue;
                }
                if (projectile.ModProjectile is IDrawLikeHeldItem draw) {
                    Color color = Lighting.GetColor(projectile.Center.ToTileCoordinates());
                    draw.Draw(ref color, drawinfo);
                }
            }
        }

        public void Unload() { }
    }
}
